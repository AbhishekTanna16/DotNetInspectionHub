using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Application.Interfaces;
public interface IQRCodeRepository
{
    Task<(byte[] PngBytes, string? SavedPath)> GenerateAsync(string url,     int? assetId = null, bool saveToDisk = false);
}
