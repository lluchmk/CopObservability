using Azure.Storage.Blobs;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace DocumentsGenerator;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    private IConnection? _connection;
    private IModel? _channel;

    public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["MessageQueue:HostName"],
            DispatchConsumersAsync = true,
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(_configuration["MessageQueue:QueueName"], false, false);

        return base.StartAsync(cancellationToken);
    }

    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());
            var request = JsonSerializer.Deserialize<CreateDocumentMessage>(message);
            if (request is null)
            {
                _logger.InvalidMessageReceived(message);
                return;
            }

            _logger.MessageReceived(request);
            await ProcessMessage(request);
        };

        _channel.BasicConsume(_configuration["MessageQueue:QueueName"], true, consumer);

        return Task.CompletedTask;
    }

    private async Task ProcessMessage(CreateDocumentMessage request)
    {
        if ((!request.Customers?.Any() ?? false) && (!request.Products?.Any() ?? false))
        {
            _logger.EmptyRequestReceived();
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();

        var filePath = $"{timeProvider.GetUtcNow().ToString("yyyy-MM-dd-HH-mm-ss")}_Summary.csv";
        try
        {
            try
            {
                using (var fileWriter = new StreamWriter(new FileStream(filePath, FileMode.Create, FileAccess.Write)))
                {
                    _logger.CreatingFile(filePath);

                    if (request.Customers?.Any() ?? false)
                    {
                        await CreateCustomersResume(request.Customers, scope.ServiceProvider, fileWriter);
                    }

                    if (request.Products?.Any() ?? false)
                    {
                        await CreateProductsResume(request.Products, scope.ServiceProvider, fileWriter);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorCreatingFile(ex.Message);
                throw;
            }

            try
            {
                _logger.UploadingToBlob();

                var blobServiceClient = scope.ServiceProvider.GetRequiredService<BlobServiceClient>();
                var containerClient = blobServiceClient.GetBlobContainerClient("documents");
                await containerClient.CreateIfNotExistsAsync();
                var blobClient = containerClient.GetBlobClient(filePath);
                await blobClient.UploadAsync(filePath);
            }
            catch (Exception ex)
            {
                _logger.ErrorUploadingFile(ex.Message);
                throw;
            }
        }
        finally
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }

    private async Task CreateCustomersResume(IEnumerable<string> customers, IServiceProvider serviceProvider, StreamWriter fileWriter)
    {
        var ordersClient = serviceProvider.GetRequiredService<OrdersClient>();
        var catalogClient = serviceProvider.GetRequiredService<CatalogClient>();
        var customersClient = serviceProvider.GetRequiredService<CustomersClient>();

        await fileWriter.WriteLineAsync("Customers Summary;");
        await fileWriter.WriteLineAsync(";");

        foreach (var customer in customers)
        {
            var customerData = await customersClient.GetCustomer(customer);
            if (customerData is null)
            {
                await fileWriter.WriteLineAsync($"{customer};Not Found;");
                continue;
            }

            var customerOrders = await ordersClient.GetOrdersByCustomer(customerData.Id);
            if (customerOrders is null || !customerOrders.Any())
            {
                await fileWriter.WriteLineAsync($"{customer};No orders;");
                continue;
            }

            await fileWriter.WriteLineAsync($"{customerData.Name};");
            await fileWriter.WriteLineAsync($"Product;Amount;");

            foreach (var order in customerOrders.GroupBy(o => o.ProductId))
            {
                var product = await catalogClient.GetProduct(order.Key);
                if (product is null)
                    continue;
                var totalAmount = order.Sum(o => o.Amount);
                fileWriter.WriteLine($"{product.Name};{totalAmount};");
            }
        }
    }

    private async Task CreateProductsResume(IEnumerable<string> products, IServiceProvider serviceProvider, StreamWriter fileWriter)
    {
        var ordersClient = serviceProvider.GetRequiredService<OrdersClient>();
        var catalogClient = serviceProvider.GetRequiredService<CatalogClient>();
        var customersClient = serviceProvider.GetRequiredService<CustomersClient>();

        await fileWriter.WriteLineAsync("Produts Summary;");
        await fileWriter.WriteLineAsync(";");

        foreach (var product in products)
        {
            var productData = await catalogClient.GetProduct(product);
            if (productData is null)
            {
                await fileWriter.WriteLineAsync($"{product};Not Found;");
                continue;
            }

            var productOrders = await ordersClient.GetOrdersByProduct(productData.Id);
            if (productOrders is null || !productOrders.Any())
            {
                await fileWriter.WriteLineAsync($"{product};No orders;");
                continue;
            }

            await fileWriter.WriteLineAsync($"{productData.Name};{productData.Description}");
            await fileWriter.WriteLineAsync($"Customer;Amount;");

            foreach (var order in productOrders.GroupBy(o => o.CustomerId))
            {
                var customer = await customersClient.GetCustomer(order.Key);
                if (customer is null)
                    continue;
                var totalAmount = order.Sum(o => o.Amount);
                fileWriter.WriteLine($"{customer.Name};{totalAmount};");
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        _connection?.Close();
    }
}
