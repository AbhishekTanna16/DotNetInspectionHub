using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ShopInspector.Web.Areas.Admin.Models;

public class AssetCheckListEditViewModel
{
    public int? AssetCheckListID { get; set; }

    [Required]
    public int AssetID { get; set; }

    [Required]
    public int InspectionCheckListID { get; set; }

    public int DisplayOrder { get; set; } = 1;

    public bool Active { get; set; } = true;

    // Dropdowns
    public IEnumerable<SelectListItem>? Assets { get; set; }
    public IEnumerable<SelectListItem>? InspectionCheckLists { get; set; }
    
    // Alias for backward compatibility with views
    public IEnumerable<SelectListItem>? CheckLists => InspectionCheckLists;
}
