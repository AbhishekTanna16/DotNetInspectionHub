using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc.Rendering;
namespace ShopInspector.Web.Areas.Admin.Models;

public class InspectionItemViewModel
{
   public int AssetCheckListID { get; set; }
    public int InspectionCheckListID { get; set; }

    [Required(ErrorMessage = "Check List Name is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Check List Name must be between 2 and 200 characters")]
    [Display(Name = "Check List Name")]
    public string InspectionCheckListName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Description must be between 5 and 500 characters")]
    [Display(Name = "Description")]
    public string InspectionCheckListDescription { get; set; } = string.Empty;

    [Required(ErrorMessage = "Title is required")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Title must be between 2 and 150 characters")]
    [Display(Name = "Title")]
    public string InspectionCheckListTitle { get; set; } = string.Empty;

    [Display(Name = "Active")]
    public bool Active { get; set; } = true;
}

public class InspectionCheckListCreateViewModel
{
    [Required(ErrorMessage = "Check List Name is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Check List Name must be between 2 and 200 characters")]
    [Display(Name = "Check List Name")]
    public string InspectionCheckListName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Description must be between 5 and 500 characters")]
    [Display(Name = "Description")]
    public string InspectionCheckListDescription { get; set; } = string.Empty;

    [Required(ErrorMessage = "Title is required")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Title must be between 2 and 150 characters")]
    [Display(Name = "Title")]
    public string InspectionCheckListTitle { get; set; } = string.Empty;

    [Display(Name = "Active")]
    public bool Active { get; set; } = true;
}

public class InspectionCheckListEditViewModel
{
    public int InspectionCheckListID { get; set; }

    [Required(ErrorMessage = "Check List Name is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Check List Name must be between 2 and 200 characters")]
    [Display(Name = "Check List Name")]
    public string InspectionCheckListName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Description must be between 5 and 500 characters")]
    [Display(Name = "Description")]
    public string InspectionCheckListDescription { get; set; } = string.Empty;

    [Required(ErrorMessage = "Title is required")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Title must be between 2 and 150 characters")]
    [Display(Name = "Title")]
    public string InspectionCheckListTitle { get; set; } = string.Empty;

    [Display(Name = "Active")]
    public bool Active { get; set; } = true;
}

public class InspectionCheckListListViewModel
{
    public int InspectionCheckListID { get; set; }
    public string InspectionCheckListName { get; set; } = string.Empty;
    public string InspectionCheckListDescription { get; set; } = string.Empty;
    public string InspectionCheckListTitle { get; set; } = string.Empty;
    public bool Active { get; set; }
    public int AssetCheckListsCount { get; set; }
}
