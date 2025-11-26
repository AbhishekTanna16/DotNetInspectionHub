using ShopInspector.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Application.Interfaces;
public interface IInspectionFrequencyRepository
{
    Task<PaginatedList<InspectionFrequency>> GetAllAsync(int? pageIndex, int? pageSize, string searchTerm = "");
    Task<InspectionFrequency?> GetByIdAsync(int id);
    Task AddAsync(InspectionFrequency entity);
    Task UpdateAsync(InspectionFrequency entity);
    Task DeleteAsync(int id);
    Task ForceDeleteAsync(int id);
    
    // Enhanced methods for validation
    Task<bool> ExistsByNameAsync(string frequencyName, int? excludeFrequencyId = null);
    Task<int> GetInspectionCountByFrequencyAsync(int frequencyId);
    Task<bool> CanDeleteFrequencyAsync(int frequencyId);
    Task<InspectionFrequency> GetByIdWithInspectionsAsync(int id);
    Task<InspectionFrequencyRelatedDataInfo> GetFrequencyRelatedDataAsync(int frequencyId);
}

// Add this class to hold the related data information
public class InspectionFrequencyRelatedDataInfo
{
    public int TotalInspections { get; set; }
    public DateTime? LastInspectionDate { get; set; }
    public DateTime? FirstInspectionDate { get; set; }
    public List<string> AffectedAssetNames { get; set; } = new();
    public List<string> AssignedEmployeeNames { get; set; } = new();
    public int ChecklistItemsCount { get; set; }
}
