using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ShopInspector.Core.Entities;
public class Company
{
    public int CompanyID { get; set; }

    [Required(ErrorMessage = "Company Name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Company Name must be between 2 and 100 characters")]
    [Display(Name = "Company Name")]
    public string CompanyName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Admin Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [StringLength(150, ErrorMessage = "Email address cannot exceed 150 characters")]
    [Display(Name = "Admin Email")]
    public string CompanyAdminEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Contact Person Name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Contact Person Name must be between 2 and 100 characters")]
    [Display(Name = "Contact Person Name")]
    public string CompanyContactName { get; set; } = string.Empty;

    [Display(Name = "Active")]
    public bool Active { get; set; } = true;

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    
    [StringLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    // Navigation
    public List<Employee> Employees { get; set; } = new();
}
