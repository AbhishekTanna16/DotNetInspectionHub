using ShopInspector.Application.Interfaces;
using ShopInspector.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Application.Services;
public class InspectionCheckListService : IInspectionCheckListService
{
    private readonly IInspectionCheckListRepository _repo;
    public InspectionCheckListService(IInspectionCheckListRepository repo) => _repo = repo;

    public Task<List<InspectionCheckList>> GetAllAsync() => _repo.GetAllAsync();
    public Task<InspectionCheckList?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);
    public Task AddAsync(InspectionCheckList entity) => _repo.AddAsync(entity);
    public Task UpdateAsync(InspectionCheckList entity) => _repo.UpdateAsync(entity);
    public Task DeleteAsync(int id) => _repo.DeleteAsync(id);
}


