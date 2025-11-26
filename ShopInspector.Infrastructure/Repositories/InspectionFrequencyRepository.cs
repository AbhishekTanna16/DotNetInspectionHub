using Microsoft.EntityFrameworkCore;
using ShopInspector.Application.Helpers;
using ShopInspector.Application.Interfaces;
using ShopInspector.Core.Entities;
using ShopInspector.Infrastructure.Data;

namespace ShopInspector.Infrastructure.Repositories;

public class InspectionFrequencyRepository : IInspectionFrequencyRepository
{
    private readonly AppDbContext _db;

    public InspectionFrequencyRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PaginatedList<InspectionFrequency>> GetAllAsync(int? pageIndex, int? pageSize, string searchTerm = "")
    {
        var query = _db.InspectionFrequencies
            .Include(f => f.AssetInspections)
            .AsNoTracking();
        
        // Apply search filter if searchTerm is provided
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(f => EF.Functions.Like(f.FrequencyName, $"%{searchTerm}%"));
        }
        
        return await query.OrderBy(f => f.FrequencyName).ToPaginatedListAsync(pageIndex, pageSize);
    }

    public async Task<InspectionFrequency?> GetByIdAsync(int id)
    {
        return await _db.InspectionFrequencies
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.InspectionFrequencyID == id);
    }

    public async Task<InspectionFrequency> GetByIdWithInspectionsAsync(int id)
    {
        return await _db.InspectionFrequencies
            .Include(f => f.AssetInspections)
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.InspectionFrequencyID == id);
    }

    public async Task AddAsync(InspectionFrequency entity)
    {
        await _db.InspectionFrequencies.AddAsync(entity);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(InspectionFrequency entity)
    {
        _db.Entry(entity).State = EntityState.Modified;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var freq = await _db.InspectionFrequencies.FindAsync(id);
        if (freq != null)
        {
            _db.InspectionFrequencies.Remove(freq);
            await _db.SaveChangesAsync();
        }
    }

    public async Task ForceDeleteAsync(int id)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            // Get all AssetInspections using this frequency
            var assetInspections = await _db.AssetInspections
                .Where(ai => ai.InspectionFrequencyID == id)
                .ToListAsync();

            if (assetInspections.Any())
            {
                var assetInspectionIds = assetInspections.Select(ai => ai.AssetInspectionID).ToList();

                // Remove AssetInspectionCheckLists first
                var inspectionCheckLists = await _db.AssetInspectionCheckLists
                    .Where(aicl => assetInspectionIds.Contains(aicl.AssetInspectionID))
                    .ToListAsync();

                if (inspectionCheckLists.Any())
                {
                    _db.AssetInspectionCheckLists.RemoveRange(inspectionCheckLists);
                }

                // Remove InspectionPhotos if they exist
                try
                {
                    var inspectionPhotos = await _db.InspectionPhotos
                        .Where(ip => assetInspectionIds.Contains(ip.AssetInspectionID))
                        .ToListAsync();

                    if (inspectionPhotos.Any())
                    {
                        _db.InspectionPhotos.RemoveRange(inspectionPhotos);
                    }
                }
                catch (Exception)
                {
                    // InspectionPhotos table might not exist or might be configured differently
                    // Continue without removing photos
                }

                // Remove AssetInspections
                _db.AssetInspections.RemoveRange(assetInspections);
            }

            // Finally, remove the inspection frequency
            var freq = await _db.InspectionFrequencies.FindAsync(id);
            if (freq != null)
            {
                _db.InspectionFrequencies.Remove(freq);
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

    // Enhanced business logic methods
    public async Task<bool> ExistsByNameAsync(string frequencyName, int? excludeFrequencyId = null)
    {
        var normalizedName = frequencyName.Trim().ToLowerInvariant();
        
        var query = _db.InspectionFrequencies
            .Where(f => f.FrequencyName.ToLower() == normalizedName);
        
        if (excludeFrequencyId.HasValue)
        {
            query = query.Where(f => f.InspectionFrequencyID != excludeFrequencyId.Value);
        }
        
        return await query.AnyAsync();
    }

    public async Task<int> GetInspectionCountByFrequencyAsync(int frequencyId)
    {
        return await _db.AssetInspections
            .AsNoTracking()
            .CountAsync(ai => ai.InspectionFrequencyID == frequencyId);
    }

    public async Task<bool> CanDeleteFrequencyAsync(int frequencyId)
    {
        var inspectionCount = await GetInspectionCountByFrequencyAsync(frequencyId);
        return inspectionCount == 0;
    }

    public async Task<InspectionFrequencyRelatedDataInfo> GetFrequencyRelatedDataAsync(int frequencyId)
    {
        var result = new InspectionFrequencyRelatedDataInfo();

        try
        {
            // Get all AssetInspections using this frequency
            var inspections = await _db.AssetInspections
                .Include(ai => ai.Asset)
                .Include(ai => ai.Employee)
                .Where(ai => ai.InspectionFrequencyID == frequencyId)
                .AsNoTracking()
                .ToListAsync();

            result.TotalInspections = inspections.Count;
            result.LastInspectionDate = inspections.Any() ? inspections.Max(i => i.InspectionDate) : null;
            result.FirstInspectionDate = inspections.Any() ? inspections.Min(i => i.InspectionDate) : null;

            // Get affected assets
            result.AffectedAssetNames = inspections
                .Select(i => i.Asset?.AssetName ?? "Unknown Asset")
                .Distinct()
                .OrderBy(name => name)
                .ToList();

            // Get assigned employees
            result.AssignedEmployeeNames = inspections
                .Select(i => i.Employee?.EmployeeName ?? "Unknown Employee")
                .Distinct()
                .OrderBy(name => name)
                .ToList();

            // Get total checklist items count
            var inspectionIds = inspections.Select(i => i.AssetInspectionID).ToList();
            if (inspectionIds.Any())
            {
                result.ChecklistItemsCount = await _db.AssetInspectionCheckLists
                    .Where(aicl => inspectionIds.Contains(aicl.AssetInspectionID))
                    .AsNoTracking()
                    .CountAsync();
            }
        }
        catch (Exception ex)
        {
            // Log error but return empty result rather than throw
            // The calling method will handle logging
            result = new InspectionFrequencyRelatedDataInfo();
        }

        return result;
    }
}
