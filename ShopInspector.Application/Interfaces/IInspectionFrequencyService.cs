using ShopInspector.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Application.Interfaces;

public interface IInspectionFrequencyService
{
    Task<PaginatedList<InspectionFrequency>> GetAllAsync(int? pageIndex, int? pageSize, string searchTerm = "");
    Task<InspectionFrequency?> GetByIdAsync(int id);
    Task AddAsync(InspectionFrequency entity);
    Task UpdateAsync(InspectionFrequency entity);
    Task DeleteAsync(int id);
    Task ForceDeleteAsync(int id);
    
    // Enhanced business logic methods
    Task<bool> IsFrequencyNameExistsAsync(string frequencyName, int? excludeFrequencyId = null);
    Task<InspectionFrequency> CreateFrequencyAsync(string frequencyName);
    Task<InspectionFrequency> UpdateFrequencyAsync(int frequencyId, string frequencyName);
    Task<bool> CanDeleteFrequencyAsync(int frequencyId);
    Task<InspectionFrequencyRelatedDataInfo> GetFrequencyRelatedInspectionsAsync(int frequencyId);
}
