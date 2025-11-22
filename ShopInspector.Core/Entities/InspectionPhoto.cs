namespace ShopInspector.Core.Entities;

public class InspectionPhoto
{
    public int InspectionPhotoID { get; set; }
    public int AssetInspectionID { get; set; }
    public string PhotoPath { get; set; }
    public DateTime UploadedOn { get; set; }
    public int DisplayOrder { get; set; }

    // Navigation
    public AssetInspection AssetInspection { get; set; }

}
