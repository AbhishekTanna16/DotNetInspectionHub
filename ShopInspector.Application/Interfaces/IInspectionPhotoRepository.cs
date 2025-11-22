using ShopInspector.Core.Entities;

namespace ShopInspector.Application.Interfaces;

public interface IInspectionPhotoRepository
{
    Task AddAsync(InspectionPhoto photo);
    Task<List<InspectionPhoto>> GetByInspectionIdAsync(int inspectionId);
    Task DeleteAsync(int photoId);
}
