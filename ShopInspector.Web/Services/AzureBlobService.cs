using Azure.Storage.Blobs;

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
        var containerClient = new BlobContainerClient(_connectionString, _containerName);

        // Storage account has public access disabled → remove PublicAccessType
        await containerClient.CreateIfNotExistsAsync();

        var blobClient = containerClient.GetBlobClient(fileName);

        using var ms = new MemoryStream(fileBytes);

        await blobClient.UploadAsync(ms, overwrite: true);

        return blobClient.Uri.ToString();
    }
}
