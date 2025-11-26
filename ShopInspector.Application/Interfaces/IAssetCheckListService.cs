using ShopInspector.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Application.Interfaces;
public interface IAssetCheckListService
{
    Task<PaginatedList<AssetCheckList>> GetAllAsync(int? pageIndex, int? pageSize, string searchTerm = "");
    Task<List<AssetCheckList>> GetByAssetIdAsync(int assetId);
    Task<AssetCheckList?> GetByIdAsync(int id);
    Task<AssetCheckList?> GetByAssetAndChecklistAsync(int assetId, int inspectionCheckListId);
    Task AddAsync(AssetCheckList entity);
    Task UpdateAsync(AssetCheckList entity);
    Task DeleteAsync(int id);
    Task ForceDeleteAsync(int id);
    Task<bool> CanDeleteAsync(int assetCheckListId);
    Task<AssetCheckListRelatedDataInfo> GetAssetCheckListRelatedDataAsync(int assetCheckListId);
    
    // New helper methods for clean controller code
    Task<List<(int AssetID, string DisplayText)>> GetAssetDropdownDataAsync();
    Task<List<(int AssetCheckListID, int InspectionCheckListID, string InspectionCheckListName, string InspectionCheckListDescription, string InspectionCheckListTitle, bool Active)>> GetInspectionItemsForAssetAsync(int assetId);
    Task<List<(int EmployeeID, string EmployeeName)>> GetEmployeeDropdownDataAsync();
    Task<List<(int InspectionFrequencyID, string FrequencyName)>> GetInspectionFrequencyDropdownDataAsync();
}
