using AutoMapper;
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
    private readonly IEmployeeService _employeeService;
    private readonly ICompanyService _companyService;
    private readonly IMapper _mapper;
    private readonly ILogger<EmployeeController> _logger;

    public EmployeeController(IEmployeeService employeeService, ICompanyService companyService, IMapper mapper, ILogger<EmployeeController> logger)
    {
        _employeeService = employeeService;
        _companyService = companyService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int pageIndex = 1, int pageSize = 5, string searchTerm = "")
    {
        try
        {
            var employees = await _employeeService.GetAllAsync(pageIndex, pageSize, searchTerm);
            
            // Manually create the PaginatedList with mapped ViewModels
            var employeeViewModels = _mapper.Map<List<EmployeeListViewModel>>(employees.ToList());
            var viewModel = new PaginatedList<EmployeeListViewModel>(
                employeeViewModels,
                employees.TotalCount,
                employees.PageIndex,
                pageSize);
            
            ViewData["PageSize"] = pageSize;
            ViewData["CurrentPage"] = pageIndex;
            ViewData["SearchTerm"] = searchTerm;
            
            _logger.LogInformation("Successfully retrieved {Count} employees for page {PageIndex} with search term '{SearchTerm}'", 
                employees.Count, pageIndex, searchTerm);
            
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employees for page {PageIndex}", pageIndex);
            TempData["ErrorMessage"] = "An error occurred while loading employees. Please try again.";
            return View(new PaginatedList<EmployeeListViewModel>(new List<EmployeeListViewModel>(), 0, pageIndex, pageSize));
        }
    }

    // AJAX endpoint for loading table with pagination and search
    public async Task<IActionResult> LoadTable(int pageIndex = 1, int pageSize = 5, string searchTerm = "")
    {
        try
        {
            var employees = await _employeeService.GetAllAsync(pageIndex, pageSize, searchTerm);
            
            // Manually create the PaginatedList with mapped ViewModels
            var employeeViewModels = _mapper.Map<List<EmployeeListViewModel>>(employees.ToList());
            var viewModel = new PaginatedList<EmployeeListViewModel>(
                employeeViewModels,
                employees.TotalCount,
                employees.PageIndex,
                pageSize);
            
            ViewData["PageSize"] = pageSize;
            ViewData["CurrentPage"] = pageIndex;
            ViewData["TotalCount"] = employees.TotalCount;
            
            _logger.LogInformation("LoadTable: Retrieved {Count} employees for page {PageIndex} with {PageSize} items per page and search term '{SearchTerm}'", 
                employees.Count, pageIndex, pageSize, searchTerm);
            
            return PartialView("_TablePartial", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LoadTable Error retrieving employees for page {PageIndex}", pageIndex);
            return PartialView("_TablePartial", new PaginatedList<EmployeeListViewModel>(new List<EmployeeListViewModel>(), 0, pageIndex, pageSize));
        }
    }

    public async Task<IActionResult> Create()
    {
        try
        {
            var model = new EmployeeCreateViewModel();
            await LoadCompanyDropdownAsync();
            
            _logger.LogInformation("Loading Create Employee view");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading Create Employee view");
            TempData["ErrorMessage"] = "An error occurred while loading the form. Please try again.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmployeeCreateViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                await LoadCompanyDropdownAsync();
                return View(model);
            }

            // Business logic validation using enhanced service method
            if (await _employeeService.IsEmployeeNameExistsInCompanyAsync(model.EmployeeName, model.CompanyID))
            {
                ModelState.AddModelError(nameof(model.EmployeeName), "An employee with this name already exists in the selected company.");
                await LoadCompanyDropdownAsync();
                return View(model);
            }

            var employee = await _employeeService.CreateEmployeeAsync(
                model.EmployeeName,
                model.CompanyID,
                model.Active,
                User?.Identity?.Name ?? "System");
            
            _logger.LogInformation("Successfully created employee: {EmployeeName}", model.EmployeeName);
            TempData["SuccessMessage"] = $"Employee '{model.EmployeeName}' created successfully!";
            
            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid input for employee creation: {Message}", ex.Message);
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadCompanyDropdownAsync();
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employee: {EmployeeName}", model.EmployeeName);
            TempData["ErrorMessage"] = "An error occurred while creating the employee. Please try again.";
            await LoadCompanyDropdownAsync();
            return View(model);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var employee = await _employeeService.GetByIdAsync(id);
            if (employee == null)
            {
                _logger.LogWarning("Employee not found for edit with ID: {EmployeeID}", id);
                TempData["ErrorMessage"] = "Employee not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = _mapper.Map<EmployeeEditViewModel>(employee);
            await LoadCompanyDropdownAsync(employee.CompanyID);
            
            _logger.LogInformation("Loading edit view for employee: {EmployeeName} (ID: {EmployeeID})", 
                employee.EmployeeName, id);
            
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading edit view for employee ID: {EmployeeID}", id);
            TempData["ErrorMessage"] = "An error occurred while loading the employee. Please try again.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EmployeeEditViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                await LoadCompanyDropdownAsync(model.CompanyID);
                return View(model);
            }

            // Business logic validation using enhanced service method
            if (await _employeeService.IsEmployeeNameExistsInCompanyAsync(model.EmployeeName, model.CompanyID, model.EmployeeID))
            {
                ModelState.AddModelError(nameof(model.EmployeeName), "An employee with this name already exists in the selected company.");
                await LoadCompanyDropdownAsync(model.CompanyID);
                return View(model);
            }

            var updatedEmployee = await _employeeService.UpdateEmployeeAsync(
                model.EmployeeID,
                model.EmployeeName,
                model.CompanyID,
                model.Active);
            
            _logger.LogInformation("Successfully updated employee: {EmployeeName} (ID: {EmployeeID})", 
                updatedEmployee.EmployeeName, updatedEmployee.EmployeeID);
            
            TempData["SuccessMessage"] = $"Employee '{updatedEmployee.EmployeeName}' updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Employee not found for update with ID: {EmployeeID}", model.EmployeeID);
            TempData["ErrorMessage"] = "Employee not found.";
            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid input for employee update: {Message}", ex.Message);
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadCompanyDropdownAsync(model.CompanyID);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating employee with ID: {EmployeeID}", model.EmployeeID);
            TempData["ErrorMessage"] = "An error occurred while updating the employee. Please try again.";
            await LoadCompanyDropdownAsync(model.CompanyID);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var employee = await _employeeService.GetByIdAsync(id);
            if (employee == null)
            {
                var errorMessage = "Employee not found.";
                _logger.LogWarning("Employee not found for delete with ID: {EmployeeID}", id);
                
                // Check if this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = errorMessage });
                }
                
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction(nameof(Index));
            }

            // Check business rules using enhanced service method
            if (!await _employeeService.CanDeleteEmployeeAsync(id))
            {
                // Get detailed information about what prevents deletion
                var relatedData = await GetEmployeeRelatedDataAsync(id);
                
                var warningMessage = $"Employee '{employee.EmployeeName}' cannot be deleted because of the following dependencies:";
                
                _logger.LogWarning("Cannot delete employee with ID {EmployeeID} - has business constraints", id);
                
                // Check if this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { 
                        success = false, 
                        message = warningMessage,
                        requiresConfirmation = true,
                        employeeId = id,
                        employeeName = employee.EmployeeName,
                        relatedData = relatedData
                    });
                }
                
                TempData["WarningMessage"] = warningMessage;
                TempData["EmployeeId"] = id;
                TempData["EmployeeName"] = employee.EmployeeName;
                TempData["RelatedData"] = relatedData;
                return RedirectToAction(nameof(Index));
            }

            await _employeeService.DeleteAsync(id);
            
            var successMessage = $"Employee '{employee.EmployeeName}' deleted successfully!";
            _logger.LogInformation("Successfully deleted employee: {EmployeeName} (ID: {EmployeeID})", 
                employee.EmployeeName, id);
            
            // Check if this is an AJAX request
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = successMessage });
            }
            
            TempData["SuccessMessage"] = successMessage;
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            var errorMessage = "An error occurred while deleting the employee. Please try again.";
            _logger.LogError(ex, "Error deleting employee with ID: {EmployeeID}", id);
            
            // Check if this is an AJAX request
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = errorMessage });
            }
            
            TempData["ErrorMessage"] = errorMessage;
            return RedirectToAction(nameof(Index));
        }
    }

    // Add new method to get detailed information about related data
    private async Task<object> GetEmployeeRelatedDataAsync(int employeeId)
    {
        try
        {
            // Get related inspection data
            var relatedInspections = await _employeeService.GetEmployeeRelatedInspectionsAsync(employeeId);
            
            return new
            {
                inspectionCount = relatedInspections.TotalInspections,
                lastInspectionDate = relatedInspections.LastInspectionDate?.ToString("yyyy-MM-dd") ?? "N/A",
                affectedAssets = relatedInspections.AffectedAssetNames.Take(5).ToList(),
                totalAffectedAssets = relatedInspections.AffectedAssetNames.Count(),
                checklistItemsCount = relatedInspections.ChecklistItemsCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting related data for employee {EmployeeId}", employeeId);
            return new
            {
                inspectionCount = 0,
                lastInspectionDate = "Unknown",
                affectedAssets = new List<string>(),
                totalAffectedAssets = 0,
                checklistItemsCount = 0
            };
        }
    }

    public async Task<IActionResult> ForceDelete(int id)
    {
        try
        {
            var employee = await _employeeService.GetByIdAsync(id);
            if (employee == null)
            {
                var errorMessage = "Employee not found.";
                _logger.LogWarning("Employee not found for force delete with ID: {EmployeeID}", id);
                
                // Check if this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = errorMessage });
                }
                
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction(nameof(Index));
            }

            await _employeeService.ForceDeleteAsync(id);
            
            var successMessage = $"Employee '{employee.EmployeeName}' and all associated data deleted successfully!";
            _logger.LogInformation("Successfully force deleted employee: {EmployeeName} (ID: {EmployeeID})", 
                employee.EmployeeName, id);
            
            // Check if this is an AJAX request
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = successMessage });
            }
            
            TempData["SuccessMessage"] = successMessage;
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            var errorMessage = "An error occurred while force deleting the employee. Please try again.";
            _logger.LogError(ex, "Error force deleting employee with ID: {EmployeeID}", id);
            
            // Check if this is an AJAX request
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = errorMessage });
            }
            
            TempData["ErrorMessage"] = errorMessage;
            return RedirectToAction(nameof(Index));
        }
    }

    private async Task LoadCompanyDropdownAsync(int? selectedCompanyId = null)
    {
        try
        {
            var companies = await _companyService.GetAllAsync(null, null);
            var activeCompanies = companies.Where(c => c.Active).ToList();
            ViewBag.Companies = new SelectList(activeCompanies, "CompanyID", "CompanyName", selectedCompanyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading company dropdown data");
            ViewBag.Companies = new SelectList(new List<Company>(), "CompanyID", "CompanyName");
        }
    }
}
