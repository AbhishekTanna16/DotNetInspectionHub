using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopInspector.Application.Interfaces;
using ShopInspector.Core.Entities;
using ShopInspector.Web.Areas.Admin.Models;

namespace ShopInspector.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class InspectionFrequencyController : Controller
{
    private readonly IInspectionFrequencyService _frequencyService;
    private readonly IMapper _mapper;
    private readonly ILogger<InspectionFrequencyController> _logger;

    public InspectionFrequencyController(IInspectionFrequencyService frequencyService, IMapper mapper, ILogger<InspectionFrequencyController> logger)
    {
        _frequencyService = frequencyService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int pageIndex = 1, int pageSize = 5, string searchTerm = "")
    {
        try
        {
            var frequencies = await _frequencyService.GetAllAsync(pageIndex, pageSize, searchTerm);
            
            // Manually create the PaginatedList with mapped ViewModels
            var frequencyViewModels = _mapper.Map<List<InspectionFrequencyListViewModel>>(frequencies.ToList());
            var viewModel = new PaginatedList<InspectionFrequencyListViewModel>(
                frequencyViewModels,
                frequencies.TotalCount,
                frequencies.PageIndex,
                pageSize);
            
            ViewData["PageSize"] = pageSize;
            ViewData["CurrentPage"] = pageIndex;
            ViewData["SearchTerm"] = searchTerm;
            
            _logger.LogInformation("Successfully retrieved {Count} inspection frequencies for page {PageIndex} with search term '{SearchTerm}'", 
                frequencies.Count, pageIndex, searchTerm);
            
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving inspection frequencies for page {PageIndex}", pageIndex);
            TempData["ErrorMessage"] = "An error occurred while loading inspection frequencies. Please try again.";
            return View(new PaginatedList<InspectionFrequencyListViewModel>(new List<InspectionFrequencyListViewModel>(), 0, pageIndex, pageSize));
        }
    }

    // AJAX endpoint for loading table with pagination and search
    public async Task<IActionResult> LoadTable(int pageIndex = 1, int pageSize = 5, string searchTerm = "")
    {
        try
        {
            var frequencies = await _frequencyService.GetAllAsync(pageIndex, pageSize, searchTerm);
            
            // Manually create the PaginatedList with mapped ViewModels
            var frequencyViewModels = _mapper.Map<List<InspectionFrequencyListViewModel>>(frequencies.ToList());
            var viewModel = new PaginatedList<InspectionFrequencyListViewModel>(
                frequencyViewModels,
                frequencies.TotalCount,
                frequencies.PageIndex,
                pageSize);
            
            // Add data-total-count attribute for JavaScript
            ViewData["TotalCount"] = frequencies.TotalCount;
            
            _logger.LogInformation("LoadTable: Retrieved {Count} inspection frequencies for page {PageIndex} with {PageSize} items per page and search term '{SearchTerm}'", 
                frequencies.Count, pageIndex, pageSize, searchTerm);
            
            return PartialView("_TablePartial", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LoadTable Error retrieving inspection frequencies for page {PageIndex}", pageIndex);
            return PartialView("_TablePartial", new PaginatedList<InspectionFrequencyListViewModel>(new List<InspectionFrequencyListViewModel>(), 0, pageIndex, pageSize));
        }
    }

    public IActionResult Create()
    {
        _logger.LogInformation("Loading Create Inspection Frequency view");
        return View(new InspectionFrequencyCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InspectionFrequencyCreateViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Business logic validation using enhanced service method
            if (await _frequencyService.IsFrequencyNameExistsAsync(model.FrequencyName))
            {
                ModelState.AddModelError(nameof(model.FrequencyName), "An inspection frequency with this name already exists.");
                return View(model);
            }

            var frequency = await _frequencyService.CreateFrequencyAsync(model.FrequencyName);
            
            _logger.LogInformation("Successfully created inspection frequency: {FrequencyName}", model.FrequencyName);
            TempData["SuccessMessage"] = $"Inspection Frequency '{model.FrequencyName}' created successfully!";
            
            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid input for inspection frequency creation: {Message}", ex.Message);
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating inspection frequency: {FrequencyName}", model.FrequencyName);
            TempData["ErrorMessage"] = "An error occurred while creating the inspection frequency. Please try again.";
            return View(model);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var frequency = await _frequencyService.GetByIdAsync(id);
            if (frequency == null)
            {
                _logger.LogWarning("Inspection frequency not found for edit with ID: {FrequencyID}", id);
                TempData["ErrorMessage"] = "Inspection frequency not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = _mapper.Map<InspectionFrequencyEditViewModel>(frequency);
            _logger.LogInformation("Loading edit view for inspection frequency: {FrequencyName} (ID: {FrequencyID})", 
                frequency.FrequencyName, id);
            
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading edit view for inspection frequency ID: {FrequencyID}", id);
            TempData["ErrorMessage"] = "An error occurred while loading the inspection frequency. Please try again.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(InspectionFrequencyEditViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Business logic validation using enhanced service method
            if (await _frequencyService.IsFrequencyNameExistsAsync(model.FrequencyName, model.InspectionFrequencyID))
            {
                ModelState.AddModelError(nameof(model.FrequencyName), "An inspection frequency with this name already exists.");
                return View(model);
            }

            var updatedFrequency = await _frequencyService.UpdateFrequencyAsync(
                model.InspectionFrequencyID,
                model.FrequencyName);
            
            _logger.LogInformation("Successfully updated inspection frequency: {FrequencyName} (ID: {FrequencyID})", 
                updatedFrequency.FrequencyName, updatedFrequency.InspectionFrequencyID);
            
            TempData["SuccessMessage"] = $"Inspection Frequency '{updatedFrequency.FrequencyName}' updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Inspection frequency not found for update with ID: {FrequencyID}", model.InspectionFrequencyID);
            TempData["ErrorMessage"] = "Inspection frequency not found.";
            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid input for inspection frequency update: {Message}", ex.Message);
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating inspection frequency with ID: {FrequencyID}", model.InspectionFrequencyID);
            TempData["ErrorMessage"] = "An error occurred while updating the inspection frequency. Please try again.";
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var frequency = await _frequencyService.GetByIdAsync(id);
            if (frequency == null)
            {
                var errorMessage = "Inspection frequency not found.";
                _logger.LogWarning("Inspection frequency not found for delete with ID: {FrequencyID}", id);
                
                // Check if this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = errorMessage });
                }
                
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction(nameof(Index));
            }

            // Check business rules using enhanced service method
            if (!await _frequencyService.CanDeleteFrequencyAsync(id))
            {
                // Get detailed information about what prevents deletion
                var relatedData = await GetFrequencyRelatedDataAsync(id);
                
                var warningMessage = $"Inspection Frequency '{frequency.FrequencyName}' cannot be deleted because of the following dependencies:";
                _logger.LogWarning("Cannot delete inspection frequency with ID {FrequencyID} - has associated inspections", id);
                
                // Check if this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { 
                        success = false, 
                        message = warningMessage,
                        requiresConfirmation = true,
                        frequencyId = id,
                        frequencyName = frequency.FrequencyName,
                        relatedData = relatedData
                    });
                }
                
