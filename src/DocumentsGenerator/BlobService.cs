using Azure.Storage.Blobs;

namespace DocumentsGenerator;

public class BlobService
{
    private readonly ILogger<BlobService> _logger;
    private readonly BlobServiceClient _blobServiceClient;

    public BlobService(ILogger<BlobService> logger, BlobServiceClient blobServiceClient)
    {
        _logger = logger;
        _blobServiceClient = blobServiceClient;
    }

    public async Task<string> UploadBlob(string filePath)
    {
        _logger.UploadingToBlob();

        var containerClient = _blobServiceClient.GetBlobContainerClient("documents");
        await containerClient.CreateIfNotExistsAsync();
        var blobClient = containerClient.GetBlobClient(filePath);
        await blobClient.UploadAsync(filePath);

        return blobClient.Uri.AbsoluteUri;
    }
}
