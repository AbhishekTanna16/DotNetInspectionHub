using ShopInspector.Application.Interfaces;
using ShopInspector.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Application.Services;
public class InspectionFrequencyService : IInspectionFrequencyService
{
    private readonly IInspectionFrequencyRepository _repo;

    public InspectionFrequencyService(IInspectionFrequencyRepository repo)
    {
        _repo = repo;
    }

    public Task<List<InspectionFrequency>> GetAllAsync()
        => _repo.GetAllAsync();

    public Task<InspectionFrequency?> GetByIdAsync(int id)
        => _repo.GetByIdAsync(id);

    public Task AddAsync(InspectionFrequency entity)
        => _repo.AddAsync(entity);

    public Task UpdateAsync(InspectionFrequency entity)
        => _repo.UpdateAsync(entity);

    public Task DeleteAsync(int id)
        => _repo.DeleteAsync(id);
}


