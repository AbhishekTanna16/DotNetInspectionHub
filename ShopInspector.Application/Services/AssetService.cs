using ShopInspector.Application.Interfaces;
using ShopInspector.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Application.Services;
public class AssetService : IAssetService
{
    private readonly IAssetRepository _repo;
    public AssetService(IAssetRepository repo) => _repo = repo;

    public Task<PaginatedList<Asset>> GetAllAsync(int? pageIndex, int? pageSize) => _repo.GetAllAsync(pageIndex, pageSize);

    public Task<Asset?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);

    public Task AddAsync(Asset asset) => _repo.AddAsync(asset);

    public Task UpdateAsync(Asset asset) => _repo.UpdateAsync(asset);

    public Task DeleteAsync(int id) => _repo.DeleteAsync(id);

    public Task<PaginatedList<Asset>> SerchAssetAsync(string seachterm, int? pageIndex, int? pageSize) => _repo.SerchAssetAsync(seachterm, pageIndex, pageSize);

    // New helper methods for Asset Controller dropdown data
    public Task<List<(int AssetTypeID, string AssetTypeName)>> GetAssetTypeDropdownDataAsync() => _repo.GetAssetTypeDropdownDataAsync();
    public Task<List<(int EmployeeID, string EmployeeName)>> GetEmployeeDropdownDataAsync() => _repo.GetEmployeeDropdownDataAsync();
    public Task<List<(int CompanyID, string CompanyName)>> GetCompanyDropdownDataAsync() => _repo.GetCompanyDropdownDataAsync();
    public Task<(
        List<(int AssetTypeID, string AssetTypeName)> AssetTypes,
        List<(int EmployeeID, string EmployeeName)> Employees,
        List<(int CompanyID, string CompanyName)> Companies
    )> GetAssetFormDropdownDataAsync() => _repo.GetAssetFormDropdownDataAsync();
}

