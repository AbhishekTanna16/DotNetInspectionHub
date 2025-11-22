using Microsoft.AspNetCore.Mvc.Rendering;
namespace ShopInspector.Web.Areas.Admin.Models;

public class InspectionItemViewModel
{
   public int AssetCheckListID { get; set; }
    public int InspectionCheckListID { get; set; }
    public string InspectionCheckListName { get; set; }
    public string InspectionCheckListDescription { get; set; }
    public string InspectionCheckListTitle { get; set; }
    public bool Active { get; set; }

}
