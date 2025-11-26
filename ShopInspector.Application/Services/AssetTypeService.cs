using ShopInspector.Application.Interfaces;
using ShopInspector.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Application.Services;
public class AssetTypeService : IAssetTypeService
{
    private readonly IAssetTypeRepository _repo;

    public AssetTypeService(IAssetTypeRepository repo)
    {
        _repo = repo;
    }

    public Task<PaginatedList<AssetType>> GetAllAsync(int? pageIndex, int? pageSize) => _repo.GetAllAsync(pageIndex, pageSize);
    public Task<AssetType?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);
    public Task AddAsync(AssetType entity) => _repo.AddAsync(entity);
    public Task UpdateAsync(AssetType entity) => _repo.UpdateAsync(entity);
    public Task DeleteAsync(int id) => _repo.DeleteAsync(id);

    public Task<PaginatedList<AssetType>> SerchAssetTypeAsync(string seachterm, int? pageIndex, int? pageSize) => _repo.SerchAssetTypeAsync(seachterm, pageIndex, pageSize);
}
