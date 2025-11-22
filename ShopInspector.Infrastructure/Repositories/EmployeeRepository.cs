using Microsoft.EntityFrameworkCore;
using ShopInspector.Application.Interfaces;
using ShopInspector.Core.Entities;
using ShopInspector.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Infrastructure.Repositories;
public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _db;

    public EmployeeRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Employee>> GetAllAsync()
    {
        
        var Data =  await _db.Employees.ToListAsync();
        if(Data != null)
        {
            Data = await _db.Employees
          .Include(e => e.Company)
          .ToListAsync();
            return Data;
        }
        return null;
    }

    public async Task<Employee?> GetByIdAsync(int id)
    {
        return await _db.Employees
            .Include(e => e.Company)
            .FirstOrDefaultAsync(e => e.EmployeeID == id);
    }

    public async Task AddAsync(Employee entity)
    {
        await _db.Employees.AddAsync(entity);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Employee entity)
    {
        _db.Employees.Update(entity);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _db.Employees.FindAsync(id);
        if (entity != null)
        {
            _db.Employees.Remove(entity);
            await _db.SaveChangesAsync();
        }
    }
}
