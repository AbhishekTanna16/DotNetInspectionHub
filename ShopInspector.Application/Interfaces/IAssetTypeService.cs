using ShopInspector.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Application.Interfaces;
public interface IAssetTypeService
{
    Task<PaginatedList<AssetType>> GetAllAsync(int? pageIndex, int? pageSize);
    Task<AssetType?> GetByIdAsync(int id);
    Task AddAsync(AssetType entity);
    Task<PaginatedList<AssetType>> SerchAssetTypeAsync(string seachterm, int? pageIndex, int? pageSize);
    Task UpdateAsync(AssetType entity);
    Task DeleteAsync(int id);
}
