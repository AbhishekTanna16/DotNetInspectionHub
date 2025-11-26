using Microsoft.EntityFrameworkCore;
using ShopInspector.Application.Helpers;
using ShopInspector.Application.Interfaces;
using ShopInspector.Core.Entities;
using ShopInspector.Infrastructure.Data;

namespace ShopInspector.Infrastructure.Repositories;

public class AssetRepository : IAssetRepository
{
    private readonly AppDbContext _db;
    
    public AssetRepository(AppDbContext db) 
    {
        _db = db;
    }
    
    public async Task<PaginatedList<Asset>> GetAllAsync(int? pageIndex, int? pageSize)
    {
        return await _db.Assets
            .Include(a => a.AssetType)
            .AsNoTracking()
            .OrderBy(a => a.AssetID)
            .ToPaginatedListAsync(pageIndex, pageSize);
    }

    public async Task<Asset?> GetByIdAsync(int id)
    {
        return await _db.Assets
            .Include(a => a.AssetType)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AssetID == id);
    }

    // New helper methods for Asset Controller dropdown data
    public async Task<List<(int AssetTypeID, string AssetTypeName)>> GetAssetTypeDropdownDataAsync()
    {
        return await _db.AssetTypes
            .AsNoTracking()
            .OrderBy(at => at.AssetTypeName)
            .Select(at => new ValueTuple<int, string>(at.AssetTypeID, at.AssetTypeName))
            .ToListAsync();
    }

    public async Task<List<(int EmployeeID, string EmployeeName)>> GetEmployeeDropdownDataAsync()
    {
        return await _db.Employees
            .AsNoTracking()
            .Where(e => e.Active)
            .OrderBy(e => e.EmployeeName)
            .Select(e => new ValueTuple<int, string>(e.EmployeeID, e.EmployeeName))
            .ToListAsync();
    }

    public async Task<List<(int CompanyID, string CompanyName)>> GetCompanyDropdownDataAsync()
    {
        return await _db.Companies
            .AsNoTracking()
            .Where(c => c.Active)
            .OrderBy(c => c.CompanyName)
            .Select(c => new ValueTuple<int, string>(c.CompanyID, c.CompanyName))
            .ToListAsync();
    }

    // Fixed: Helper method to get all dropdown data for Asset forms in one call
    public async Task<(
        List<(int AssetTypeID, string AssetTypeName)> AssetTypes,
        List<(int EmployeeID, string EmployeeName)> Employees,
        List<(int CompanyID, string CompanyName)> Companies
    )> GetAssetFormDropdownDataAsync()
    {
        // Execute queries sequentially to avoid DbContext concurrency issues
        var assetTypes = await GetAssetTypeDropdownDataAsync();
        var employees = await GetEmployeeDropdownDataAsync();
        var companies = await GetCompanyDropdownDataAsync();

        return (
            AssetTypes: assetTypes,
            Employees: employees,
            Companies: companies
        );
    }

    public async Task AddAsync(Asset asset)
    {
        await _db.Assets.AddAsync(asset);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Asset asset)
    {
        _db.Entry(asset).State = EntityState.Modified;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int assetId)
    {
        
        if (assetId <= 0)
        {
            throw new ArgumentException("Asset ID must be a positive integer", nameof(assetId));
        }
        if (_db == null)
        {
            throw new InvalidOperationException("Database context is not initialized");
        }

        using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var assetExists = await _db.Assets.AsNoTracking()
                .AnyAsync(a => a.AssetID == assetId);
            
            if (!assetExists)
            {
                throw new InvalidOperationException($"Asset with ID {assetId} does not exist");
            }

            var inspectionIds = await _db.AssetInspections
                .AsNoTracking()
                .Where(i => i.AssetID == assetId)
                .Select(i => i.AssetInspectionID)
                .ToListAsync();

            if (inspectionIds.Any())
            {
                var inspectionPhotos = await _db.InspectionPhotos
                    .Where(ip => inspectionIds.Contains(ip.AssetInspectionID))
                    .ToListAsync();

                if (inspectionPhotos.Any())
                {
                    _db.InspectionPhotos.RemoveRange(inspectionPhotos);
                    await _db.SaveChangesAsync();
                }
                var inspectionCheckListItems = await _db.AssetInspectionCheckLists
                    .Where(ic => inspectionIds.Contains(ic.AssetInspectionID))
                    .ToListAsync();

                if (inspectionCheckListItems.Any())
                {
                    _db.AssetInspectionCheckLists.RemoveRange(inspectionCheckListItems);
                    await _db.SaveChangesAsync();
                }

                var inspectionsToDelete = await _db.AssetInspections
                    .Where(i => i.AssetID == assetId)
                    .ToListAsync();

                if (inspectionsToDelete.Any())
                {
                    _db.AssetInspections.RemoveRange(inspectionsToDelete);
                    await _db.SaveChangesAsync();
                }
            }
            var assetCheckLists = await _db.AssetCheckLists
                .Where(ac => ac.AssetID == assetId)
                .ToListAsync();

            if (assetCheckLists.Any())
            {
                _db.AssetCheckLists.RemoveRange(assetCheckLists);
                await _db.SaveChangesAsync();
            }
            var asset = await _db.Assets.FindAsync(assetId);
            if (asset == null)
            {
                throw new InvalidOperationException($"Asset with ID {assetId} was not found during deletion");
            }
            _db.Assets.Remove(asset);
            var deletedRows = await _db.SaveChangesAsync();
            if (deletedRows == 0)
            {
                throw new InvalidOperationException($"Failed to delete asset with ID {assetId}");
            }

            await tx.CommitAsync();
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            throw new InvalidOperationException($"Failed to delete asset with ID {assetId}. {ex.Message}", ex);
        }
    }

    public async Task<PaginatedList<Asset>> SerchAssetAsync(string seachterm, int? pageIndex, int? pageSize)
    {
        var query = _db.Assets.Include(a => a.AssetType).AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(seachterm))
        {
            query = query.Where(a =>
                EF.Functions.Like(a.AssetName, $"%{seachterm}%") ||
                EF.Functions.Like(a.AssetCode, $"%{seachterm}%"));
        }

        return await query.OrderBy(a => a.AssetName).ToPaginatedListAsync(pageIndex, pageSize);
    }
}