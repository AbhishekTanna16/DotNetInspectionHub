using ShopInspector.Application.Interfaces;
using ShopInspector.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Application.Services;
public class AssetCheckListService : IAssetCheckListService
{
    private readonly IAssetCheckListRepository _repo;

    public AssetCheckListService(IAssetCheckListRepository repo)
    {
        _repo = repo;
    }

    public Task<List<AssetCheckList>> GetAllAsync() => _repo.GetAllAsync();
    public Task<List<AssetCheckList>> GetByAssetIdAsync(
       int assetId) => _repo.GetByAssetIdAsync(assetId);
    public Task<AssetCheckList?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);
    public Task AddAsync(AssetCheckList entity) => _repo.AddAsync(entity);
    public Task UpdateAsync(AssetCheckList entity) => _repo.UpdateAsync(entity);
    public Task DeleteAsync(int id) => _repo.DeleteAsync(id);
}

