using Microsoft.EntityFrameworkCore;
using ShopInspector.Application.Interfaces;
using ShopInspector.Core.Entities;
using ShopInspector.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Infrastructure.Repositories;
public class AssetCheckListRepository : IAssetCheckListRepository
{
    private readonly AppDbContext _db;
    public AssetCheckListRepository(AppDbContext db) => _db = db;

    public async Task<List<AssetCheckList>> GetAllAsync()
    {
        return await _db.AssetCheckLists
            .Include(ac => ac.Asset)
            .Include(ac => ac.InspectionCheckList)
            .OrderBy(ac => ac.AssetID).ThenBy(ac => ac.DisplayOrder)
            .ToListAsync();
    }

    public async Task<List<AssetCheckList>> GetByAssetIdAsync(
     int   assetId)
    {
        return await _db.AssetCheckLists
            .Where(ac => ac.AssetID == assetId && ac.Active)
            .Include(ac => ac.InspectionCheckList)
            .OrderBy(ac => ac.DisplayOrder)
            .ToListAsync();
    }

    public async Task<AssetCheckList?> GetByIdAsync(int id)
    {
        return await _db.AssetCheckLists
            .Include(ac => ac.Asset)
            .Include(ac => ac.InspectionCheckList)
            .FirstOrDefaultAsync(ac => ac.AssetCheckListID == id);
    }

    public async Task AddAsync(AssetCheckList entity)
    {
        await _db.AssetCheckLists.AddAsync(entity);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(AssetCheckList entity)
    {
        _db.AssetCheckLists.Update(entity);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _db.AssetCheckLists.FindAsync(id);
        if (entity == null) return;
        _db.AssetCheckLists.Remove(entity);
        await _db.SaveChangesAsync();
    }
}


