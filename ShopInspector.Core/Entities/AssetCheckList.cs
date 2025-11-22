using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Core.Entities;
public class AssetCheckList
{
    public int AssetCheckListID { get; set; }
    public int  AssetID { get; set; }
    public int InspectionCheckListID { get; set; }
    public int DisplayOrder { get; set; }
    public bool Active { get; set; }

    // Navigation
    public Asset Asset { get; set; }
    public InspectionCheckList InspectionCheckList { get; set; }
    public List<AssetInspectionCheckList> AssetInspectionCheckLists { get; set; }
}
