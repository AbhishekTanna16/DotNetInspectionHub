using ShopInspector.Application.Interfaces;
using ShopInspector.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Application.Services;
public class AssetInspectionService : IAssetInspectionService
{
    private readonly IAssetInspectionRepository _repo;

    public AssetInspectionService(IAssetInspectionRepository repo)
    {
        _repo = repo;
    }

    public Task AddAsync(AssetInspection entity)
        => _repo.AddAsync(entity);

    public Task UpdateAsync(AssetInspection entity)
        => _repo.UpdateAsync(entity);

    public Task<AssetInspection?> GetByIdAsync(int id)
        => _repo.GetByIdAsync(id);

    public Task<List<AssetInspection>> GetByAssetIdAsync(
        
     int   assetId)
        => _repo.GetByAssetIdAsync(assetId);

    public Task AddCheckListRowAsync(AssetInspectionCheckList row)
        => _repo.AddCheckListRowAsync(row);
}
