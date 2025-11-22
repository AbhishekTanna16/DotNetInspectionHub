using ShopInspector.Application.Interfaces;
using ShopInspector.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Application.Services;
public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repo;

    public EmployeeService(IEmployeeRepository repo)
    {
        _repo = repo;
    }

    public Task<List<Employee>> GetAllAsync() => _repo.GetAllAsync();

    public Task<Employee?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);

    public Task AddAsync(Employee entity) => _repo.AddAsync(entity);

    public Task UpdateAsync(Employee entity) => _repo.UpdateAsync(entity);

    public Task DeleteAsync(int id) => _repo.DeleteAsync(id);
}

