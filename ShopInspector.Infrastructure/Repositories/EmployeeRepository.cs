using Microsoft.EntityFrameworkCore;
using ShopInspector.Application.Helpers;
using ShopInspector.Application.Interfaces;
using ShopInspector.Core.Entities;
using ShopInspector.Infrastructure.Data;

namespace ShopInspector.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _db;

    public EmployeeRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PaginatedList<Employee>> GetAllAsync(int? pageIndex, int? pageSize, string searchTerm = "")
    {
        var query = _db.Employees
            .Include(e => e.Company)
            .AsNoTracking();
        
        // Apply search filter if searchTerm is provided
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(e => EF.Functions.Like(e.EmployeeName, $"%{searchTerm}%") ||
                                   (e.Company != null && EF.Functions.Like(e.Company.CompanyName, $"%{searchTerm}%")));
        }
        
        query = query.OrderBy(e => e.EmployeeName);
        
        return await query.ToPaginatedListAsync(pageIndex, pageSize);
    }

    public async Task<Employee?> GetByIdAsync(int id)
    {
        return await _db.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.EmployeeID == id);
    }

    public async Task<Employee> GetByIdWithCompanyAsync(int id)
    {
        return await _db.Employees
            .Include(e => e.Company)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.EmployeeID == id);
    }

    public async Task AddAsync(Employee entity)
    {
        await _db.Employees.AddAsync(entity);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Employee entity)
    {
        _db.Entry(entity).State = EntityState.Modified;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _db.Employees.FindAsync(id);
        if (entity != null)
        {
            _db.Employees.Remove(entity);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<bool> CanDeleteEmployeeAsync(int employeeId)
    {
        // Check if this employee has any asset inspections
        var hasAssetInspections = await _db.AssetInspections
            .AsNoTracking()
            .AnyAsync(ai => ai.EmployeeID == employeeId);

        return !hasAssetInspections;
    }

    public async Task<PaginatedList<Employee>> SerchEmployeeAsync(string seachterm, int? pageIndex, int? pageSize)
    {
        return await _db.Employees
            .Include(e => e.Company)
            .AsNoTracking()
            .Where(e => EF.Functions.Like(e.EmployeeName, $"%{seachterm}%") ||
                       (e.Company != null && EF.Functions.Like(e.Company.CompanyName, $"%{seachterm}%")))
            .OrderBy(e => e.EmployeeName)
            .ToPaginatedListAsync(pageIndex, pageSize);
    }

    // Enhanced business logic methods
    public async Task<bool> ExistsByNameAndCompanyAsync(string employeeName, int companyId, int? excludeEmployeeId = null)
    {
        var normalizedName = employeeName.Trim().ToLowerInvariant();
        
        var query = _db.Employees
            .Where(e => e.CompanyID == companyId && 
                       e.EmployeeName.ToLower() == normalizedName);
        
        if (excludeEmployeeId.HasValue)
        {
            query = query.Where(e => e.EmployeeID != excludeEmployeeId.Value);
        }
        
        return await query.AnyAsync();
    }

    public async Task<int> GetEmployeeCountByCompanyAsync(int companyId)
    {
        return await _db.Employees
            .AsNoTracking()
            .CountAsync(e => e.CompanyID == companyId);
    }

    public async Task<List<Employee>> GetActiveEmployeesByCompanyAsync(int companyId)
    {
        return await _db.Employees
            .AsNoTracking()
            .Where(e => e.CompanyID == companyId && e.Active)
            .OrderBy(e => e.EmployeeName)
            .ToListAsync();
    }

    public async Task<EmployeeRelatedDataInfo> GetEmployeeRelatedDataAsync(int employeeId)
    {
        var result = new EmployeeRelatedDataInfo();

        try
        {
            // Get all AssetInspections for this employee
            var inspections = await _db.AssetInspections
                .Include(ai => ai.Asset)
                .Where(ai => ai.EmployeeID == employeeId)
                .AsNoTracking()
                .ToListAsync();

            result.TotalInspections = inspections.Count;
            result.LastInspectionDate = inspections.Any() ? inspections.Max(i => i.InspectionDate) : null;

            // Get affected assets
            result.AffectedAssetNames = inspections
                .Select(i => i.Asset?.AssetName ?? "Unknown Asset")
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
            result = new EmployeeRelatedDataInfo();
        }

        return result;
    }

    public async Task ForceDeleteAsync(int id)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            // First, remove all AssetInspections associated with this employee
            var assetInspections = await _db.AssetInspections
                .Where(ai => ai.EmployeeID == id)
                .ToListAsync();

            if (assetInspections.Any())
            {
                // Remove all AssetInspectionCheckLists first (due to cascade)
                var assetInspectionIds = assetInspections.Select(ai => ai.AssetInspectionID).ToList();
                var inspectionCheckLists = await _db.AssetInspectionCheckLists
                    .Where(aicl => assetInspectionIds.Contains(aicl.AssetInspectionID))
                    .ToListAsync();

                if (inspectionCheckLists.Any())
                {
                    _db.AssetInspectionCheckLists.RemoveRange(inspectionCheckLists);
                }

                // Remove InspectionPhotos if they exist (check if the table exists first)
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

                // Now remove the AssetInspections
                _db.AssetInspections.RemoveRange(assetInspections);
            }

            // Finally, remove the employee
            var entity = await _db.Employees.FindAsync(id);
            if (entity != null)
            {
                _db.Employees.Remove(entity);
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
}
