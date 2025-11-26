using System.ComponentModel.DataAnnotations;

namespace ShopInspector.Web.Areas.Admin.Models;

public class InspectionFrequencyViewModel
{
    public int InspectionFrequencyID { get; set; }

    [Required(ErrorMessage = "Frequency Name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Frequency Name must be between 2 and 100 characters")]
    [Display(Name = "Frequency Name")]
    public string FrequencyName { get; set; } = string.Empty;
}

public class InspectionFrequencyCreateViewModel
{
    [Required(ErrorMessage = "Frequency Name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Frequency Name must be between 2 and 100 characters")]
    [Display(Name = "Frequency Name")]
    public string FrequencyName { get; set; } = string.Empty;
}

public class InspectionFrequencyEditViewModel
{
    public int InspectionFrequencyID { get; set; }

    [Required(ErrorMessage = "Frequency Name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Frequency Name must be between 2 and 100 characters")]
    [Display(Name = "Frequency Name")]
    public string FrequencyName { get; set; } = string.Empty;
}

public class InspectionFrequencyListViewModel
{
    public int InspectionFrequencyID { get; set; }
    public string FrequencyName { get; set; } = string.Empty;
    public int InspectionsCount { get; set; }
}
