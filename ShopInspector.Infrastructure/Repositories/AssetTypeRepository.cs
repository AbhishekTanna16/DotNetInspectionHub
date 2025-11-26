using Microsoft.EntityFrameworkCore;
using QuestPDF.Helpers;
using ShopInspector.Application.Helpers;
using ShopInspector.Application.Interfaces;
using ShopInspector.Core.Entities;
using ShopInspector.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Infrastructure.Repositories;

public class AssetTypeRepository : IAssetTypeRepository
{
    private readonly AppDbContext _db;

    public AssetTypeRepository(AppDbContext db) => _db = db;

    public async Task<PaginatedList<AssetType>> GetAllAsync(int? pageIndex, int? pageSize) =>
        await _db.AssetTypes.ToPaginatedListAsync(pageIndex, pageSize);

    public async Task<AssetType?> GetByIdAsync(int id) =>
        await _db.AssetTypes.FindAsync(id);

    // Helper method to get AssetType data formatted for view model
    public async Task<(int AssetTypeID, string AssetTypeName)?> GetAssetTypeViewDataAsync(int id)
    {
        var assetType = await _db.AssetTypes.FindAsync(id);
        if (assetType == null) return null;
        
        return (assetType.AssetTypeID, assetType.AssetTypeName);
    }

    // Helper method to create AssetType from view model data
    public async Task<AssetType> CreateAssetTypeFromViewDataAsync(string assetTypeName)
    {
        var entity = new AssetType
        {
            AssetTypeName = assetTypeName
        };
        
        await _db.AssetTypes.AddAsync(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    // Helper method to update AssetType from view model data
    public async Task<bool> UpdateAssetTypeFromViewDataAsync(int assetTypeID, string assetTypeName)
    {
        var entity = await _db.AssetTypes.FindAsync(assetTypeID);
        if (entity == null) return false;
        
        entity.AssetTypeName = assetTypeName;
        _db.AssetTypes.Update(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task AddAsync(AssetType entity)
    {
        await _db.AssetTypes.AddAsync(entity);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(AssetType entity)
    {
        _db.AssetTypes.Update(entity);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var o = await _db.AssetTypes.FindAsync(id);
        if (o != null)
        {
            _db.AssetTypes.Remove(o);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<PaginatedList<AssetType>> SerchAssetTypeAsync(string seachterm ,int? pageIndex, int? pageSize)
    {
        var query = _db.AssetTypes.Include(a => a.Assets).AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(seachterm))
        {
            query = query.Where(a =>
                EF.Functions.Like(a.AssetTypeName, $"%{seachterm}%"));
        }

        return await query.OrderBy(a => a.AssetTypeName).ToPaginatedListAsync(pageIndex, pageSize);
    }
}

