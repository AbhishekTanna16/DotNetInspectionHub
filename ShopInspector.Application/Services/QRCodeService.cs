using ShopInspector.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Application.Services;
public class QRCodeService : IQRCodeService
{
    private readonly IQRCodeRepository _repo;

    public QRCodeService(IQRCodeRepository repo)
    {
        _repo = repo;
    }

    public async Task<(byte[] PngBytes, string? SavedPath)> GenerateQrAsync(
        string url, int? assetId = null, bool saveToDisk = false)
    {
        return await _repo.GenerateAsync(url, assetId, saveToDisk);
    }
}

