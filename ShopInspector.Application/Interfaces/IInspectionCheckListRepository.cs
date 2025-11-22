using ShopInspector.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Application.Interfaces;
public interface IInspectionCheckListRepository
{
    Task<List<InspectionCheckList>> GetAllAsync();
    Task<InspectionCheckList?> GetByIdAsync(int id);
    Task AddAsync(InspectionCheckList entity);
    Task UpdateAsync(InspectionCheckList entity);
    Task DeleteAsync(int id);
}
