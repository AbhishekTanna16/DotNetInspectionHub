using Microsoft.EntityFrameworkCore;
using ShopInspector.Application.Helpers;
using ShopInspector.Application.Interfaces;
using ShopInspector.Core.Entities;
using ShopInspector.Infrastructure.Data;

namespace ShopInspector.Infrastructure.Repositories;

public class AssetCheckListRepository : IAssetCheckListRepository
{
    private readonly AppDbContext _db;
    public AssetCheckListRepository(AppDbContext db) => _db = db;

    public async Task<PaginatedList<AssetCheckList>> GetAllAsync(int? pageIndex, int? pageSize, string searchTerm = "")
    {
        var query = _db.AssetCheckLists
            .Include(ac => ac.Asset)
            .Include(ac => ac.InspectionCheckList)
            .AsNoTracking(); // Add this for better performance and to avoid tracking issues
        
        // Apply search filter if searchTerm is provided
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(ac => 
                (ac.Asset != null && EF.Functions.Like(ac.Asset.AssetName, $"%{searchTerm}%")) ||
                (ac.Asset != null && EF.Functions.Like(ac.Asset.AssetCode, $"%{searchTerm}%")) ||
                (ac.InspectionCheckList != null && EF.Functions.Like(ac.InspectionCheckList.InspectionCheckListName, $"%{searchTerm}%"))
            );
        }
        
