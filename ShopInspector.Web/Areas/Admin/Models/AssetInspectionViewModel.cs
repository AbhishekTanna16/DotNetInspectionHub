namespace ShopInspector.Web.Areas.Admin.Models;

public class AssetInspectionViewModel
{
    public int AssetID { get; set; }
    public string InspectorName { get; set; }
    public DateTime InspectionDate { get; set; }
    public string Attachment { get; set; }
    public int InspectionFrequencyID { get; set; }
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; }
    public int EmployeeID { get; set; }
    public bool? ThirdParty { get; set; }

}
