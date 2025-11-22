using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Core.Entities;
public class InspectionCheckList
{
    public int InspectionCheckListID { get; set; }
    public string InspectionCheckListName { get; set; }
    public string InspectionCheckListDescription { get; set; }
    public string InspectionCheckListTitle { get; set; }
    public bool Active { get; set; }

    // Navigation
  public List<AssetCheckList> AssetCheckLists { get; set; }
}
