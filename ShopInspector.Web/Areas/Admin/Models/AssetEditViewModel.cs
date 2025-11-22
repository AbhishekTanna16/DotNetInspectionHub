using System.ComponentModel.DataAnnotations;

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
    public IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>? Companies { get; set; }
    public IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>? AssetTypes { get; set; }
    public IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>? Employees { get; set; }
}
