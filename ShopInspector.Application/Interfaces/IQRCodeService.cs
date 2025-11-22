using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Application.Interfaces;
public interface IQRCodeService
{
    Task<(byte[] PngBytes, string? SavedPath)> GenerateQrAsync(string url, int? assetId = null, bool saveToDisk = false);
}
