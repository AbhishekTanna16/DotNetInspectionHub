using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopInspector.Application.Interfaces;
using ShopInspector.Core.Entities;
using ShopInspector.Web.Areas.Admin.Models;

namespace ShopInspector.Web.Areas.Admin.Controllers;
[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class InspectionCheckListController : Controller
{
    private readonly IInspectionCheckListService _service;
    ILogger<InspectionCheckListController> _logger;
    public InspectionCheckListController(IInspectionCheckListService service, ILogger<InspectionCheckListController> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int pageIndex = 1, int pageSize = 5, string? searchTerm = null)
    {
        try
        {
            var items = await _service.GetAllAsync(pageIndex, pageSize, searchTerm);
            
            // Pass pagination info to view
            ViewData["PageSize"] = pageSize;
            ViewData["CurrentPage"] = pageIndex;
            ViewData["SearchTerm"] = searchTerm;
            
            _logger.LogInformation("Successfully retrieved Inspection Check Lists for page {PageIndex} with {PageSize} items per page. Search term: {SearchTerm}", pageIndex, pageSize, searchTerm ?? "None");
            return View(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Inspection Check Lists for page {PageIndex}. Search term: {SearchTerm}", pageIndex, searchTerm);
            throw;
        }
    }

    // AJAX endpoint for loading table with pagination and search
    public async Task<IActionResult> LoadTable(int pageIndex = 1, int pageSize = 5, string? searchTerm = null)
    {
        try
        {
            var items = await _service.GetAllAsync(pageIndex, pageSize, searchTerm);
            
            // Pass search term to the partial view
            ViewBag.SearchTerm = searchTerm;
            
            _logger.LogInformation("LoadTable: Retrieved {Count} Inspection Check Lists for page {PageIndex} with {PageSize} items per page. Search term: {SearchTerm}", 
                items.Count, pageIndex, pageSize, searchTerm ?? "None");
            
            return PartialView("_TablePartial", items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LoadTable Error retrieving Inspection Check Lists for page {PageIndex}. Search term: {SearchTerm}", pageIndex, searchTerm);
            return PartialView("_TablePartial", new PaginatedList<InspectionCheckList>(new List<InspectionCheckList>(), 0, pageIndex, pageSize));
        }
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InspectionItemViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("Error creating Inspection Check List Model is Not Valid ", model.InspectionCheckListID);
                return View(model);
            }

            var entity = new InspectionCheckList
            {
                InspectionCheckListName = model.InspectionCheckListName,
                InspectionCheckListDescription = model.InspectionCheckListDescription,
                InspectionCheckListTitle = model.InspectionCheckListTitle,
                Active = model.Active
            };

            await _service.AddAsync(entity);
            _logger.LogInformation("creating Inspection Check List", model.InspectionCheckListID);
            
            // Add success message
            TempData["SuccessMessage"] = $"Inspection Check List '{model.InspectionCheckListName}' created successfully!";
            
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Inspection Check List", model.InspectionCheckListID);
            
            // Add error message
            TempData["ErrorMessage"] = "An error occurred while creating the inspection check list. Please try again.";
            return View(model);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null)
            {
                _logger.LogInformation(" retrieving Inspection Check List for editing not found",id);
                TempData["ErrorMessage"] = "Inspection Check List not found.";
                return RedirectToAction(nameof(Index));
            }
            _logger.LogInformation(" retrieving Inspection Check List for editing",id);
            return View(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Inspection Check List for editing",id);
            throw;
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(InspectionItemViewModel model)
    {
        try
        {
            if (!ModelState.IsValid) return View(model);
            var entity = new InspectionCheckList
            {
                InspectionCheckListID = model.InspectionCheckListID,
                InspectionCheckListName = model.InspectionCheckListName,
                InspectionCheckListDescription = model.InspectionCheckListDescription,
                InspectionCheckListTitle = model.InspectionCheckListTitle,
                Active = model.Active
            };
            await _service.UpdateAsync(entity);
            _logger.LogInformation("updating Inspection Check List", model.InspectionCheckListID);
            
            // Add success message
            TempData["SuccessMessage"] = $"Inspection Check List '{model.InspectionCheckListName}' updated successfully!";
            
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Inspection Check List", model.InspectionCheckListID);
            
            // Add error message
            TempData["ErrorMessage"] = "An error occurred while updating the inspection check list. Please try again.";
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null)
            {
                var errorMessage = "Inspection Check List not found.";
                _logger.LogWarning("Inspection Check List not found for delete with ID: {CheckListID}", id);
                
                // Check if this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = errorMessage });
                }
                
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction(nameof(Index));
            }

            // Check business rules - if this checklist is being used by asset checklists
            if (!await _service.CanDeleteCheckListAsync(id))
            {
                // Get detailed information about what prevents deletion
                var relatedData = await GetCheckListRelatedDataAsync(id);
                
                var warningMessage = $"Inspection Checklist '{item.InspectionCheckListName}' cannot be deleted because of the following dependencies:";
                _logger.LogWarning("Cannot delete inspection checklist with ID {CheckListID} - has associated asset checklists", id);
                
                // Check if this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { 
                        success = false, 
                        message = warningMessage,
                        requiresConfirmation = true,
                        checkListId = id,
                        checkListName = item.InspectionCheckListName,
                        relatedData = relatedData
                    });
                }
                
                TempData["WarningMessage"] = warningMessage;
                TempData["CheckListId"] = id;
                TempData["CheckListName"] = item.InspectionCheckListName;
                TempData["RelatedData"] = relatedData;
                return RedirectToAction(nameof(Index));
            }

            await _service.DeleteAsync(id);
            
            var successMessage = $"Inspection Check List '{item.InspectionCheckListName}' deleted successfully!";
            _logger.LogInformation("Successfully deleted inspection checklist: {CheckListName} (ID: {CheckListID})", 
                item.InspectionCheckListName, id);
            
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
            var errorMessage = "An error occurred while deleting the inspection check list. Please try again.";
            _logger.LogError(ex, "Error deleting inspection check list with ID: {CheckListID}", id);
            
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
            var item = await _service.GetByIdAsync(id);
            if (item == null)
            {
                var errorMessage = "Inspection Check List not found.";
                _logger.LogWarning("Inspection Check List not found for force delete with ID: {CheckListID}", id);
                
                // Check if this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = errorMessage });
                }
                
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction(nameof(Index));
            }

            await _service.ForceDeleteAsync(id);
            
            var successMessage = $"Inspection Check List '{item.InspectionCheckListName}' and all associated data deleted successfully!";
            _logger.LogInformation("Successfully force deleted inspection checklist: {CheckListName} (ID: {CheckListID})", 
                item.InspectionCheckListName, id);
            
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
            var errorMessage = "An error occurred while force deleting the inspection check list. Please try again.";
            _logger.LogError(ex, "Error force deleting inspection check list with ID: {CheckListID}", id);
            
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
    private async Task<object> GetCheckListRelatedDataAsync(int checkListId)
    {
        try
        {
            // Get related data
            var relatedData = await _service.GetCheckListRelatedDataAsync(checkListId);
            
            return new
            {
                assetCheckListCount = relatedData.TotalAssetCheckLists,
                inspectionItemsCount = relatedData.TotalInspectionItems,
                firstUsedDate = relatedData.FirstUsedDate?.ToString("yyyy-MM-dd") ?? "N/A",
                lastUsedDate = relatedData.LastUsedDate?.ToString("yyyy-MM-dd") ?? "N/A",
                affectedAssets = relatedData.AffectedAssetNames.Take(5).ToList(),
                totalAffectedAssets = relatedData.AffectedAssetNames.Count(),
                assignedCompanies = relatedData.AssignedCompanyNames.Take(5).ToList(),
                totalAssignedCompanies = relatedData.AssignedCompanyNames.Count()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting related data for checklist {CheckListId}", checkListId);
            return new
            {
                assetCheckListCount = 0,
                inspectionItemsCount = 0,
                firstUsedDate = "Unknown",
                lastUsedDate = "Unknown",
                affectedAssets = new List<string>(),
                totalAffectedAssets = 0,
                assignedCompanies = new List<string>(),
                totalAssignedCompanies = 0
            };
        }
    }
}
