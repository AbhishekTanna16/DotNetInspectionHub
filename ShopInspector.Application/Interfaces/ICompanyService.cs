using ShopInspector.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Application.Interfaces;
public interface ICompanyService
{

    Task<PaginatedList<Company>> GetAllAsync(int? pageIndex, int? pageSize, string searchTerm = "");
    Task<Company> GetByIdAsync(int id);
    Task AddAsync(Company entity);

    Task<PaginatedList<Company>> SerchCompanyAsync(string seachterm, int? pageIndex, int? pageSize);
    Task UpdateAsync(Company entity);
    Task DeleteAsync(int id);
    Task ForceDeleteAsync(int id);
    
    // Enhanced methods for better business logic separation
    Task<bool> IsCompanyNameExistsAsync(string companyName, int? excludeCompanyId = null);
    Task<bool> IsEmailExistsAsync(string email, int? excludeCompanyId = null);
    Task<bool> CanDeleteCompanyAsync(int companyId);
    Task<Company> CreateCompanyAsync(string companyName, string adminEmail, string contactName, bool active, string createdBy);
    Task<Company> UpdateCompanyAsync(int companyId, string companyName, string adminEmail, string contactName, bool active);
    Task<CompanyRelatedDataInfo> GetCompanyRelatedDataAsync(int companyId);
}

// Add this class to hold the related data information for companies
public class CompanyRelatedDataInfo
{
    public int EmployeeCount { get; set; }
    public int TotalInspections { get; set; }
    public DateTime? LastInspectionDate { get; set; }
    public List<string> EmployeeNames { get; set; } = new();
    public List<string> AffectedAssetNames { get; set; } = new();
}
