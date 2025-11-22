using ShopInspector.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Application.Interfaces;
public  interface IAssetInspectionRepository
{
    Task AddAsync(AssetInspection entity);
    Task UpdateAsync(AssetInspection entity);
    Task<AssetInspection?> GetByIdAsync(int id);
    Task<List<AssetInspection>> GetByAssetIdAsync(int assetId);
    Task AddCheckListRowAsync(AssetInspectionCheckList row);
}
