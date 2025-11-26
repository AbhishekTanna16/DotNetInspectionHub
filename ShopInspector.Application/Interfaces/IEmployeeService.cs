using ShopInspector.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Application.Interfaces;

public interface IEmployeeService
{
    Task<PaginatedList<Employee>> GetAllAsync(int? pageIndex, int? pageSize, string searchTerm = "");
    Task<Employee?> GetByIdAsync(int id);
    Task<PaginatedList<Employee>> SerchEmployeeAsync(string seachterm, int? pageIndex, int? pageSize);
    Task AddAsync(Employee entity);
    Task UpdateAsync(Employee entity);
    Task DeleteAsync(int id);
    Task ForceDeleteAsync(int id);

    // Enhanced business logic methods
    Task<bool> IsEmployeeNameExistsInCompanyAsync(string employeeName, int companyId, int? excludeEmployeeId = null);
    Task<Employee> CreateEmployeeAsync(string employeeName, int companyId, bool active, string createdBy);
    Task<Employee> UpdateEmployeeAsync(int employeeId, string employeeName, int companyId, bool active);
    Task<bool> CanDeleteEmployeeAsync(int employeeId);
    Task<EmployeeRelatedDataInfo> GetEmployeeRelatedInspectionsAsync(int employeeId);
}

// Add this class to hold the related data information
public class EmployeeRelatedDataInfo
{
    public int TotalInspections { get; set; }
    public DateTime? LastInspectionDate { get; set; }
    public List<string> AffectedAssetNames { get; set; } = new();
    public int ChecklistItemsCount { get; set; }
}
