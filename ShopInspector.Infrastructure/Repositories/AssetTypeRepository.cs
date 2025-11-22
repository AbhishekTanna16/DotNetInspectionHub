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
public class AssetTypeRepository : IAssetTypeRepository
{
    private readonly AppDbContext _db;

    public AssetTypeRepository(AppDbContext db) => _db = db;

    public async Task<List<AssetType>> GetAllAsync() =>
        await _db.AssetTypes.ToListAsync();

    public async Task<AssetType?> GetByIdAsync(int id) =>
        await _db.AssetTypes.FindAsync(id);

    public async Task AddAsync(AssetType entity)
    {
        await _db.AssetTypes.AddAsync(entity);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(AssetType entity)
    {
        _db.AssetTypes.Update(entity);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var o = await _db.AssetTypes.FindAsync(id);
        if (o != null)
        {
            _db.AssetTypes.Remove(o);
            await _db.SaveChangesAsync();
        }
    }
}

