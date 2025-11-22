using ShopInspector.Application.Interfaces;
using ShopInspector.Core.Entities;

namespace ShopInspector.Application.Services;

public class InspectionPhotoService : IInspectionPhotoService
{
    private readonly IInspectionPhotoRepository _repo;

    public InspectionPhotoService(IInspectionPhotoRepository repo) => _repo = repo;

    public Task AddAsync(InspectionPhoto photo) => _repo.AddAsync(photo);

    public Task<List<InspectionPhoto>> GetByInspectionIdAsync(int inspectionId) 
        => _repo.GetByInspectionIdAsync(inspectionId);

    public Task DeleteAsync(int photoId) => _repo.DeleteAsync(photoId);
}
