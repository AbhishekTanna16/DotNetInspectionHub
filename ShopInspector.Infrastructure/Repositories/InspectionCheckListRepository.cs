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
public class InspectionCheckListRepository : IInspectionCheckListRepository
{
    private readonly AppDbContext _db;
    public InspectionCheckListRepository(AppDbContext db) => _db = db;

    public async Task<List<InspectionCheckList>> GetAllAsync()
    {
        return await _db.InspectionCheckLists
                        .OrderBy(i => i.InspectionCheckListName)
                        .ToListAsync();
    }

    public async Task<InspectionCheckList?> GetByIdAsync(int id)
    {
        return await _db.InspectionCheckLists.FindAsync(id);
    }

    public async Task AddAsync(InspectionCheckList entity)
    {
        await _db.InspectionCheckLists.AddAsync(entity);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(InspectionCheckList entity)
    {
        _db.InspectionCheckLists.Update(entity);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var item = await _db.InspectionCheckLists.FindAsync(id);
        if (item == null) return;
        _db.InspectionCheckLists.Remove(item);
        await _db.SaveChangesAsync();
    }
}