        return await query.OrderBy(ac => ac.AssetID).ThenBy(ac => ac.DisplayOrder)
            .ToPaginatedListAsync(pageIndex, pageSize);
    }

    public async Task<List<AssetCheckList>> GetByAssetIdAsync(int assetId)
    {
        return await _db.AssetCheckLists
            .Where(ac => ac.AssetID == assetId && ac.Active)
            .Include(ac => ac.InspectionCheckList)
            .AsNoTracking()
            .OrderBy(ac => ac.DisplayOrder)
            .ToListAsync();
    }

    public async Task<bool> CanDeleteAsync(int assetCheckListId)
    {
        // Check if this AssetCheckList is being used by any AssetInspectionCheckList records
        var hasInspectionRecords = await _db.AssetInspectionCheckLists
            .AsNoTracking()
            .AnyAsync(aicl => aicl.AssetCheckListID == assetCheckListId);

        return !hasInspectionRecords;
    }

    // New method: Get formatted asset data for SelectListItems
    public async Task<List<(int AssetID, string DisplayText)>> GetAssetDropdownDataAsync()
    {
        return await _db.Assets
            .Where(a => a.Active)
            .AsNoTracking()
            .OrderBy(a => a.AssetName)
            .Select(a => new ValueTuple<int, string>(
                a.AssetID, 
                $"{a.AssetName} ({a.AssetCode})"
            ))
            .ToListAsync();
    }

    // New method: Get inspection items with all required data for the controller
    public async Task<List<(int AssetCheckListID, int InspectionCheckListID, string InspectionCheckListName, string InspectionCheckListDescription, string InspectionCheckListTitle, bool Active)>> GetInspectionItemsForAssetAsync(int assetId)
    {
        return await _db.AssetCheckLists
            .Where(ac => ac.AssetID == assetId && ac.Active)
            .Include(ac => ac.InspectionCheckList)
            .AsNoTracking()
            .OrderBy(ac => ac.DisplayOrder)
            .Select(m => new ValueTuple<int, int, string, string, string, bool>(
                m.AssetCheckListID,
                m.InspectionCheckListID,
                m.InspectionCheckList != null ? m.InspectionCheckList.InspectionCheckListName : $"Checklist {m.InspectionCheckListID}",
                m.InspectionCheckList != null ? m.InspectionCheckList.InspectionCheckListDescription ?? string.Empty : string.Empty,
                m.InspectionCheckList != null ? m.InspectionCheckList.InspectionCheckListTitle ?? string.Empty : string.Empty,
                m.Active
            ))
            .ToListAsync();
    }

    // New method: Get employee dropdown data
    public async Task<List<(int EmployeeID, string EmployeeName)>> GetEmployeeDropdownDataAsync()
    {
        return await _db.Employees
            .Where(e => e.Active)
            .AsNoTracking()
            .OrderBy(e => e.EmployeeName)
            .Select(e => new ValueTuple<int, string>(e.EmployeeID, e.EmployeeName))
            .ToListAsync();
    }

    // New method: Get inspection frequency dropdown data
    public async Task<List<(int InspectionFrequencyID, string FrequencyName)>> GetInspectionFrequencyDropdownDataAsync()
    {
        return await _db.InspectionFrequencies
            .AsNoTracking()
            .OrderBy(f => f.FrequencyName)
            .Select(f => new ValueTuple<int, string>(f.InspectionFrequencyID, f.FrequencyName))
            .ToListAsync();
    }

    public async Task<AssetCheckList?> GetByIdAsync(int id)
    {
        return await _db.AssetCheckLists
            .Include(ac => ac.Asset)
            .Include(ac => ac.InspectionCheckList)
            .AsNoTracking()
            .FirstOrDefaultAsync(ac => ac.AssetCheckListID == id);
    }

    public async Task<AssetCheckList?> GetByAssetAndChecklistAsync(int assetId, int inspectionCheckListId)
    {
        return await _db.AssetCheckLists
            .AsNoTracking()
            .FirstOrDefaultAsync(ac => ac.AssetID == assetId && ac.InspectionCheckListID == inspectionCheckListId);
    }

    public async Task AddAsync(AssetCheckList entity)
    {
        await _db.AssetCheckLists.AddAsync(entity);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(AssetCheckList entity)
    {
        _db.Entry(entity).State = EntityState.Modified;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _db.AssetCheckLists.FindAsync(id);
        if (entity == null) return;
        _db.AssetCheckLists.Remove(entity);
        await _db.SaveChangesAsync();
    }

    public async Task ForceDeleteAsync(int id)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            // Get all AssetInspectionCheckLists that use this AssetCheckList
            var assetInspectionCheckLists = await _db.AssetInspectionCheckLists
                .Where(aicl => aicl.AssetCheckListID == id)
                .ToListAsync();

            if (assetInspectionCheckLists.Any())
            {
                // Remove all AssetInspectionCheckLists that reference this AssetCheckList
                _db.AssetInspectionCheckLists.RemoveRange(assetInspectionCheckLists);
            }

            // Finally, remove the AssetCheckList
            var entity = await _db.AssetCheckLists.FindAsync(id);
            if (entity != null)
            {
                _db.AssetCheckLists.Remove(entity);
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<AssetCheckListRelatedDataInfo> GetAssetCheckListRelatedDataAsync(int assetCheckListId)
    {
        var result = new AssetCheckListRelatedDataInfo();

        try
        {
            // Get the AssetCheckList with its related data
            var assetCheckList = await _db.AssetCheckLists
                .Include(acl => acl.Asset)
                .Include(acl => acl.InspectionCheckList)
                .Where(acl => acl.AssetCheckListID == assetCheckListId)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (assetCheckList != null)
            {
                result.AssetName = assetCheckList.Asset?.AssetName ?? "Unknown Asset";
                result.CheckListName = assetCheckList.InspectionCheckList?.InspectionCheckListName ?? "Unknown Checklist";
            }

            // Get all AssetInspectionCheckLists that use this AssetCheckList
            var inspectionCheckLists = await _db.AssetInspectionCheckLists
                .Include(aicl => aicl.AssetInspection)
                .Include(aicl => aicl.AssetInspection.Employee)
                .Include(aicl => aicl.AssetInspection.InspectionFrequency)
                .Where(aicl => aicl.AssetCheckListID == assetCheckListId)
                .AsNoTracking()
                .ToListAsync();

            result.TotalInspectionRecords = inspectionCheckLists.Count;

            if (inspectionCheckLists.Any())
            {
                result.FirstInspectionDate = inspectionCheckLists.Min(icl => icl.AssetInspection.InspectionDate);
                result.LastInspectionDate = inspectionCheckLists.Max(icl => icl.AssetInspection.InspectionDate);

                // Get unique employee names
                result.EmployeeNames = inspectionCheckLists
                    .Select(icl => icl.AssetInspection.Employee?.EmployeeName ?? "Unknown Employee")
                    .Distinct()
                    .OrderBy(name => name)
                    .ToList();

                // Get unique inspection frequencies
                result.InspectionFrequencies = inspectionCheckLists
                    .Select(icl => icl.AssetInspection.InspectionFrequency?.FrequencyName ?? "Unknown Frequency")
                    .Distinct()
                    .OrderBy(name => name)
                    .ToList();
            }
        }
        catch (Exception)
        {
            // Log error but return empty result rather than throw
            // The calling method will handle logging
            result = new AssetCheckListRelatedDataInfo();
        }

        return result;
    }

    // NEW: Add methods to get real-time statistics
    public async Task<int> GetTotalActiveAssetsWithChecklistsAsync()
    {
        return await _db.AssetCheckLists
         .Where(ac => ac.Active)
         .AsNoTracking()             // ✔ Apply here
         .Select(ac => ac.AssetID)
         .Distinct()
         .CountAsync(); 
    }

    public async Task<int> GetTotalActiveChecklistAssignmentsAsync()
    {
        return await _db.AssetCheckLists
            .Where(ac => ac.Active)
            .AsNoTracking()
            .CountAsync();
    }
}


