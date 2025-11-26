using Microsoft.EntityFrameworkCore;
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
public class InspectionCheckListRepository : IInspectionCheckListRepository
{
    private readonly AppDbContext _db;
    public InspectionCheckListRepository(AppDbContext db) => _db = db;

    public async Task<PaginatedList<InspectionCheckList>> GetAllAsync(int? pageIndex, int? pageSize)
    {
        return await _db.InspectionCheckLists
                        .OrderBy(i => i.InspectionCheckListName)
                        .ToPaginatedListAsync(pageIndex,pageSize);
    }

    public async Task<PaginatedList<InspectionCheckList>> GetAllAsync(int? pageIndex, int? pageSize, string? searchTerm)
    {
        var query = _db.InspectionCheckLists.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(i => 
                EF.Functions.Like(i.InspectionCheckListName, $"%{searchTerm}%") ||
                EF.Functions.Like(i.InspectionCheckListTitle, $"%{searchTerm}%"));
        }

        return await query
                    .OrderBy(i => i.InspectionCheckListName)
                    .ToPaginatedListAsync(pageIndex, pageSize);
    }

    public async Task<InspectionCheckList?> GetByIdAsync(int id)
    {
        return await _db.InspectionCheckLists.FindAsync(id);
    }

    public async Task AddAsync(InspectionCheckList entity)
    {
        await _db.InspectionCheckLists.AddAsync(entity);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(InspectionCheckList entity)
    {
        _db.InspectionCheckLists.Update(entity);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var item = await _db.InspectionCheckLists.FindAsync(id);
        if (item == null) return;
        _db.InspectionCheckLists.Remove(item);
        await _db.SaveChangesAsync();
    }

    public async Task ForceDeleteAsync(int id)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            // Get all AssetCheckLists using this InspectionCheckList
            var assetCheckLists = await _db.AssetCheckLists
                .Where(acl => acl.InspectionCheckListID == id)
                .ToListAsync();

            if (assetCheckLists.Any())
            {
                var assetCheckListIds = assetCheckLists.Select(acl => acl.AssetCheckListID).ToList();

                // Remove related AssetInspectionCheckLists first
                var inspectionCheckLists = await _db.AssetInspectionCheckLists
                    .Where(aicl => assetCheckListIds.Contains(aicl.AssetCheckListID))
                    .ToListAsync();

                if (inspectionCheckLists.Any())
                {
                    _db.AssetInspectionCheckLists.RemoveRange(inspectionCheckLists);
                }

                // Remove the AssetCheckLists
                _db.AssetCheckLists.RemoveRange(assetCheckLists);
            }

            // Finally, remove the InspectionCheckList
            var item = await _db.InspectionCheckLists.FindAsync(id);
            if (item != null)
            {
                _db.InspectionCheckLists.Remove(item);
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

    public async Task<bool> CanDeleteCheckListAsync(int id)
    {
        // Check if this InspectionCheckList is being used by any AssetCheckList
        var hasAssetCheckLists = await _db.AssetCheckLists
            .AnyAsync(acl => acl.InspectionCheckListID == id);

        return !hasAssetCheckLists;
    }

    public async Task<InspectionCheckListRelatedDataInfo> GetCheckListRelatedDataAsync(int checkListId)
    {
        var result = new InspectionCheckListRelatedDataInfo();

        try
        {
            // Get all AssetCheckLists using this InspectionCheckList
            var assetCheckLists = await _db.AssetCheckLists
                .Include(acl => acl.Asset)
                .Include(acl => acl.Asset.AssetType)
                .Where(acl => acl.InspectionCheckListID == checkListId)
                .AsNoTracking()
                .ToListAsync();

            result.TotalAssetCheckLists = assetCheckLists.Count;

            // Get affected assets
            result.AffectedAssetNames = assetCheckLists
                .Select(acl => acl.Asset?.AssetName ?? "Unknown Asset")
                .Distinct()
                .OrderBy(name => name)
                .ToList();

            // Get asset departments instead of company names (since Assets don't have direct Company relationship)
            result.AssignedCompanyNames = assetCheckLists
                .Select(acl => acl.Asset?.Department ?? "Unknown Department")
                .Distinct()
                .OrderBy(name => name)
                .ToList();

            // Get total inspection items count from AssetInspectionCheckLists
            var assetCheckListIds = assetCheckLists.Select(acl => acl.AssetCheckListID).ToList();
            if (assetCheckListIds.Any())
            {
                var inspectionItems = await _db.AssetInspectionCheckLists
                    .Include(aicl => aicl.AssetInspection)
                    .Where(aicl => assetCheckListIds.Contains(aicl.AssetCheckListID))
                    .AsNoTracking()
                    .ToListAsync();

                result.TotalInspectionItems = inspectionItems.Count;
                
                if (inspectionItems.Any())
                {
                    result.FirstUsedDate = inspectionItems.Min(ii => ii.AssetInspection.InspectionDate);
                    result.LastUsedDate = inspectionItems.Max(ii => ii.AssetInspection.InspectionDate);
                }
            }
        }
        catch (Exception)
        {
            // Log error but return empty result rather than throw
            // The calling method will handle logging
            result = new InspectionCheckListRelatedDataInfo();
        }

        return result;
    }
}


