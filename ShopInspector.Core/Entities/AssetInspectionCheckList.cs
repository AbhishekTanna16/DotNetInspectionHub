using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Core.Entities;
public class AssetInspectionCheckList
{
    public long AssetInspectionCheckListID { get; set; }  // bigint
    public int AssetInspectionID { get; set; }
    public int AssetCheckListID { get; set; }
    public bool IsChecked { get; set; }
    public string? Remarks { get; set; }  // to store remarks/notes

    // Navigation
    public AssetInspection AssetInspection { get; set; }
    public AssetCheckList AssetCheckList { get; set; }
}
