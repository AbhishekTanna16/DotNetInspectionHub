using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

public class AzureBlobService
{
    private readonly string _connectionString;
    private readonly string _containerName;

    public AzureBlobService(IConfiguration config)
    {
        _connectionString = config["AzureBlob:ConnectionString"];
        _containerName = config["AzureBlob:ContainerName"];
    }

    public async Task<string> UploadAsync(byte[] fileBytes, string fileName)
    {
        // Create client for container
        var containerClient = new BlobContainerClient(_connectionString, _containerName);

        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var blobClient = containerClient.GetBlobClient(fileName);

        using var ms = new MemoryStream(fileBytes);

        await blobClient.UploadAsync(ms, overwrite: true);

        return blobClient.Uri.ToString(); // Returns public URL
    }
}
