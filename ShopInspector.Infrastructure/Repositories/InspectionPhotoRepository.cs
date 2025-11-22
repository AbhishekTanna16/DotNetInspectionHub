using Microsoft.EntityFrameworkCore;
using ShopInspector.Application.Interfaces;
using ShopInspector.Core.Entities;
using ShopInspector.Infrastructure.Data;

namespace ShopInspector.Infrastructure.Repositories;

public class InspectionPhotoRepository : IInspectionPhotoRepository
{
    private readonly AppDbContext _db;

    public InspectionPhotoRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(InspectionPhoto photo)
    {
        await _db.InspectionPhotos.AddAsync(photo);
        await _db.SaveChangesAsync();
    }

    public async Task<List<InspectionPhoto>> GetByInspectionIdAsync(int inspectionId)
    {
        return await _db.InspectionPhotos
            .Where(p => p.AssetInspectionID == inspectionId)
            .OrderBy(p => p.DisplayOrder)
            .ToListAsync();
    }

    public async Task DeleteAsync(int photoId)
    {
        var photo = await _db.InspectionPhotos.FindAsync(photoId);
        if (photo != null)
        {
            _db.InspectionPhotos.Remove(photo);
            await _db.SaveChangesAsync();
        }
    }
}
