using Azure.Storage.Blobs;
using Messaging;
using Microsoft.Data.SqlClient;
using System.Data.SqlTypes;

namespace DocumentsGenerator;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly RequestsMetrics _requestMetrics;
    private readonly IServiceProvider _serviceProvider;
    private readonly MessageReceiver _messageReceiver;

    public Worker(ILogger<Worker> logger, RequestsMetrics requestsMetrics, IServiceProvider serviceProvider, MessageReceiver messageReceiver)
    {
        _logger = logger;
        _requestMetrics = requestsMetrics;
        _serviceProvider = serviceProvider;
        _messageReceiver = messageReceiver;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        _messageReceiver.StartConsumer<CreateDocumentMessage>(ProcessMessage);

        return Task.CompletedTask;
    }

    private async Task ProcessMessage(CreateDocumentMessage request)
    {
        try
        {
            await ProcessDocument(request);
        }
        catch
        {
            await UpdateDocumentStatus(request.DocumentId, DocumentStatus.Failed);
            throw;
        }
    }

    private async Task ProcessDocument(CreateDocumentMessage request)
    {
        if (request.Errors?.Any(e => "ProcessStart".Equals(e, StringComparison.OrdinalIgnoreCase)) ?? false)
        {
            throw new Exception("Error at the start of message processing.");
        }

        if ((!request.Customers?.Any() ?? false) && (!request.Products?.Any() ?? false))
        {
            await UpdateDocumentStatus(request.DocumentId, DocumentStatus.Completed);
            _logger.EmptyRequestReceived();
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();

        await UpdateDocumentStatus(request.DocumentId, DocumentStatus.Processing);

        var filePath = $"{timeProvider.GetUtcNow().ToString("yyyy-MM-dd-HH-mm-ss")}_${request.DocumentId}.csv";
        try
        {
            try
            {
                _logger.CreatingFile(filePath);
                using var fileWriter = new StreamWriter(new FileStream(filePath, FileMode.Create, FileAccess.Write));

                if (request.Customers?.Any() ?? false)
                {
                    await CreateCustomersResume(request.Customers, fileWriter);
                }

                if (request.Products?.Any() ?? false)
                {
                    await CreateProductsResume(request.Products, fileWriter);
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

                if (request.Errors?.Any(e => "ProcessUpload".Equals(e, StringComparison.OrdinalIgnoreCase)) ?? false)
                {
                    throw new Exception("Error when uploading the document.");
                }

                var blobServiceClient = scope.ServiceProvider.GetRequiredService<BlobServiceClient>();
                var containerClient = blobServiceClient.GetBlobContainerClient("documents");
                await containerClient.CreateIfNotExistsAsync();
                var blobClient = containerClient.GetBlobClient(filePath);
                await blobClient.UploadAsync(filePath);

                await UpdateDocumentStatus(request.DocumentId, DocumentStatus.Completed, blobClient.Uri.AbsoluteUri);
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

        if (request.Errors?.Any(e => "ProcessEnd".Equals(e, StringComparison.OrdinalIgnoreCase)) ?? false)
        {
            throw new Exception("Error at the end of message processing.");
        }
    }

    private async Task CreateCustomersResume(IEnumerable<string> customers, StreamWriter fileWriter)
    {
        var ordersClient = _serviceProvider.GetRequiredService<OrdersClient>();
        var catalogClient = _serviceProvider.GetRequiredService<CatalogClient>();
        var customersClient = _serviceProvider.GetRequiredService<CustomersClient>();

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

            _requestMetrics.IncreaseCustomersMeter(customerData.Name);

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

    private async Task CreateProductsResume(IEnumerable<string> products, StreamWriter fileWriter)
    {
        var ordersClient = _serviceProvider.GetRequiredService<OrdersClient>();
        var catalogClient = _serviceProvider.GetRequiredService<CatalogClient>();
        var customersClient = _serviceProvider.GetRequiredService<CustomersClient>();

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

            _requestMetrics.IncreaseProductsMeter(productData.Name);

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

    private async Task UpdateDocumentStatus(Guid documentId, DocumentStatus status, string? downloadUrl = null)
    {
        var connectionString = _serviceProvider.GetRequiredService<IConfiguration>()
            .GetConnectionString("Database");

        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            UPDATE Documents 
            SET Status = @status, DownloadUrl = @downloadUrl
            WHERE Id = @documentId", connection);

        command.Parameters.AddWithValue("@status", status);
        command.Parameters.AddWithValue("@documentId", documentId);
        command.Parameters.AddWithValue("@downloadUrl", downloadUrl ?? SqlString.Null);

        await command.ExecuteNonQueryAsync();

    }
}
