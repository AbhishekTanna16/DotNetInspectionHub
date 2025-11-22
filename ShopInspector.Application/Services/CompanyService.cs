using ShopInspector.Application.Interfaces;
using ShopInspector.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Application.Services;
public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _repository;

    public CompanyService(ICompanyRepository repository)
    {
        _repository = repository;
    }

    public Task<List<Company>> GetAllAsync() =>
        _repository.GetAllAsync();

    public Task<Company> GetByIdAsync(int id) =>
        _repository.GetByIdAsync(id);

    public Task AddAsync(Company entity) =>
        _repository.AddAsync(entity);

    public Task UpdateAsync(Company entity) =>
        _repository.UpdateAsync(entity);

    public Task DeleteAsync(int id) =>
        _repository.DeleteAsync(id);
}

