using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Core.Entities;
public class Asset
{
    public int AssetID { get; set; }           // matches smallint
    public string AssetName { get; set; }
    public string AssetLocation { get; set; }
    public int AssetTypeID { get; set; }
    public string AssetCode { get; set; }
    public string Department { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; }

    // Navigation
    public AssetType AssetType { get; set; }
    public List<AssetCheckList> AssetCheckLists { get; set; }
    public List<AssetInspection> AssetInspections { get; set; }
}
