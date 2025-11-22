using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInspector.Application.Interfaces;
using ShopInspector.Core.Entities;
using ShopInspector.Infrastructure.Data;
using ShopInspector.Web.Areas.Admin.Models;

namespace ShopInspector.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class CompanyController : Controller
{
    private readonly ICompanyService _service;
    ILogger<CompanyController> _logger;

    public CompanyController(ICompanyService service, ILogger<CompanyController> logger)
    {
        _service = service;
        _logger = logger;
    }

    // ==============================
    // AUTH CHECK ENDPOINT
    // ==============================
    [HttpGet]
    public IActionResult CheckAuth()
    {
        return Ok(new { authenticated = User.Identity?.IsAuthenticated == true });
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var companies = await _service.GetAllAsync();
            _logger.LogInformation("Retrieving Companies");
            return View(companies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Companies");
            throw;
        }
    }

    public IActionResult Create()
    {
        try
        {
            var model = new Company();
            _logger.LogInformation("Loading Create Company view");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading Create Company view");
            throw;
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Company model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Company creation failed - model validation errors");
                TempData["ErrorMessage"] = "Please correct the validation errors and try again.";
                return View(model);
            }

            // Check for duplicate company name
            var existingCompanies = await _service.GetAllAsync();
            if (existingCompanies.Any(c => c.CompanyName.Equals(model.CompanyName, StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError("CompanyName", "A company with this name already exists.");
                TempData["ErrorMessage"] = "A company with this name already exists.";
                return View(model);
            }

            // Check for duplicate email
            if (existingCompanies.Any(c => c.CompanyAdminEmail.Equals(model.CompanyAdminEmail, StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError("CompanyAdminEmail", "A company with this email address already exists.");
                TempData["ErrorMessage"] = "A company with this email address already exists.";
                return View(model);
            }

            var entity = new Company
            {
                CompanyName = model.CompanyName.Trim(),
                CompanyAdminEmail = model.CompanyAdminEmail.Trim().ToLower(),
                CompanyContactName = model.CompanyContactName.Trim(),
                Active = model.Active,
                CreatedBy = User?.Identity?.Name ?? "System",
                CreatedOn = DateTime.UtcNow
            };

            await _service.AddAsync(entity);
            
            _logger.LogInformation("Successfully created Company: {CompanyName}", model.CompanyName);
            TempData["SuccessMessage"] = $"Company '{model.CompanyName}' was created successfully.";
            
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Company: {CompanyName}", model.CompanyName);
            TempData["ErrorMessage"] = "An error occurred while creating the company. Please try again.";
            return View(model);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var company = await _service.GetByIdAsync(id);
            if (company == null)
            {
                _logger.LogWarning("Company not found for Edit with ID: {CompanyID}", id);
                return NotFound();
            }

            _logger.LogInformation("Loading Edit Company view for ID: {CompanyID}", id);
            return View(company);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading Edit Company view for ID: {CompanyID}", id);
            throw;
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Company model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Company edit failed - model validation errors for ID: {CompanyID}", model.CompanyID);
                TempData["ErrorMessage"] = "Please correct the validation errors and try again.";
                return View(model);
            }

            var existingEntity = await _service.GetByIdAsync(model.CompanyID);
            if (existingEntity == null)
            {
                _logger.LogWarning("Company not found for Edit with ID: {CompanyID}", model.CompanyID);
                return NotFound();
            }

            // Check for duplicate company name (excluding current company)
            var existingCompanies = await _service.GetAllAsync();
            if (existingCompanies.Any(c => c.CompanyID != model.CompanyID && 
                c.CompanyName.Equals(model.CompanyName, StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError("CompanyName", "A company with this name already exists.");
                TempData["ErrorMessage"] = "A company with this name already exists.";
                return View(model);
            }

            // Check for duplicate email (excluding current company)
            if (existingCompanies.Any(c => c.CompanyID != model.CompanyID && 
                c.CompanyAdminEmail.Equals(model.CompanyAdminEmail, StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError("CompanyAdminEmail", "A company with this email address already exists.");
                TempData["ErrorMessage"] = "A company with this email address already exists.";
                return View(model);
            }

            // Update the entity
            existingEntity.CompanyName = model.CompanyName.Trim();
            existingEntity.CompanyAdminEmail = model.CompanyAdminEmail.Trim().ToLower();
            existingEntity.CompanyContactName = model.CompanyContactName.Trim();
            existingEntity.Active = model.Active;

            await _service.UpdateAsync(existingEntity);
            
            _logger.LogInformation("Successfully updated Company: {CompanyName} (ID: {CompanyID})", 
                model.CompanyName, model.CompanyID);
            TempData["SuccessMessage"] = $"Company '{model.CompanyName}' was updated successfully.";
            
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error editing Company with ID: {CompanyID}", model.CompanyID);
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
            var company = await _service.GetByIdAsync(id);
            if (company == null)
            {
                _logger.LogWarning("Company not found for Delete with ID: {CompanyID}", id);
                return NotFound();
            }

            // Check if company has employees
            if (company.Employees != null && company.Employees.Any())
            {
                _logger.LogWarning("Cannot delete Company with ID {CompanyID} - has {EmployeeCount} employees", 
                    id, company.Employees.Count);
                TempData["ErrorMessage"] = $"Cannot delete company '{company.CompanyName}' because it has {company.Employees.Count} employee(s). Please reassign or remove employees first.";
                return RedirectToAction(nameof(Index));
            }

            await _service.DeleteAsync(id);
            
            _logger.LogInformation("Successfully deleted Company: {CompanyName} (ID: {CompanyID})", 
                company.CompanyName, id);
            TempData["SuccessMessage"] = $"Company '{company.CompanyName}' was deleted successfully.";
            
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Company with ID: {CompanyID}", id);
            TempData["ErrorMessage"] = "An error occurred while deleting the company. Please try again.";
            return RedirectToAction(nameof(Index));
        }
    }
}