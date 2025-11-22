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
public class AssetRepository : IAssetRepository
{
    private readonly AppDbContext _db;
    public AssetRepository(AppDbContext db) => _db = db;



    public async Task<List<Asset>> GetAllAsync()
    {
        return await _db.Assets
            .Include(a => a.AssetType)
            .ToListAsync();
    }

    public async Task<Asset?> GetByIdAsync(
        
       int id)
    {
        return await _db.Assets
            .Include(a => a.AssetType)
            .FirstOrDefaultAsync(a => a.AssetID == id);
    }

    public async Task AddAsync(Asset asset)
    {
        await _db.Assets.AddAsync(asset);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Asset asset)
    {
        _db.Assets.Update(asset);
        await _db.SaveChangesAsync();
    }

    // using Microsoft.EntityFrameworkCore;
    // inside your AssetRepository class

    public async Task DeleteAsync(int assetId)
    {
        // adjust _dbContext to your actual context field name
        using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            // 1) Find inspections for this asset (if any)
            var inspectionIds = await _db.AssetInspections
                .Where(i => i.AssetID == assetId)
                .Select(i => i.AssetInspectionID)
                .ToListAsync();

            // 2) Delete AssetInspectionCheckLists (child of inspections) first
            if (inspectionIds.Any())
            {
                var inspectionCheckListItems = await _db.AssetInspectionCheckLists
                    .Where(ic => inspectionIds.Contains(ic.AssetInspectionID))
                    .ToListAsync();

                if (inspectionCheckListItems.Any())
                {
                    _db.AssetInspectionCheckLists.RemoveRange(inspectionCheckListItems);
                    await _db.SaveChangesAsync();
                }
            }

            // 3) Delete AssetInspections (child of asset)
            var inspectionsToDelete = await _db.AssetInspections
                .Where(i => i.AssetID == assetId)
                .ToListAsync();

            if (inspectionsToDelete.Any())
            {
                _db.AssetInspections.RemoveRange(inspectionsToDelete);
                await _db.SaveChangesAsync();
            }

            // 4) Delete AssetCheckLists (child entries that reference Asset)
            var assetCheckLists = await _db.AssetCheckLists
                .Where(ac => ac.AssetID == assetId)
                .ToListAsync();

            if (assetCheckLists.Any())
            {
                _db.AssetCheckLists.RemoveRange(assetCheckLists);
                await _db.SaveChangesAsync();
            }

            // 5) (Optional) Delete any other child tables that reference Asset (add here)
            // e.g. other tables: AssetFiles, AssetAssignments, ... remove them before deleting Asset

            // 6) Finally delete the Asset
            var asset = await _db.Assets.FindAsync(assetId);
            if (asset != null)
            {
                _db.Assets.Remove(asset);
                await _db.SaveChangesAsync();
            }

            await tx.CommitAsync();
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            // optionally log ex here
            throw; // rethrow so controller can handle and report
        }
    }

}
