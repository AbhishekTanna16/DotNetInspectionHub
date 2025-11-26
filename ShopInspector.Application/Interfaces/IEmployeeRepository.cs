using ShopInspector.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Application.Interfaces;
public interface IEmployeeRepository
{
    Task<PaginatedList<Employee>> GetAllAsync(int? pageIndex, int? pageSize, string searchTerm = "");
    Task<Employee?> GetByIdAsync(int id);
    Task<PaginatedList<Employee>> SerchEmployeeAsync(string seachterm, int? pageIndex, int? pageSize);
    Task AddAsync(Employee entity);
    Task UpdateAsync(Employee entity);
    Task DeleteAsync(int id);
    Task ForceDeleteAsync(int id);
    
    // Enhanced methods for business logic
    Task<bool> ExistsByNameAndCompanyAsync(string employeeName, int companyId, int? excludeEmployeeId = null);
    Task<int> GetEmployeeCountByCompanyAsync(int companyId);
    Task<List<Employee>> GetActiveEmployeesByCompanyAsync(int companyId);
    Task<Employee> GetByIdWithCompanyAsync(int id);
    Task<bool> CanDeleteEmployeeAsync(int employeeId);
    Task<EmployeeRelatedDataInfo> GetEmployeeRelatedDataAsync(int employeeId);
}
