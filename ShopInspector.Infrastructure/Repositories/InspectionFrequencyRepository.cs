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
public class InspectionFrequencyRepository : IInspectionFrequencyRepository
{
    private readonly AppDbContext _db;

    public InspectionFrequencyRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<InspectionFrequency>> GetAllAsync()
    {
        return await _db.InspectionFrequencies
            .OrderBy(f => f.FrequencyName)
            .ToListAsync();
    }

    public async Task<InspectionFrequency?> GetByIdAsync(int id)
    {
        return await _db.InspectionFrequencies.FindAsync(id);
    }

    public async Task AddAsync(InspectionFrequency entity)
    {
        await _db.InspectionFrequencies.AddAsync(entity);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(InspectionFrequency entity)
    {
        _db.InspectionFrequencies.Update(entity);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var freq = await _db.InspectionFrequencies.FindAsync(id);
        if (freq == null) return;

        _db.InspectionFrequencies.Remove(freq);
        await _db.SaveChangesAsync();
    }
}
