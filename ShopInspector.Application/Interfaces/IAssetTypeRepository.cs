using ShopInspector.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Application.Interfaces;
public interface IAssetTypeRepository
{
    Task<List<AssetType>> GetAllAsync();
    Task<AssetType?> GetByIdAsync(int id);
    Task AddAsync(AssetType entity);
    Task UpdateAsync(AssetType entity);
    Task DeleteAsync(int id);
}
