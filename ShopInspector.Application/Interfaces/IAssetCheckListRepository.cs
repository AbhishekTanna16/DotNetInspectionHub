using ShopInspector.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Application.Interfaces;
public interface IAssetCheckListRepository
{
    Task<List<AssetCheckList>> GetAllAsync();
    Task<List<AssetCheckList>> GetByAssetIdAsync(int assetId);
    Task<AssetCheckList?> GetByIdAsync(int id);
    Task<AssetCheckList?> GetByAssetAndChecklistAsync(int assetId, int inspectionCheckListId);
    Task AddAsync(AssetCheckList entity);
    Task UpdateAsync(AssetCheckList entity);
    Task DeleteAsync(int id);
}
