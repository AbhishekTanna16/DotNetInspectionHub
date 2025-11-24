using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ShopInspector.Web.Areas.Admin.Models;

public class AssetCheckListEditViewModel
{
    public int? AssetCheckListID { get; set; }

    [Required]
    public int AssetID { get; set; }

    // Primary property for multiple checklist selection
    [Required]
    public List<int> InspectionCheckListIDs { get; set; } = new();

    public int DisplayOrder { get; set; } = 1;
    public bool Active { get; set; } = true;

    // Dropdowns
    public IEnumerable<SelectListItem>? Assets { get; set; }
    public IEnumerable<SelectListItem>? InspectionCheckLists { get; set; }

    // For backward compatibility with existing single checklist operations (Edit mode)
    [Obsolete("Use InspectionCheckListIDs for multi-select functionality")]
    public int InspectionCheckListID 
    { 
        get => InspectionCheckListIDs.FirstOrDefault();
        set 
        {
            if (value > 0 && !InspectionCheckListIDs.Contains(value))
            {
                InspectionCheckListIDs.Clear();
                InspectionCheckListIDs.Add(value);
            }
        }
    }
}
