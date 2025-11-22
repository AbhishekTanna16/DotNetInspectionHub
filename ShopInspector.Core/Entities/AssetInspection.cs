using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Core.Entities;
public class AssetInspection
{
    public int AssetInspectionID { get; set; }
    public int AssetID { get; set; }
    public string InspectorName { get; set; }
    public DateTime InspectionDate { get; set; }
    public string Attachment { get; set; }
    public int InspectionFrequencyID { get; set; }
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; }
    public int EmployeeID { get; set; }
    public bool? ThirdParty { get; set; }

    // Navigation
    public Asset Asset { get; set; }
    public Employee Employee { get; set; }
    public InspectionFrequency InspectionFrequency { get; set; }
    public List<AssetInspectionCheckList> AssetInspectionCheckLists { get; set; }
    public List<InspectionPhoto> Photos { get; set; } = new();
}
