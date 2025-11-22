using Microsoft.AspNetCore.Mvc.Rendering;

namespace ShopInspector.Web.Areas.Admin.Models;

public class InspectionStartViewModel
{
    public int AssetID { get; set; }
    public string AssetName { get; set; } = string.Empty;

    public string? AssetCode { get; set; }
    public string? AssetLocation { get; set; }
    public List<InspectionItemViewModel> Items { get; set; } = new();

    public IEnumerable<SelectListItem>? Employees { get; set; }
    public IEnumerable<SelectListItem>? Frequencies { get; set; }
    public IEnumerable<SelectListItem>? Assets { get; set; } // For asset selection page
    public LastInspectionSummary? LastInspection { get; set; }
}

public class LastInspectionSummary
{
    public int InspectionId { get; set; }
    public DateTime InspectionDate { get; set; }
    public string? InspectorName { get; set; }
    public string? EmployeeName { get; set; }
    public string? FrequencyName { get; set; }
    public bool ThirdParty { get; set; }
    public string? Attachment { get; set; }
    public int PhotoCount { get; set; }
}