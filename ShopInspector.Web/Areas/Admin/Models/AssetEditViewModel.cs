using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ShopInspector.Web.Areas.Admin.Models;

public class AssetEditViewModel
{
    public
        int? AssetID { get; set; }

    [Required, StringLength(100)]
    public string AssetName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? AssetLocation { get; set; }

    [Required]
    public int AssetTypeID { get; set; }

    [Required, StringLength(100)]
    public string AssetCode { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Department { get; set; }

    public bool Active { get; set; } = true;

    // dropdown lists
    public IEnumerable<SelectListItem>? Companies { get; set; }
    public IEnumerable<SelectListItem>? AssetTypes { get; set; }
    public IEnumerable<SelectListItem>? Employees { get; set; }

    // Helper method to populate dropdowns from repository data
    public void PopulateDropdowns(
        List<(int AssetTypeID, string AssetTypeName)> assetTypes,
        List<(int EmployeeID, string EmployeeName)> employees,
        List<(int CompanyID, string CompanyName)> companies)
    {
        AssetTypes = assetTypes.Select(at => new SelectListItem(
            at.AssetTypeName, 
            at.AssetTypeID.ToString(), 
            at.AssetTypeID == AssetTypeID));

        Employees = employees.Select(e => new SelectListItem(
            e.EmployeeName, 
            e.EmployeeID.ToString()));

        Companies = companies.Select(c => new SelectListItem(
            c.CompanyName, 
            c.CompanyID.ToString()));
    }

    // Helper method to populate dropdowns without selection (for Create)
    public void PopulateDropdownsForCreate(
        List<(int AssetTypeID, string AssetTypeName)> assetTypes,
        List<(int EmployeeID, string EmployeeName)> employees,
        List<(int CompanyID, string CompanyName)> companies)
    {
        AssetTypes = assetTypes.Select(at => new SelectListItem(
            at.AssetTypeName, 
            at.AssetTypeID.ToString()));

        Employees = employees.Select(e => new SelectListItem(
            e.EmployeeName, 
            e.EmployeeID.ToString()));

        Companies = companies.Select(c => new SelectListItem(
            c.CompanyName, 
            c.CompanyID.ToString()));
    }
}
