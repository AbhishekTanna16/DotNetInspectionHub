using ShopInspector.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Application.Interfaces;
public interface IAssetService
{
    Task<PaginatedList<Asset>> GetAllAsync(int? pageIndex, int? pageSize);
    Task<Asset?> GetByIdAsync(int id);
    Task AddAsync(Asset asset);
    Task UpdateAsync(Asset asset);
    Task DeleteAsync(int id);
    Task<PaginatedList<Asset>> SerchAssetAsync(string seachterm, int? pageIndex, int? pageSize);

    // New helper methods for Asset Controller dropdown data
    Task<List<(int AssetTypeID, string AssetTypeName)>> GetAssetTypeDropdownDataAsync();
    Task<List<(int EmployeeID, string EmployeeName)>> GetEmployeeDropdownDataAsync();
    Task<List<(int CompanyID, string CompanyName)>> GetCompanyDropdownDataAsync();
    Task<(
        List<(int AssetTypeID, string AssetTypeName)> AssetTypes,
        List<(int EmployeeID, string EmployeeName)> Employees,
        List<(int CompanyID, string CompanyName)> Companies
    )> GetAssetFormDropdownDataAsync();
}
