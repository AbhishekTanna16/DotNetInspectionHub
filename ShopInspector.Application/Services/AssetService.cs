using ShopInspector.Application.Interfaces;
using ShopInspector.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Application.Services;
public class AssetService : IAssetService
{
    private readonly IAssetRepository _repo;
    public AssetService(IAssetRepository repo) => _repo = repo;

    public Task<List<Asset>> GetAllAsync() => _repo.GetAllAsync();

    public Task<Asset?> GetByIdAsync(
        
       int id) => _repo.GetByIdAsync(id);

    public Task AddAsync(Asset asset) => _repo.AddAsync(asset);

    public Task UpdateAsync(Asset asset) => _repo.UpdateAsync(asset);

    public Task DeleteAsync(int id) => _repo.DeleteAsync(id);
}
