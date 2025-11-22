using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ShopInspector.Application.Interfaces;
using ShopInspector.Core.Entities;
using ShopInspector.Web.Areas.Admin.Models;

namespace ShopInspector.Web.Areas.Admin.Controllers;
[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class EmployeeController : Controller
{
    private readonly IEmployeeService _service;
    private readonly ICompanyService companyService;
    ILogger<EmployeeController> _logger;

    public EmployeeController(IEmployeeService service, ICompanyService _companyService, ILogger<EmployeeController> logger)
    {
        _service = service;
        companyService = _companyService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var employees = await _service.GetAllAsync();
            _logger.LogInformation(" retrieving Employees");
            return View(employees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Employees");
            throw;
        }
    }

    public async Task<IActionResult> Create()
    {
        try
        {
            var model = new Employee();

            ViewBag.Companies = new SelectList(
                await companyService.GetAllAsync(),
                "CompanyID",
                "CompanyName"
            );
            _logger.LogInformation(" loading Create Employee view");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading Create Employee view");
            throw;
        }

    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmployeeViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Companies = new SelectList(await companyService.GetAllAsync(), "CompanyID", "CompanyName");
                _logger.LogError(" creating Employee but model is not valid ");
                return View(nameof(Index));
            }

            var company = await companyService.GetByIdAsync(model.CompanyID);

            var entity = new Employee
            {

                EmployeeName = model.EmployeeName,
                CompanyID = model.CompanyID,
                Company = company,
                Active = model.Active,
                CreatedBy = User.Identity.Name ?? "System",
            };

            await _service.AddAsync(entity);
            _logger.LogInformation(" creating Employee");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Employee");
            throw;
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var employee = await _service.GetByIdAsync(id);
            _logger.LogInformation(" loading Edit view for Employee ID not found {EmployeeID}", id);
            if (employee == null) return NotFound();

            ViewBag.Companies = new SelectList(await companyService.GetAllAsync(), "CompanyID", "CompanyName", employee.CompanyID);
            _logger.LogInformation(" loading Edit view for Employee ID {EmployeeID}", id);
            return View(employee);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading Edit view for Employee ID {EmployeeID}", id);
            throw;
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EmployeeViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Companies = new SelectList(await companyService.GetAllAsync(), "CompanyID", "CompanyName", model.CompanyID);
                _logger.LogInformation(" editing Employee ID  Model is not valid  {EmployeeID}", model.EmployeeID);
                return View(model);
            }
            var company = await companyService.GetByIdAsync(model.CompanyID);

            var entity = new Employee
            {

                EmployeeID = model.EmployeeID,
                EmployeeName = model.EmployeeName,
                CompanyID = model.CompanyID,
                Company = company,
                Active = model.Active,
                CreatedBy = User.Identity.Name ?? "System",
            };

            await _service.UpdateAsync(entity);
            _logger.LogInformation(" editing Employee ID {EmployeeID}", model.EmployeeID);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error editing Employee ID {EmployeeID}", model.EmployeeID);
            throw;
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _service.DeleteAsync(id);
            _logger.LogInformation(" deleting Employee ID {EmployeeID}", id);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Employee ID {EmployeeID}", id);
            throw;
        }
    }
}
