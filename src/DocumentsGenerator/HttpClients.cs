using System.Net.Http.Json;

namespace DocumentsGenerator;

public class OrdersClient
{
    private readonly HttpClient _httpClient;

    public OrdersClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<Order>?> GetOrdersByCustomer(Guid customerId)
        => await _httpClient.GetFromJsonAsync<IEnumerable<Order>>($"customers/{customerId}");

    public async Task<IEnumerable<Order>?> GetOrdersByProduct(Guid productId)
        => await _httpClient.GetFromJsonAsync<IEnumerable<Order>>($"products/{productId}");
}

public class CustomersClient
{
    private readonly HttpClient _httpClient;

    public CustomersClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Customer?> GetCustomer(Guid customerId)
        => await _httpClient.GetFromJsonAsync<Customer>($"{customerId}");

    public async Task<Customer?> GetCustomer(string customerName)
        => await _httpClient.GetFromJsonAsync<Customer>($"{customerName}");
}

public class CatalogClient
{
    private readonly HttpClient _httpClient;

    public CatalogClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Product?> GetProduct(Guid productId)
        => await _httpClient.GetFromJsonAsync<Product>($"{productId}");

    public async Task<Product?> GetProduct(string productName)
        => await _httpClient.GetFromJsonAsync<Product>($"{productName}");
}
