using System.ComponentModel.DataAnnotations;

namespace ShopInspector.Web.Areas.Admin.Models;

public class InspectionSubmitModel
{
    [Required]
    public int AssetID { get; set; }

    public string? InspectorName { get; set; }

    public int? EmployeeID { get; set; }

    public int? InspectionFrequencyID { get; set; }

    public bool? ThirdParty { get; set; }

    public List<InspectionItemAnswerModel>? Items { get; set; }

    public List<IFormFile>? Files { get; set; }
}
public class InspectionItemAnswerModel
{
    public int AssetCheckListID { get; set; }
    public bool IsChecked { get; set; }
    public string? Remarks { get; set; }
}
