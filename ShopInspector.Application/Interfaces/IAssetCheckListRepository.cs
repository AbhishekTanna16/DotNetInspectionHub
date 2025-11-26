using ShopInspector.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Application.Interfaces;
public interface IAssetCheckListRepository
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
    
    // New helper methods for dropdown data and formatted results
    Task<List<(int AssetID, string DisplayText)>> GetAssetDropdownDataAsync();
    Task<List<(int AssetCheckListID, int InspectionCheckListID, string InspectionCheckListName, string InspectionCheckListDescription, string InspectionCheckListTitle, bool Active)>> GetInspectionItemsForAssetAsync(int assetId);
    Task<List<(int EmployeeID, string EmployeeName)>> GetEmployeeDropdownDataAsync();
    Task<List<(int InspectionFrequencyID, string FrequencyName)>> GetInspectionFrequencyDropdownDataAsync();
    Task<AssetCheckListRelatedDataInfo> GetAssetCheckListRelatedDataAsync(int assetCheckListId);
}

// Add this class to hold the related data information
public class AssetCheckListRelatedDataInfo
{
    public int TotalInspectionRecords { get; set; }
    public DateTime? FirstInspectionDate { get; set; }
    public DateTime? LastInspectionDate { get; set; }
    public List<string> EmployeeNames { get; set; } = new();
    public List<string> InspectionFrequencies { get; set; } = new();
    public string AssetName { get; set; } = string.Empty;
    public string CheckListName { get; set; } = string.Empty;
}
