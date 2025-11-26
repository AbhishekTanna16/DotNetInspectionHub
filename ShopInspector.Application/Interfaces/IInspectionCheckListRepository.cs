using ShopInspector.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Application.Interfaces;
public interface IInspectionCheckListRepository
{
    Task<PaginatedList<InspectionCheckList>> GetAllAsync(int? pageIndex, int? pageSize);
    Task<PaginatedList<InspectionCheckList>> GetAllAsync(int? pageIndex, int? pageSize, string? searchTerm);
    Task<InspectionCheckList?> GetByIdAsync(int id);
    Task AddAsync(InspectionCheckList entity);
    Task UpdateAsync(InspectionCheckList entity);
    Task DeleteAsync(int id);
    Task ForceDeleteAsync(int id);
    Task<bool> CanDeleteCheckListAsync(int id);
    
    Task<InspectionCheckListRelatedDataInfo> GetCheckListRelatedDataAsync(int checkListId);
}

// Add this class to hold the related data information
public class InspectionCheckListRelatedDataInfo
{
    public int TotalAssetCheckLists { get; set; }
    public int TotalInspectionItems { get; set; }
    public List<string> AffectedAssetNames { get; set; } = new();
    public List<string> AssignedCompanyNames { get; set; } = new();
    public DateTime? LastUsedDate { get; set; }
    public DateTime? FirstUsedDate { get; set; }
}
