using System.ComponentModel.DataAnnotations;

namespace ShopInspector.Web.Areas.Admin.Models;

public class EmployeeViewModel
{
    public int EmployeeID { get; set; }

    [Required(ErrorMessage = "Employee Name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Employee Name must be between 2 and 100 characters")]
    [Display(Name = "Employee Name")]
    public string EmployeeName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a company")]
    [Display(Name = "Company")]
    public int CompanyID { get; set; }

    [Display(Name = "Active")]
    public bool Active { get; set; } = true;
}

public class EmployeeCreateViewModel
{
    [Required(ErrorMessage = "Employee Name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Employee Name must be between 2 and 100 characters")]
    [Display(Name = "Employee Name")]
    public string EmployeeName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a company")]
    [Display(Name = "Company")]
    public int CompanyID { get; set; }

    [Display(Name = "Active")]
    public bool Active { get; set; } = true;
}

public class EmployeeEditViewModel
{
    public int EmployeeID { get; set; }

    [Required(ErrorMessage = "Employee Name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Employee Name must be between 2 and 100 characters")]
    [Display(Name = "Employee Name")]
    public string EmployeeName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a company")]
    [Display(Name = "Company")]
    public int CompanyID { get; set; }

    [Display(Name = "Active")]
    public bool Active { get; set; } = true;
}

public class EmployeeListViewModel
{
    public int EmployeeID { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int CompanyID { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public bool Active { get; set; }
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}
