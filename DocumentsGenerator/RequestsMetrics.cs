using System.Diagnostics.Metrics;

namespace DocumentsGenerator;

public class RequestsMetrics
{
    private readonly Counter<int> _customersCounter;
    private readonly Counter<int> _productsCounter;

    public RequestsMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("documentsgenerator");
        _customersCounter = meter.CreateCounter<int>("documentsgenerator.customers", description: "Counts the amount of times reports are generated for a customer");
        _productsCounter = meter.CreateCounter<int>("documentsgenerator.products", description: "Counts the amount of times reports are generated for a product");
    }

    public void IncreaseCustomersMeter(string customerName)
    {
        _customersCounter.Add(1, new KeyValuePair<string, object?>("customerName", customerName));
    }

    public void IncreaseProductsMeter(string productName)
    {
        _productsCounter.Add(1, new KeyValuePair<string, object?>("productName", productName));
    }
}
