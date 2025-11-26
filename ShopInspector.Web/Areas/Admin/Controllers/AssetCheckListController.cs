using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ShopInspector.Application.DTOs;
using ShopInspector.Application.Interfaces;
using ShopInspector.Core.Entities;
using ShopInspector.Infrastructure.Services;
using ShopInspector.Web.Areas.Admin.Models;

namespace ShopInspector.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class AssetCheckListController : Controller
{
    private readonly IAssetCheckListService _service;
    private readonly IAssetService _assetService;
    private readonly IInspectionCheckListService _inspectionCheckListService;
    private readonly ILogger<AssetCheckListController> _logger;
    private readonly IAssetTypeService _assetTypeService;

    public AssetCheckListController(
        IAssetCheckListService service,
        IAssetService assetService,
        IInspectionCheckListService inspectionCheckListService,
        ILogger<AssetCheckListController> logger,
        IAssetTypeService assetTypeService = null!)
    {
        _service = service;
        _assetService = assetService;
        _inspectionCheckListService = inspectionCheckListService;
        _logger = logger;
        _assetTypeService = assetTypeService;
    }

    public async Task<IActionResult> getAllAsset()
    {
        try
        {
            var list = await _assetService.GetAllAsync(null, null);
            var dtoList = list.Select(a => new AssetDropdownDto
            {
                AssetID = a.AssetID,
                AssetName = a.AssetName
            }).ToList();

            _logger.LogInformation("Get All Asset From service");
            return Json(dtoList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Asset");
            throw;
        }
    }

    public async Task<IActionResult> getAllChecklist()
    {
        try
        {
            var list = await _inspectionCheckListService.GetAllAsync(null, null);
            _logger.LogInformation("Get All inspectionCheckList From service");
            return Json(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving InspectionCheckList");
            throw;
        }
    }

    public async Task<IActionResult> Index(int pageIndex = 1, int pageSize = 3, string searchTerm = "")
    {
        try
        {
            // Use small page size to ensure pagination shows up
            var list = await _service.GetAllAsync(pageIndex, pageSize, searchTerm);
            
            // Add cache-busting headers to ensure fresh data
            Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
            Response.Headers.Append("Pragma", "no-cache");
            Response.Headers.Append("Expires", "0");
            
            // Pass pagination info to view
            ViewData["PageSize"] = pageSize;
            ViewData["CurrentPage"] = pageIndex;
            ViewData["SearchTerm"] = searchTerm;
            
            // Log detailed information for debugging
            _logger.LogInformation("Successfully retrieved {Count} AssetCheckLists for page {PageIndex} with {PageSize} items per page and search term '{SearchTerm}'. Total count: {TotalCount}", 
                list.Count, pageIndex, pageSize, searchTerm, list.TotalCount);
            
            return View(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving AssetCheckList for page {PageIndex}", pageIndex);
            TempData["ErrorMessage"] = "An error occurred while loading asset checklists. Please try again.";
            return View(new PaginatedList<AssetCheckList>(new List<AssetCheckList>(), 0, pageIndex, pageSize));
        }
    }

    // AJAX endpoint for loading table with pagination and search
    public async Task<IActionResult> LoadTable(int pageIndex = 1, int pageSize = 3, string searchTerm = "")
    {
        try
        {
            var list = await _service.GetAllAsync(pageIndex, pageSize, searchTerm);
            
            ViewData["TotalCount"] = list.TotalCount;
            
            _logger.LogInformation("LoadTable: Retrieved {Count} AssetCheckLists for page {PageIndex} with {PageSize} items per page and search term '{SearchTerm}'", 
                list.Count, pageIndex, pageSize, searchTerm);
            
            return PartialView("_TablePartial", list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LoadTable Error retrieving AssetCheckList for page {PageIndex}", pageIndex);
            return PartialView("_TablePartial", new PaginatedList<AssetCheckList>(new List<AssetCheckList>(), 0, pageIndex, pageSize));
        }
    }

    // AJAX endpoint for loading asset checklists
    public async Task<IActionResult> LoadAssetCheckLists(int pageIndex = 1, int pageSize = 10, string viewMode = "grouped", string searchTerm = "")
    {
        try
        {
            var list = await _service.GetAllAsync(pageIndex, pageSize, searchTerm);
            
            // Pass pagination info to view
            ViewData["PageSize"] = pageSize;
            ViewData["CurrentPage"] = pageIndex;
            ViewData["TotalCount"] = list.TotalCount;
            
            _logger.LogInformation("AJAX: Retrieved {Count} AssetCheckLists for page {PageIndex} with {PageSize} items per page and search term '{SearchTerm}'", 
                list.Count, pageIndex, pageSize, searchTerm);
            
            // Return appropriate partial view based on view mode
            if (viewMode == "table")
            {
                return PartialView("_AssetCheckListTablePartial", list);
            }
            else
            {
                return PartialView("_AssetCheckListCardsPartial", list);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AJAX Error retrieving AssetCheckList for page {PageIndex}", pageIndex);
            return PartialView("_ErrorPartial", "An error occurred while loading asset checklists.");
        }
    }

    public async Task<IActionResult> Create()
    {
        try
        {
            var vm = new AssetCheckListEditViewModel
            {
                Assets = (await _assetService.GetAllAsync(null, null))
                    .Where(a => a.Active)
                    .Select(a => new SelectListItem(a.AssetName, a.AssetID.ToString())),
                InspectionCheckLists = (await _inspectionCheckListService.GetAllAsync(null, null))
                    .Where(i => i.Active)
                    .Select(i => new SelectListItem(i.InspectionCheckListName, i.InspectionCheckListID.ToString()))
            };
            
            _logger.LogInformation("Loading Create view for AssetCheckList with {AssetCount} assets and {ChecklistCount} checklists", 
                vm.Assets.Count(), vm.InspectionCheckLists.Count());
            
            return View("Create", vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error preparing Create view for AssetCheckList");
            TempData["ErrorMessage"] = "An error occurred while loading the form. Please try again.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AssetCheckListEditViewModel vm)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                vm.Assets = (await _assetService.GetAllAsync(null, null))
                    .Where(a => a.Active)
                    .Select(a => new SelectListItem(a.AssetName, a.AssetID.ToString()));
                vm.InspectionCheckLists = (await _inspectionCheckListService.GetAllAsync(null, null))
                    .Where(i => i.Active)
                    .Select(i => new SelectListItem(i.InspectionCheckListName, i.InspectionCheckListID.ToString()));
                
                _logger.LogWarning("Create AssetCheckList validation failed for AssetID: {AssetID}", vm.AssetID);
                return View(vm);
            }

            // Handle multiple checklist selection
            if (vm.InspectionCheckListIDs != null && vm.InspectionCheckListIDs.Any())
            {
                var createdCount = 0;
                foreach (var checklistId in vm.InspectionCheckListIDs)
                {
                    // Check if this combination already exists
                    var existingEntity = await _service.GetByAssetAndChecklistAsync(vm.AssetID, checklistId);
                    if (existingEntity == null)
                    {
                        var entity = new AssetCheckList
                        {
                            AssetID = vm.AssetID,
                            InspectionCheckListID = checklistId,
                            DisplayOrder = vm.DisplayOrder + createdCount,
                            Active = vm.Active
                        };

                        await _service.AddAsync(entity);
                        createdCount++;
                        _logger.LogInformation("Created AssetCheckList successfully: Asset {AssetID} -> Checklist {ChecklistID}", 
                            entity.AssetID, checklistId);
                    }
                    else
                    {
                        _logger.LogWarning("AssetCheckList already exists for Asset {AssetID} and Checklist {ChecklistID}", 
                            vm.AssetID, checklistId);
                    }
                }

                if (createdCount > 0)
                {
                    TempData["SuccessMessage"] = $"Successfully created {createdCount} asset checklist(s).";
                    _logger.LogInformation("Successfully created {CreatedCount} new asset checklist assignments", createdCount);
                }
                else
                {
                    TempData["WarningMessage"] = "All selected checklist combinations already exist for this asset.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Please select at least one checklist.";
                vm.Assets = (await _assetService.GetAllAsync(null, null))
                    .Where(a => a.Active)
                    .Select(a => new SelectListItem(a.AssetName, a.AssetID.ToString()));
                vm.InspectionCheckLists = (await _inspectionCheckListService.GetAllAsync(null, null))
                    .Where(i => i.Active)
                    .Select(i => new SelectListItem(i.InspectionCheckListName, i.InspectionCheckListID.ToString()));
                return View(vm);
            }

            // Force a redirect with cache busting to ensure fresh data
            return RedirectToAction(nameof(Index), new { refresh = DateTime.Now.Ticks });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating AssetCheckList for AssetID: {AssetID}", vm.AssetID);
            TempData["ErrorMessage"] = "An error occurred while creating the asset checklist. Please try again.";
            return RedirectToAction(nameof(Index));
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var entity = await _service.GetByIdAsync(id);
            if (entity == null) 
            {
                TempData["ErrorMessage"] = "Asset Check List not found.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new AssetCheckListEditViewModel
            {
                AssetCheckListID = entity.AssetCheckListID,
                AssetID = entity.AssetID,
                InspectionCheckListID = entity.InspectionCheckListID,
                DisplayOrder = entity.DisplayOrder,
                Active = entity.Active,
                Assets = (await _assetService.GetAllAsync(null, null))
                    .Where(a => a.Active)
                    .Select(a => new SelectListItem(a.AssetName, a.AssetID.ToString(), a.AssetID == entity.AssetID)),
                InspectionCheckLists = (await _inspectionCheckListService.GetAllAsync(null, null))
                    .Where(i => i.Active)
                    .Select(i => new SelectListItem(i.InspectionCheckListName, i.InspectionCheckListID.ToString(), i.InspectionCheckListID == entity.InspectionCheckListID))
            };
            
            _logger.LogInformation("Loading Edit view for AssetCheckListID: {AssetCheckListID}", id);
            return View(vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving AssetCheckList with ID {AssetCheckListID}", id);
            TempData["ErrorMessage"] = "An error occurred while loading the asset checklist. Please try again.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AssetCheckListEditViewModel vm)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                vm.Assets = (await _assetService.GetAllAsync(null, null))
                    .Where(a => a.Active)
                    .Select(a => new SelectListItem(a.AssetName, a.AssetID.ToString(), a.AssetID == vm.AssetID));
                vm.InspectionCheckLists = (await _inspectionCheckListService.GetAllAsync(null, null))
                    .Where(i => i.Active)
                    .Select(i => new SelectListItem(i.InspectionCheckListName, i.InspectionCheckListID.ToString(), i.InspectionCheckListID == vm.InspectionCheckListID));
                
                _logger.LogWarning("Edit AssetCheckList validation failed for ID: {AssetCheckListID}", vm.AssetCheckListID);
                return View(vm);
            }

            var entity = await _service.GetByIdAsync(vm.AssetCheckListID!.Value);
            if (entity == null) 
            {
                TempData["ErrorMessage"] = "Asset Check List not found.";
                return RedirectToAction(nameof(Index));
            }

            entity.AssetID = vm.AssetID;
            entity.InspectionCheckListID = vm.InspectionCheckListID;
            entity.DisplayOrder = vm.DisplayOrder;
            entity.Active = vm.Active;

            await _service.UpdateAsync(entity);
            _logger.LogInformation("Updated AssetCheckList successfully: ID {AssetCheckListID}", entity.AssetCheckListID);
            
            TempData["SuccessMessage"] = "Asset Check List updated successfully!";
            return RedirectToAction(nameof(Index), new { refresh = DateTime.Now.Ticks });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating AssetCheckList with ID {AssetCheckListID}", vm.AssetCheckListID);
            TempData["ErrorMessage"] = "An error occurred while updating the asset checklist. Please try again.";
            return RedirectToAction(nameof(Index));
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
                var errorMessage = "Asset Check List not found.";
                _logger.LogWarning("Asset Check List not found for delete with ID: {AssetCheckListID}", id);
                
                // Check if this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = errorMessage });
                }
                
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction(nameof(Index));
            }

            // Check business rules - if this asset checklist is being used by asset inspections
            if (!await _service.CanDeleteAsync(id))
            {
                // Get detailed information about what prevents deletion
                var relatedData = await GetAssetCheckListRelatedDataAsync(id);
                
                var warningMessage = $"Asset checklist for '{item.Asset?.AssetName}' - '{item.InspectionCheckList?.InspectionCheckListName}' cannot be deleted because of the following dependencies:";
                _logger.LogWarning("Cannot delete asset checklist with ID {AssetCheckListID} - has associated inspection records", id);
                
                // Check if this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { 
                        success = false, 
                        message = warningMessage,
                        requiresConfirmation = true,
                        assetCheckListId = id,
                        assetName = item.Asset?.AssetName ?? "Unknown Asset",
                        checkListName = item.InspectionCheckList?.InspectionCheckListName ?? "Unknown Checklist",
                        relatedData = relatedData
                    });
                }
                
                TempData["WarningMessage"] = warningMessage;
                TempData["AssetCheckListId"] = id;
                TempData["AssetName"] = item.Asset?.AssetName;
                TempData["CheckListName"] = item.InspectionCheckList?.InspectionCheckListName;
                TempData["RelatedData"] = relatedData;
                return RedirectToAction(nameof(Index));
            }

            await _service.DeleteAsync(id);
            
            var successMessage = $"Asset Check List for '{item.Asset?.AssetName}' deleted successfully!";
            _logger.LogInformation("Successfully deleted AssetCheckList: Asset {AssetName} - Checklist {ChecklistName} (ID: {AssetCheckListID})", 
                item.Asset?.AssetName, item.InspectionCheckList?.InspectionCheckListName, id);
            
            // Check if this is an AJAX request
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = successMessage });
            }
            
            TempData["SuccessMessage"] = successMessage;
            return RedirectToAction(nameof(Index), new { refresh = DateTime.Now.Ticks });
        }
        catch (Exception ex)
        {
            var errorMessage = "An error occurred while deleting the asset check list. Please try again.";
            _logger.LogError(ex, "Error deleting AssetCheckList with ID {AssetCheckListID}", id);
            
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
                var errorMessage = "Asset Check List not found.";
                _logger.LogWarning("Asset Check List not found for force delete with ID: {AssetCheckListID}", id);
                
                // Check if this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = errorMessage });
                }
                
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction(nameof(Index));
            }

            await _service.ForceDeleteAsync(id);
            
            var successMessage = $"Asset Check List for '{item.Asset?.AssetName}' - '{item.InspectionCheckList?.InspectionCheckListName}' and all associated data deleted successfully!";
            _logger.LogInformation("Successfully force deleted AssetCheckList: Asset {AssetName} - Checklist {ChecklistName} (ID: {AssetCheckListID})", 
                item.Asset?.AssetName, item.InspectionCheckList?.InspectionCheckListName, id);
            
            // Check if this is an AJAX request
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = successMessage });
            }
            
            TempData["SuccessMessage"] = successMessage;
            return RedirectToAction(nameof(Index), new { refresh = DateTime.Now.Ticks });
        }
        catch (Exception ex)
        {
            var errorMessage = "An error occurred while force deleting the asset check list. Please try again.";
            _logger.LogError(ex, "Error force deleting AssetCheckList with ID {AssetCheckListID}", id);
            
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
    private async Task<object> GetAssetCheckListRelatedDataAsync(int assetCheckListId)
    {
        try
        {
            // Get related data
            var relatedData = await _service.GetAssetCheckListRelatedDataAsync(assetCheckListId);
            
            return new
            {
                inspectionRecordsCount = relatedData.TotalInspectionRecords,
                firstInspectionDate = relatedData.FirstInspectionDate?.ToString("yyyy-MM-dd") ?? "N/A",
                lastInspectionDate = relatedData.LastInspectionDate?.ToString("yyyy-MM-dd") ?? "N/A",
                employeeNames = relatedData.EmployeeNames.Take(5).ToList(),
                totalEmployees = relatedData.EmployeeNames.Count(),
                inspectionFrequencies = relatedData.InspectionFrequencies.Take(5).ToList(),
                totalFrequencies = relatedData.InspectionFrequencies.Count(),
                assetName = relatedData.AssetName,
                checkListName = relatedData.CheckListName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting related data for asset checklist {AssetCheckListId}", assetCheckListId);
            return new
            {
                inspectionRecordsCount = 0,
                firstInspectionDate = "Unknown",
                lastInspectionDate = "Unknown",
                employeeNames = new List<string>(),
                totalEmployees = 0,
                inspectionFrequencies = new List<string>(),
                totalFrequencies = 0,
                assetName = "Unknown Asset",
                checkListName = "Unknown Checklist"
            };
        }
    }

    // API endpoint to get fresh statistics
    [HttpGet]
    public async Task<IActionResult> GetStatistics()
    {
        try
        {
            var allChecklists = await _service.GetAllAsync(null, null);
            
            var statistics = new
            {
                TotalAssets = allChecklists.GroupBy(ac => ac.AssetID).Count(),
                TotalAssignments = allChecklists.Count(),
                ActiveAssignments = allChecklists.Count(ac => ac.Active),
                LastUpdated = DateTime.Now
            };
            
            _logger.LogInformation("Retrieved fresh statistics: {TotalAssets} assets, {TotalAssignments} assignments", 
                statistics.TotalAssets, statistics.TotalAssignments);
            
            return Json(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving statistics");
            return Json(new { error = "Failed to load statistics" });
        }
    }

    // Optional: view mapping by asset
    public async Task<IActionResult> ByAsset(int assetId)
    {
        try
        {
            var list = await _service.GetByAssetIdAsync(assetId);
            _logger.LogInformation("Retrieved AssetCheckList data for AssetID: {AssetID}, Count: {Count}", assetId, list.Count);
            return View("Index", list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving AssetCheckList for AssetID {AssetID}", assetId);
            throw;
        }
    }
}
