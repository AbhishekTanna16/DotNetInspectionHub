using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Core.Entities;
public class Employee
{
    public int EmployeeID { get; set; }
    public string EmployeeName { get; set; }
    public int CompanyID { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; }

    // Navigation
    public Company Company { get; set; }
    public List<AssetInspection> AssetInspections { get; set; }
}
