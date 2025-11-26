using ShopInspector.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Application.Interfaces;
public interface IInspectionCheckListService
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
