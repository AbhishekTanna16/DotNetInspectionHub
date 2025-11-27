using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using ShopInspector.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Infrastructure.Services;
public class FileService : IFileService
{
    private readonly IHostEnvironment _env;
    private readonly long _maxBytes = 20 * 1024 * 1024; // 20MB

    public FileService(IHostEnvironment env) => _env = env;

    public bool IsAllowedImage(IFormFile file)
    {
        var allowed = new[] { "image/jpeg", "image/png", "image/webp" };
        if (file == null || !allowed.Contains(file.ContentType) || file.Length == 0 || file.Length > _maxBytes) return false;
        return HasValidImageSignature(file);
    }

    private bool HasValidImageSignature(IFormFile file)
    {
        try
        {
            using var stream = file.OpenReadStream();
            var header = new byte[12];
            var read = stream.Read(header, 0, header.Length);

            // JPEG
            if (read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF) return true;
            // PNG
            if (read >= 8 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
                header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A) return true;
            // WEBP (RIFF....WEBP)
            if (read >= 12 && header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 &&
                header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50) return true;
        }
        catch { return false; }
        return false;
    }

    public async Task<string?> SaveImageAsync(IFormFile file, int inspectionId)
    {
        if (!IsAllowedImage(file)) return null;

        var ext = Path.GetExtension(file.FileName);
        var fileName = $"insp_{inspectionId}_{Guid.NewGuid()}{ext}";
        
        // Use persistent storage path for production (Render) vs development
        var uploadsRoot = _env.IsDevelopment()
            ? Path.Combine(_env.ContentRootPath, "wwwroot", "uploads")
            : Path.Combine("/var/data", "uploads");
            
        var folder = Path.Combine(uploadsRoot, "inspections");
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        var fullPath = Path.Combine(folder, fileName);
        using (var fs = new FileStream(fullPath, FileMode.Create))
        {   
            await file.CopyToAsync(fs);
        }

        return $"/uploads/inspections/{fileName}";
    }
}
