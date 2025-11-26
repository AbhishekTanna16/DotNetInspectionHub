using Microsoft.Extensions.Logging;
using ShopInspector.Application.Interfaces;
using ShopInspector.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Application.Services;
public class InspectionCheckListService : IInspectionCheckListService
{
    private readonly IInspectionCheckListRepository _repo;
    private readonly ILogger<InspectionCheckListService> _logger;
    
    public InspectionCheckListService(IInspectionCheckListRepository repo, ILogger<InspectionCheckListService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public Task<PaginatedList<InspectionCheckList>> GetAllAsync(int? pageIndex, int? pageSize) => _repo.GetAllAsync(pageIndex, pageSize);
    
    public Task<PaginatedList<InspectionCheckList>> GetAllAsync(int? pageIndex, int? pageSize, string? searchTerm) => _repo.GetAllAsync(pageIndex, pageSize, searchTerm);
    
    public Task<InspectionCheckList?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);
    public Task AddAsync(InspectionCheckList entity) => _repo.AddAsync(entity);
    public Task UpdateAsync(InspectionCheckList entity) => _repo.UpdateAsync(entity);
    public Task DeleteAsync(int id) => _repo.DeleteAsync(id);
    
    public async Task ForceDeleteAsync(int id)
    {
        try
        {
            await _repo.ForceDeleteAsync(id);
            _logger.LogWarning("Force deleted inspection checklist with ID {CheckListId} and all associated data", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error force deleting inspection checklist with ID {CheckListId}", id);
            throw;
        }
    }
    
    public Task<bool> CanDeleteCheckListAsync(int id) => _repo.CanDeleteCheckListAsync(id);
    
    public async Task<InspectionCheckListRelatedDataInfo> GetCheckListRelatedDataAsync(int checkListId)
    {
        try
        {
            return await _repo.GetCheckListRelatedDataAsync(checkListId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting related data for checklist {CheckListId}", checkListId);
            return new InspectionCheckListRelatedDataInfo();
        }
    }
}



