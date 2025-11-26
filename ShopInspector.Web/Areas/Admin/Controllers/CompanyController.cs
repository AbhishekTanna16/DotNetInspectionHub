using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopInspector.Application.Interfaces;
using ShopInspector.Core.Entities;
using ShopInspector.Web.Areas.Admin.Models;

namespace ShopInspector.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class CompanyController : Controller
{
    private readonly ICompanyService _companyService;
    private readonly IMapper _mapper;
    private readonly ILogger<CompanyController> _logger;

    public CompanyController(ICompanyService companyService, IMapper mapper, ILogger<CompanyController> logger)
    {
        _companyService = companyService;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult CheckAuth()
    {
        return Ok(new { authenticated = User.Identity?.IsAuthenticated == true });
    }

    public async Task<IActionResult> Index(int pageIndex = 1, int pageSize = 5, string searchTerm = "")
    {
        try
        {
            var companies = await _companyService.GetAllAsync(pageIndex, pageSize, searchTerm);
            
            // Manually create the PaginatedList with mapped ViewModels
            var companyViewModels = _mapper.Map<List<CompanyListViewModel>>(companies.ToList());
            var viewModel = new PaginatedList<CompanyListViewModel>(
                companyViewModels, 
                companies.TotalCount, 
                companies.PageIndex, 
                pageSize);
            
            ViewData["PageSize"] = pageSize;
            ViewData["CurrentPage"] = pageIndex;
            ViewData["SearchTerm"] = searchTerm;
            
            _logger.LogInformation("Successfully retrieved {Count} companies for page {PageIndex} with search term '{SearchTerm}'", 
                companies.Count, pageIndex, searchTerm);
            
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving companies for page {PageIndex}", pageIndex);
            TempData["ErrorMessage"] = "An error occurred while loading companies. Please try again.";
            return View(new PaginatedList<CompanyListViewModel>(new List<CompanyListViewModel>(), 0, pageIndex, pageSize));
        }
    }

    // AJAX endpoint for loading table with pagination and search
    public async Task<IActionResult> LoadTable(int pageIndex = 1, int pageSize = 5, string searchTerm = "")
    {
        try
        {
            var companies = await _companyService.GetAllAsync(pageIndex, pageSize, searchTerm);
            
            // Manually create the PaginatedList with mapped ViewModels
            var companyViewModels = _mapper.Map<List<CompanyListViewModel>>(companies.ToList());
            var viewModel = new PaginatedList<CompanyListViewModel>(
                companyViewModels, 
                companies.TotalCount, 
                companies.PageIndex, 
                pageSize);
            
            ViewData["TotalCount"] = companies.TotalCount;
            
            _logger.LogInformation("LoadTable: Retrieved {Count} companies for page {PageIndex} with {PageSize} items per page and search term '{SearchTerm}'", 
                companies.Count, pageIndex, pageSize, searchTerm);
            
            return PartialView("_TablePartial", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LoadTable Error retrieving companies for page {PageIndex}", pageIndex);
            return PartialView("_TablePartial", new PaginatedList<CompanyListViewModel>(new List<CompanyListViewModel>(), 0, pageIndex, pageSize));
        }
    }

    public IActionResult Create()
    {
        return View(new CompanyCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CompanyCreateViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Business logic validation using existing service methods
            if (await _companyService.IsCompanyNameExistsAsync(model.CompanyName))
            {
                ModelState.AddModelError(nameof(model.CompanyName), "A company with this name already exists.");
                return View(model);
            }

            if (await _companyService.IsEmailExistsAsync(model.CompanyAdminEmail))
            {
                ModelState.AddModelError(nameof(model.CompanyAdminEmail), "A company with this email address already exists.");
                return View(model);
            }

            var company = await _companyService.CreateCompanyAsync(
                model.CompanyName,
                model.CompanyAdminEmail,
                model.CompanyContactName,
                model.Active,
                User?.Identity?.Name ?? "System");

            _logger.LogInformation("Successfully created company: {CompanyName} (ID: {CompanyID})", 
                company.CompanyName, company.CompanyID);

            TempData["SuccessMessage"] = $"Company '{company.CompanyName}' was created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating company: {CompanyName}", model.CompanyName);
            TempData["ErrorMessage"] = "An error occurred while creating the company. Please try again.";
            return View(model);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var company = await _companyService.GetByIdAsync(id);
            if (company == null)
            {
                _logger.LogWarning("Company not found for edit with ID: {CompanyID}", id);
                TempData["ErrorMessage"] = "Company not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = _mapper.Map<CompanyEditViewModel>(company);
            _logger.LogInformation("Loading edit view for company: {CompanyName} (ID: {CompanyID})", 
                company.CompanyName, id);

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading edit view for company ID: {CompanyID}", id);
            TempData["ErrorMessage"] = "An error occurred while loading the company. Please try again.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CompanyEditViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Business logic validation using existing service methods
            if (await _companyService.IsCompanyNameExistsAsync(model.CompanyName, model.CompanyID))
            {
                ModelState.AddModelError(nameof(model.CompanyName), "A company with this name already exists.");
                return View(model);
            }

            if (await _companyService.IsEmailExistsAsync(model.CompanyAdminEmail, model.CompanyID))
            {
                ModelState.AddModelError(nameof(model.CompanyAdminEmail), "A company with this email address already exists.");
                return View(model);
            }

            var updatedCompany = await _companyService.UpdateCompanyAsync(
                model.CompanyID,
                model.CompanyName,
                model.CompanyAdminEmail,
                model.CompanyContactName,
                model.Active);

            _logger.LogInformation("Successfully updated company: {CompanyName} (ID: {CompanyID})", 
                updatedCompany.CompanyName, updatedCompany.CompanyID);

            TempData["SuccessMessage"] = $"Company '{updatedCompany.CompanyName}' was updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Company not found for update with ID: {CompanyID}", model.CompanyID);
            TempData["ErrorMessage"] = "Company not found.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating company with ID: {CompanyID}", model.CompanyID);
            TempData["ErrorMessage"] = "An error occurred while updating the company. Please try again.";
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var company = await _companyService.GetByIdAsync(id);
            if (company == null)
            {
                var errorMessage = "Company not found.";
                _logger.LogWarning("Company not found for delete with ID: {CompanyID}", id);
                
                // Check if this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = errorMessage });
                }
                
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction(nameof(Index));
            }

            // Check business rules using existing service method
            if (!await _companyService.CanDeleteCompanyAsync(id))
            {
                // Get detailed information about what prevents deletion
                var relatedData = await GetCompanyRelatedDataAsync(id);
                
                var warningMessage = $"Company '{company.CompanyName}' cannot be deleted because of the following dependencies:";
                _logger.LogWarning("Cannot delete company with ID {CompanyID} - has employees", id);
                
                // Check if this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { 
                        success = false, 
                        message = warningMessage,
                        requiresConfirmation = true,
                        companyId = id,
                        companyName = company.CompanyName,
                        relatedData = relatedData
                    });
                }
                
                TempData["WarningMessage"] = warningMessage;
                TempData["CompanyId"] = id;
                TempData["CompanyName"] = company.CompanyName;
                TempData["RelatedData"] = relatedData;
                return RedirectToAction(nameof(Index));
            }

            await _companyService.DeleteAsync(id);

            var successMessage = $"Company '{company.CompanyName}' was deleted successfully.";
            _logger.LogInformation("Successfully deleted company: {CompanyName} (ID: {CompanyID})", 
                company.CompanyName, id);

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
            var errorMessage = "An error occurred while deleting the company. Please try again.";
            _logger.LogError(ex, "Error deleting company with ID: {CompanyID}", id);
            
            // Check if this is an AJAX request
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = errorMessage });
            }
            
            TempData["ErrorMessage"] = errorMessage;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForceDelete(int id)
    {
        try
        {
            var company = await _companyService.GetByIdAsync(id);
            if (company == null)
            {
                var errorMessage = "Company not found.";
                _logger.LogWarning("Company not found for force delete with ID: {CompanyID}", id);
                
                // Check if this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = errorMessage });
                }
                
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction(nameof(Index));
            }

            await _companyService.ForceDeleteAsync(id);
            
            var successMessage = $"Company '{company.CompanyName}' and all associated data deleted successfully!";
            _logger.LogInformation("Successfully force deleted company: {CompanyName} (ID: {CompanyID})", 
                company.CompanyName, id);
            
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
            var errorMessage = "An error occurred while force deleting the company. Please try again.";
            _logger.LogError(ex, "Error force deleting company with ID: {CompanyID}", id);
            
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
    private async Task<object> GetCompanyRelatedDataAsync(int companyId)
    {
        try
        {
            // Get related data
            var relatedData = await _companyService.GetCompanyRelatedDataAsync(companyId);
            
            return new
            {
                employeeCount = relatedData.EmployeeCount,
                totalInspections = relatedData.TotalInspections,
                lastInspectionDate = relatedData.LastInspectionDate?.ToString("yyyy-MM-dd") ?? "N/A",
                employeeNames = relatedData.EmployeeNames.Take(5).ToList(),
                totalEmployees = relatedData.EmployeeNames.Count(),
                affectedAssets = relatedData.AffectedAssetNames.Take(5).ToList(),
                totalAffectedAssets = relatedData.AffectedAssetNames.Count()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting related data for company {CompanyId}", companyId);
            return new
            {
                employeeCount = 0,
                totalInspections = 0,
                lastInspectionDate = "Unknown",
                employeeNames = new List<string>(),
                totalEmployees = 0,
                affectedAssets = new List<string>(),
                totalAffectedAssets = 0
            };
        }
    }
}