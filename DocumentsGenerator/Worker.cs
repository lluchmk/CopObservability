using Messaging;

namespace DocumentsGenerator;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly MessageReceiver _messageReceiver;

    public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, MessageReceiver messageReceiver)
    {
        _logger = logger;
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
        using var scope = _serviceProvider.CreateScope();
        var documentsRepository = scope.ServiceProvider.GetRequiredService<DocumentsRepository>();

        try
        {
            await ProcessDocument(scope, documentsRepository, request);
        }
        catch
        {
            await documentsRepository.UpdateDocumentStatus(request.DocumentId, DocumentStatus.Failed);
            throw;
        }
    }

    private async Task ProcessDocument(IServiceScope scope, DocumentsRepository documentsRepository, CreateDocumentMessage request)
    {
        if (request.Errors?.Any(e => "ProcessStart".Equals(e, StringComparison.OrdinalIgnoreCase)) ?? false)
        {
            throw new Exception("Error at the start of message processing.");
        }

        if ((!request.Customers?.Any() ?? false) && (!request.Products?.Any() ?? false))
        {
            await documentsRepository.UpdateDocumentStatus(request.DocumentId, DocumentStatus.Completed);
            _logger.EmptyRequestReceived();
            return;
        }

        await documentsRepository.UpdateDocumentStatus(request.DocumentId, DocumentStatus.Processing);

        var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();
        var filePath = $"{timeProvider.GetUtcNow().ToString("yyyy-MM-dd-HH-mm-ss")}_${request.DocumentId}.csv";
        try
        {
            try
            {
                await CreateResume(filePath, request, scope);
            }
            catch (Exception ex)
            {
                _logger.ErrorCreatingFile(ex.Message);
                throw;
            }

            try
            {
                var blobService = scope.ServiceProvider.GetRequiredService<BlobService>();
                var blobUri = await blobService.UploadBlob(filePath);
                await documentsRepository.UpdateDocumentStatus(request.DocumentId, DocumentStatus.Completed, blobUri);
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

    private async Task CreateResume(string filePath, CreateDocumentMessage request, IServiceScope scope)
    {
        try
        {
            _logger.CreatingFile(filePath);
            var resumeGenerator = scope.ServiceProvider.GetRequiredService<ResumeGenerator>();
            using var fileWriter = new StreamWriter(new FileStream(filePath, FileMode.Create, FileAccess.Write));

            if (request.Customers?.Any() ?? false)
            {
                await resumeGenerator.CreateCustomersResume(request.Customers, fileWriter);
            }

            if (request.Products?.Any() ?? false)
            {
                await resumeGenerator.CreateProductsResume(request.Products, fileWriter);
            }
        }
        catch (Exception ex)
        {
            _logger.ErrorCreatingFile(ex.Message);
            throw;
        }
    }
}
