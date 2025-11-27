using Microsoft.Extensions.Hosting;
using QRCoder;
using ShopInspector.Application.Interfaces;

namespace ShopInspector.Infrastructure.Repositories;

public class QRCodeRepository : IQRCodeRepository
{
    private readonly IWebHostEnvironment _env;
    private readonly AzureBlobService _blobService;
    public QRCodeRepository(IWebHostEnvironment env, AzureBlobService blobService)
    {
        _env = env;
        _blobService = blobService;
    }

    public async Task<(byte[] PngBytes, string? SavedPath)> GenerateAsync(
        string url, int? assetId = null, bool saveToDisk = false)
    {
        // Generate QR data
        QRCodeGenerator generator = new QRCodeGenerator();
        QRCodeData qrData = generator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);

        // *** IMPORTANT: Use PNG BYTE QR CODE (CROSS-PLATFORM) ***
        PngByteQRCode qrCode = new PngByteQRCode(qrData);
        byte[] bytes = qrCode.GetGraphic(20);

        string? savedPath = null;  
        string? publicUrl = null;

        if (saveToDisk)
        {
            var folder = Path.Combine(_env.WebRootPath, "qrcodes");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var fileName = assetId.HasValue
                ? $"asset_{assetId.Value}.png" 
                : $"qr_{Guid.NewGuid()}.png";

            var fullPath = Path.Combine(folder, fileName);
             publicUrl = await _blobService.UploadAsync(bytes, fileName);
            // Save the PNG bytes
            await File.WriteAllBytesAsync(fullPath, bytes);

            savedPath = $"/qrcodes/{fileName}";
        }

        return (bytes, publicUrl);
    }
}
