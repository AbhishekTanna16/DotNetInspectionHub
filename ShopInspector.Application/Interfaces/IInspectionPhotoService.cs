using ShopInspector.Core.Entities;

namespace ShopInspector.Application.Interfaces;

public interface IInspectionPhotoService
{
    Task AddAsync(InspectionPhoto photo);
    Task<List<InspectionPhoto>> GetByInspectionIdAsync(int inspectionId);
    Task DeleteAsync(int photoId);
}
