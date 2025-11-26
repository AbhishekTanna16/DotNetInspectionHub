using Microsoft.EntityFrameworkCore;
using ShopInspector.Application.Helpers;
using ShopInspector.Application.Interfaces;
using ShopInspector.Core.Entities;
using ShopInspector.Infrastructure.Data;

namespace ShopInspector.Infrastructure.Repositories;

public class CompanyRepository : ICompanyRepository
{
    private readonly AppDbContext _context;

    public CompanyRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<Company>> GetAllAsync(int? pageIndex, int? pageSize, string searchTerm = "")
    {
        var query = _context.Companies
            .Include(c => c.Employees)
            .AsNoTracking();
        
        // Apply search filter if searchTerm is provided
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(c => EF.Functions.Like(c.CompanyName, $"%{searchTerm}%") ||
                                   EF.Functions.Like(c.CompanyAdminEmail, $"%{searchTerm}%") ||
                                   EF.Functions.Like(c.CompanyContactName, $"%{searchTerm}%"));
        }
        
        query = query.OrderBy(c => c.CompanyName); // Add ordering for consistent pagination
        
        return await query.ToPaginatedListAsync(pageIndex, pageSize);
    }

    public async Task<Company> GetByIdAsync(int id)
    {
        return await _context.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CompanyID == id);
    }

    public async Task<Company> GetByIdWithEmployeesAsync(int id)
    {
        return await _context.Companies
            .Include(c => c.Employees)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CompanyID == id);
    }

    public async Task AddAsync(Company entity)
    {
        await _context.Companies.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Company entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var company = await _context.Companies.FindAsync(id);
        if (company != null)
        {
            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
        }
    }

    public async Task ForceDeleteAsync(int id)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Get all employees for this company
            var employees = await _context.Employees
                .Where(e => e.CompanyID == id)
                .ToListAsync();

            if (employees.Any())
            {
                var employeeIds = employees.Select(e => e.EmployeeID).ToList();

                // Get all AssetInspections performed by these employees
                var assetInspections = await _context.AssetInspections
                    .Where(ai => employeeIds.Contains(ai.EmployeeID))
                    .ToListAsync();

                if (assetInspections.Any())
                {
                    var assetInspectionIds = assetInspections.Select(ai => ai.AssetInspectionID).ToList();

                    // Remove AssetInspectionCheckLists first
                    var inspectionCheckLists = await _context.AssetInspectionCheckLists
                        .Where(aicl => assetInspectionIds.Contains(aicl.AssetInspectionID))
                        .ToListAsync();

                    if (inspectionCheckLists.Any())
                    {
                        _context.AssetInspectionCheckLists.RemoveRange(inspectionCheckLists);
                    }

                    // Remove InspectionPhotos if they exist
                    try
                    {
                        var inspectionPhotos = await _context.InspectionPhotos
                            .Where(ip => assetInspectionIds.Contains(ip.AssetInspectionID))
                            .ToListAsync();

                        if (inspectionPhotos.Any())
                        {
                            _context.InspectionPhotos.RemoveRange(inspectionPhotos);
                        }
                    }
                    catch (Exception)
                    {
                        // InspectionPhotos table might not exist or might be configured differently
                        // Continue without removing photos
                    }

                    // Remove AssetInspections
                    _context.AssetInspections.RemoveRange(assetInspections);
                }

                // Remove all employees
                _context.Employees.RemoveRange(employees);
            }

            // Finally, remove the company
            var company = await _context.Companies.FindAsync(id);
            if (company != null)
            {
                _context.Companies.Remove(company);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<PaginatedList<Company>> SerchCompanyAsync(string seachterm, int? pageIndex, int? pageSize)
    {
        var query =  _context.Companies
            .Where(x => EF.Functions.Like(x.CompanyName, $"%{seachterm}%"))
            .AsNoTracking()
            .OrderBy(c => c.CompanyName)
            .ToPaginatedListAsync(pageIndex,pageSize);
            return await query;
    }

    public async Task<bool> ExistsByNameAsync(string companyName, int? excludeId = null)
    {
        var normalizedName = companyName.Trim().ToLowerInvariant();
        
        var query = _context.Companies
            .Where(c => c.CompanyName.ToLower() == normalizedName);
        
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.CompanyID != excludeId.Value);
        }
        
        return await query.AnyAsync();
    }

    public async Task<bool> ExistsByEmailAsync(string email, int? excludeId = null)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        
        var query = _context.Companies
            .Where(c => c.CompanyAdminEmail.ToLower() == normalizedEmail);
        
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.CompanyID != excludeId.Value);
        }
        
        return await query.AnyAsync();
    }

    public async Task<CompanyRelatedDataInfo> GetCompanyRelatedDataAsync(int companyId)
    {
        var result = new CompanyRelatedDataInfo();

        try
        {
            // Get all employees for this company
            var employees = await _context.Employees
                .Where(e => e.CompanyID == companyId)
                .AsNoTracking()
                .ToListAsync();

            result.EmployeeCount = employees.Count;
            result.EmployeeNames = employees.Select(e => e.EmployeeName).OrderBy(name => name).ToList();

            if (employees.Any())
            {
                var employeeIds = employees.Select(e => e.EmployeeID).ToList();

                // Get all inspections performed by these employees
                var inspections = await _context.AssetInspections
                    .Include(ai => ai.Asset)
                    .Where(ai => employeeIds.Contains(ai.EmployeeID))
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
            }
        }
        catch (Exception ex)
        {
            // Log error but return empty result rather than throw
            // The calling method will handle logging
            result = new CompanyRelatedDataInfo();
        }

        return result;
    }
}
