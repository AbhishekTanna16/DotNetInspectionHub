using ShopInspector.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Application.Interfaces;
public interface IAssetTypeRepository
{
    Task<PaginatedList<AssetType>> GetAllAsync(int? pageIndex, int? pageSize);
    Task<AssetType?> GetByIdAsync(int id);
    Task AddAsync(AssetType entity);
    Task UpdateAsync(AssetType entity);
    Task DeleteAsync(int id);
    Task<PaginatedList<AssetType>> SerchAssetTypeAsync(string seachterm,  int? pageIndex, int? pageSize);

    // New helper methods for clean controller operations
    Task<(int AssetTypeID, string AssetTypeName)?> GetAssetTypeViewDataAsync(int id);
    Task<AssetType> CreateAssetTypeFromViewDataAsync(string assetTypeName);
    Task<bool> UpdateAssetTypeFromViewDataAsync(int assetTypeID, string assetTypeName);
}
