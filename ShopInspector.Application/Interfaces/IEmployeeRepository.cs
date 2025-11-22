using ShopInspector.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Application.Interfaces;
public interface IEmployeeRepository
{
    Task<List<Employee>> GetAllAsync();
    Task<Employee?> GetByIdAsync(int id);
    Task AddAsync(Employee entity);
    Task UpdateAsync(Employee entity);
    Task DeleteAsync(int id);
}