                TempData["WarningMessage"] = warningMessage;
                TempData["FrequencyId"] = id;
                TempData["FrequencyName"] = frequency.FrequencyName;
                TempData["RelatedData"] = relatedData;
                return RedirectToAction(nameof(Index));
            }

            await _frequencyService.DeleteAsync(id);
            
            var successMessage = $"Inspection Frequency '{frequency.FrequencyName}' deleted successfully!";
            _logger.LogInformation("Successfully deleted inspection frequency: {FrequencyName} (ID: {FrequencyID})", 
                frequency.FrequencyName, id);
            
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
            var errorMessage = "An error occurred while deleting the inspection frequency. Please try again.";
            _logger.LogError(ex, "Error deleting inspection frequency with ID: {FrequencyID}", id);
            
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
            var frequency = await _frequencyService.GetByIdAsync(id);
            if (frequency == null)
            {
                var errorMessage = "Inspection frequency not found.";
                _logger.LogWarning("Inspection frequency not found for force delete with ID: {FrequencyID}", id);
                
                // Check if this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = errorMessage });
                }
                
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction(nameof(Index));
            }

            await _frequencyService.ForceDeleteAsync(id);
            
            var successMessage = $"Inspection Frequency '{frequency.FrequencyName}' and all associated data deleted successfully!";
            _logger.LogInformation("Successfully force deleted inspection frequency: {FrequencyName} (ID: {FrequencyID})", 
                frequency.FrequencyName, id);
            
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
            var errorMessage = "An error occurred while force deleting the inspection frequency. Please try again.";
            _logger.LogError(ex, "Error force deleting inspection frequency with ID: {FrequencyID}", id);
            
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
    private async Task<object> GetFrequencyRelatedDataAsync(int frequencyId)
    {
        try
        {
            // Get related inspection data
            var relatedData = await _frequencyService.GetFrequencyRelatedInspectionsAsync(frequencyId);
            
            return new
            {
                inspectionCount = relatedData.TotalInspections,
                lastInspectionDate = relatedData.LastInspectionDate?.ToString("yyyy-MM-dd") ?? "N/A",
                firstInspectionDate = relatedData.FirstInspectionDate?.ToString("yyyy-MM-dd") ?? "N/A",
                affectedAssets = relatedData.AffectedAssetNames.Take(5).ToList(),
                totalAffectedAssets = relatedData.AffectedAssetNames.Count(),
                assignedEmployees = relatedData.AssignedEmployeeNames.Take(5).ToList(),
                totalAssignedEmployees = relatedData.AssignedEmployeeNames.Count(),
                checklistItemsCount = relatedData.ChecklistItemsCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting related data for frequency {FrequencyId}", frequencyId);
            return new
            {
                inspectionCount = 0,
                lastInspectionDate = "Unknown",
                firstInspectionDate = "Unknown",
                affectedAssets = new List<string>(),
                totalAffectedAssets = 0,
                assignedEmployees = new List<string>(),
                totalAssignedEmployees = 0,
                checklistItemsCount = 0
            };
        }
    }
}