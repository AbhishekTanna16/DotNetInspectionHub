using ShopInspector.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Application.Interfaces;
public interface ICompanyService
{
    Task<List<Company>> GetAllAsync();
    Task<Company> GetByIdAsync(int id);
    Task AddAsync(Company entity);
    Task UpdateAsync(Company entity);
    Task DeleteAsync(int id);
}
