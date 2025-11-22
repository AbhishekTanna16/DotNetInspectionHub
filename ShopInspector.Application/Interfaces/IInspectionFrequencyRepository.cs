using ShopInspector.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Application.Interfaces;
public interface IInspectionFrequencyRepository
{
    Task<List<InspectionFrequency>> GetAllAsync();
    Task<InspectionFrequency?> GetByIdAsync(int id);
    Task AddAsync(InspectionFrequency entity);
    Task UpdateAsync(InspectionFrequency entity);
    Task DeleteAsync(int id);
}
