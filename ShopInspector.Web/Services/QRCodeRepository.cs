using Microsoft.Extensions.Hosting;
using QRCoder;
using ShopInspector.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Infrastructure.Repositories;
public class QRCodeRepository : IQRCodeRepository
{
    private readonly IWebHostEnvironment _env;

    public QRCodeRepository(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<(byte[] PngBytes, string? SavedPath)> GenerateAsync(
        string url, int? assetId = null, bool saveToDisk = false)
    {
        // Generate QR data
        using var generator = new QRCodeGenerator();
        using var qrData = generator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new QRCode(qrData);
        using Bitmap bitmap = qrCode.GetGraphic(20);

        // Convert to PNG bytes
        using var ms = new MemoryStream();
        bitmap.Save(ms, ImageFormat.Png);
        var bytes = ms.ToArray();

        string? savedPath = null;

        if (saveToDisk)
        {
            var folder = Path.Combine(_env.WebRootPath, "qrcodes");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            var fileName = assetId.HasValue
                ? $"asset_{assetId.Value}.png"
                : $"qr_{Guid.NewGuid()}.png";

            var fullPath = Path.Combine(folder, fileName);
            await File.WriteAllBytesAsync(fullPath, bytes);

            savedPath = $"/qrcodes/{fileName}";
        }

        return (bytes, savedPath);
    }
}