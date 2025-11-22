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
public class AssetInspectionRepository : IAssetInspectionRepository
{
    private readonly AppDbContext _db;
    public AssetInspectionRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(AssetInspection entity)
    {

        await _db.AssetInspections.AddAsync(entity);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(AssetInspection entity)
    {
        _db.AssetInspections.Update(entity);
        await _db.SaveChangesAsync();
    }

    public async Task<AssetInspection?> GetByIdAsync(int id)
    {
        return await _db.AssetInspections
            .Include(ai => ai.Asset)
            .Include(ai => ai.AssetInspectionCheckLists)
                .ThenInclude(aic => aic.AssetCheckList)
                    .ThenInclude(acl => acl.InspectionCheckList)
            .Include(ai => ai.Employee)
            .Include(ai => ai.InspectionFrequency)
            .Include(ai => ai.Photos)  // Add photos
            .FirstOrDefaultAsync(ai => ai.AssetInspectionID == id);
    }

    public async Task<List<AssetInspection>> GetByAssetIdAsync(int assetId)
    {
        return await _db.AssetInspections
            .Where(ai => ai.AssetID == assetId)
            .Include(ai => ai.Asset)
            .Include(ai => ai.AssetInspectionCheckLists)
                .ThenInclude(aic => aic.AssetCheckList)
                    .ThenInclude(acl => acl.InspectionCheckList)
            .Include(ai => ai.Employee)
            .Include(ai => ai.InspectionFrequency)
            .Include(ai => ai.Photos)  // Add photos
            .OrderByDescending(ai => ai.InspectionDate)
            .ToListAsync();
    }

    public async Task AddCheckListRowAsync(AssetInspectionCheckList row)
    {
        await _db.AssetInspectionCheckLists.AddAsync(row);
        await _db.SaveChangesAsync();
    }
}



