using ShopInspector.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Application.Interfaces;
public interface ICompanyRepository
{
    Task<PaginatedList<Company>> GetAllAsync(int? pageIndex, int? pageSize, string searchTerm = "");
    Task<Company> GetByIdAsync(int id);
    Task<PaginatedList<Company>> SerchCompanyAsync(string seachterm, int? pageIndex, int? pageSize);
    Task AddAsync(Company entity);
    Task UpdateAsync(Company entity);
    Task DeleteAsync(int id);
    Task ForceDeleteAsync(int id);
    
    // Enhanced methods for validation
    Task<bool> ExistsByNameAsync(string companyName, int? excludeId = null);
    Task<bool> ExistsByEmailAsync(string email, int? excludeId = null);
    Task<Company> GetByIdWithEmployeesAsync(int id);
    Task<CompanyRelatedDataInfo> GetCompanyRelatedDataAsync(int companyId);
}
