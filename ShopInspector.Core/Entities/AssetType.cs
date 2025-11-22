using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Core.Entities;
public class AssetType
{
    public int  AssetTypeID { get; set; }
    public string AssetTypeName { get; set; }

    // Navigation
    public List<Asset> Assets { get; set; }
}
