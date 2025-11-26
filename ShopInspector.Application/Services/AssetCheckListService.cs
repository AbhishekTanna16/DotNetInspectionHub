using Microsoft.Extensions.Logging;
using ShopInspector.Application.Interfaces;
using ShopInspector.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Application.Services;

public class AssetCheckListService : IAssetCheckListService
{
    private readonly IAssetCheckListRepository _repo;
    private readonly ILogger<AssetCheckListService> _logger;

    public AssetCheckListService(IAssetCheckListRepository repo, ILogger<AssetCheckListService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public Task<PaginatedList<AssetCheckList>> GetAllAsync(int? pageIndex, int? pageSize, string searchTerm = "") => 
        _repo.GetAllAsync(pageIndex, pageSize, searchTerm);
    public Task<List<AssetCheckList>> GetByAssetIdAsync(int assetId) => _repo.GetByAssetIdAsync(assetId);
    public Task<AssetCheckList?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);
    public Task<AssetCheckList?> GetByAssetAndChecklistAsync(int assetId, int inspectionCheckListId) => 
        _repo.GetByAssetAndChecklistAsync(assetId, inspectionCheckListId);
    public Task AddAsync(AssetCheckList entity) => _repo.AddAsync(entity);
    public Task UpdateAsync(AssetCheckList entity) => _repo.UpdateAsync(entity);
    public Task DeleteAsync(int id) => _repo.DeleteAsync(id);
    
    public async Task ForceDeleteAsync(int id)
    {
        try
        {
            await _repo.ForceDeleteAsync(id);
            _logger.LogWarning("Force deleted asset checklist with ID {AssetCheckListId} and all associated data", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error force deleting asset checklist with ID {AssetCheckListId}", id);
            throw;
        }
    }
    
    public Task<bool> CanDeleteAsync(int assetCheckListId) => _repo.CanDeleteAsync(assetCheckListId);
    
    public async Task<AssetCheckListRelatedDataInfo> GetAssetCheckListRelatedDataAsync(int assetCheckListId)
    {
        try
        {
            return await _repo.GetAssetCheckListRelatedDataAsync(assetCheckListId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting related data for asset checklist {AssetCheckListId}", assetCheckListId);
            return new AssetCheckListRelatedDataInfo();
        }
    }
    
    // New helper methods for clean controller code
    public Task<List<(int AssetID, string DisplayText)>> GetAssetDropdownDataAsync() => _repo.GetAssetDropdownDataAsync();
    public Task<List<(int AssetCheckListID, int InspectionCheckListID, string InspectionCheckListName, string InspectionCheckListDescription, string InspectionCheckListTitle, bool Active)>> GetInspectionItemsForAssetAsync(int assetId) => _repo.GetInspectionItemsForAssetAsync(assetId);
    public Task<List<(int EmployeeID, string EmployeeName)>> GetEmployeeDropdownDataAsync() => _repo.GetEmployeeDropdownDataAsync();
    public Task<List<(int InspectionFrequencyID, string FrequencyName)>> GetInspectionFrequencyDropdownDataAsync() => _repo.GetInspectionFrequencyDropdownDataAsync();
}

