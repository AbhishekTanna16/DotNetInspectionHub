using ShopInspector.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Application.Interfaces;
public interface IAssetService
{
    Task<List<Asset>> GetAllAsync();
    Task<Asset?> GetByIdAsync(int id);
    Task AddAsync(Asset asset);
    Task UpdateAsync(Asset asset);
    Task DeleteAsync(int id);
}
