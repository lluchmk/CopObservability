namespace DocumentsGenerator;

public class ResumeGenerator
{
    private readonly OrdersClient _ordersClient;
    private readonly CatalogClient _catalogClient;
    private readonly CustomersClient _customersClient;
    private readonly RequestsMetrics _requestMetrics;

    public ResumeGenerator(OrdersClient ordersClient, CatalogClient catalogClient, CustomersClient customersClient, RequestsMetrics requestsMetrics)
    {
        _ordersClient = ordersClient;
        _catalogClient = catalogClient;
        _customersClient = customersClient;
        _requestMetrics = requestsMetrics;
    }

    public async Task CreateCustomersResume(IEnumerable<string> customers, StreamWriter fileWriter)
    {
        await fileWriter.WriteLineAsync("Customers Summary;");
        await fileWriter.WriteLineAsync(";");

        foreach (var customer in customers)
        {
            var customerData = await _customersClient.GetCustomer(customer);
            if (customerData is null)
            {
                await fileWriter.WriteLineAsync($"{customer};Not Found;");
                continue;
            }

            _requestMetrics.IncreaseCustomersMeter(customerData.Name);

            var customerOrders = await _ordersClient.GetOrdersByCustomer(customerData.Id);
            if (customerOrders is null || !customerOrders.Any())
            {
                await fileWriter.WriteLineAsync($"{customer};No orders;");
                continue;
            }

            await fileWriter.WriteLineAsync($"{customerData.Name};");
            await fileWriter.WriteLineAsync($"Product;Amount;");

            foreach (var order in customerOrders.GroupBy(o => o.ProductId))
            {
                var product = await _catalogClient.GetProduct(order.Key);
                if (product is null)
                    continue;
                var totalAmount = order.Sum(o => o.Amount);
                fileWriter.WriteLine($"{product.Name};{totalAmount};");
            }
        }
    }

    public async Task CreateProductsResume(IEnumerable<string> products, StreamWriter fileWriter)
    {
        await fileWriter.WriteLineAsync("Produts Summary;");
        await fileWriter.WriteLineAsync(";");

        foreach (var product in products)
        {
            var productData = await _catalogClient.GetProduct(product);
            if (productData is null)
            {
                await fileWriter.WriteLineAsync($"{product};Not Found;");
                continue;
            }

            _requestMetrics.IncreaseProductsMeter(productData.Name);

            var productOrders = await _ordersClient.GetOrdersByProduct(productData.Id);
            if (productOrders is null || !productOrders.Any())
            {
                await fileWriter.WriteLineAsync($"{product};No orders;");
                continue;
            }

            await fileWriter.WriteLineAsync($"{productData.Name};{productData.Description}");
            await fileWriter.WriteLineAsync($"Customer;Amount;");

            foreach (var order in productOrders.GroupBy(o => o.CustomerId))
            {
                var customer = await _customersClient.GetCustomer(order.Key);
                if (customer is null)
                    continue;
                var totalAmount = order.Sum(o => o.Amount);
                fileWriter.WriteLine($"{customer.Name};{totalAmount};");
            }
        }
    }
}
