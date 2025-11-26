using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopInspector.Application.Interfaces;
using ShopInspector.Core.Entities;
using ShopInspector.Web.Areas.Admin.Models;

namespace ShopInspector.Web.Areas.Admin.Controllers;
[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class AssetTypeController : Controller
{
    private readonly IAssetTypeService _service;
    ILogger<AssetTypeController> _logger;

    public AssetTypeController(IAssetTypeService service, ILogger<AssetTypeController> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<IActionResult> GetAll(int pageIndex = 1, int pageSize = 3)
    {
        try
        {
            var list = await _service.GetAllAsync(pageIndex, pageSize);
            _logger.LogInformation("retrieving Asset Types");
            return Json(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Asset Types");
            throw;
        }
    }
    
    public async Task<IActionResult> Index(int pageIndex = 1, int pageSize = 5, string searchTerm = "")
    {
        try
        {
            PaginatedList<AssetType> list;
            
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                list = await _service.SerchAssetTypeAsync(searchTerm.Trim(), pageIndex, pageSize);
                ViewData["SearchTerm"] = searchTerm.Trim();
            }
            else
            {
                list = await _service.GetAllAsync(pageIndex, pageSize);
                ViewData["SearchTerm"] = "";
            }
            
            // Pass pagination info to view
            ViewData["PageSize"] = pageSize;
            ViewData["CurrentPage"] = pageIndex;
            
            _logger.LogInformation("Successfully retrieved Asset Types for page {PageIndex} with {PageSize} items per page. Search: '{SearchTerm}'", 
                pageIndex, pageSize, searchTerm ?? "none");
            return View(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Asset Types for page {PageIndex} with search '{SearchTerm}'", pageIndex, searchTerm);
            throw;
        }
    }

    // AJAX endpoint for loading table with pagination and search
    public async Task<IActionResult> LoadTable(int pageIndex = 1, int pageSize = 5, string searchTerm = "")
    {
        try
        {
            PaginatedList<AssetType> list;
            
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                list = await _service.SerchAssetTypeAsync(searchTerm.Trim(), pageIndex, pageSize);
            }
            else
            {
                list = await _service.GetAllAsync(pageIndex, pageSize);
            }
            
            _logger.LogInformation("LoadTable: Retrieved {Count} Asset Types for page {PageIndex} with {PageSize} items per page. Search: '{SearchTerm}'", 
                list.Count, pageIndex, pageSize, searchTerm ?? "none");
            
            return PartialView("_TablePartial", list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LoadTable Error retrieving Asset Types for page {PageIndex} with search '{SearchTerm}'", pageIndex, searchTerm);
            return PartialView("_TablePartial", new PaginatedList<AssetType>(new List<AssetType>(), 0, pageIndex, pageSize));
        }
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AssetTypeViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError(" creating Asset Type but Model is not valid ", model.AssetTypeID);
                return View(model);
            }

            var Entity = new AssetType
            {
                AssetTypeName = model.AssetTypeName
            };
            await _service.AddAsync(Entity);
            _logger.LogInformation(" creating Asset Type", model.AssetTypeID);
            
            // Add success message
            TempData["SuccessMessage"] = $"Asset Type '{model.AssetTypeName}' created successfully!";
            
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Asset Type");
            
            // Add error message
            TempData["ErrorMessage"] = "An error occurred while creating the asset type. Please try again.";
            return View(model);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var item = await _service.GetByIdAsync(id);
            _logger.LogInformation(" retrieving Asset Type for edit with ID {AssetTypeID}", id);
            return View(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Asset Type for edit with ID {AssetTypeID}", id);
            throw;
        }
    }
    [HttpPost]
    public async Task<IActionResult> Edit(AssetTypeViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError(" updating Asset Type with ID {AssetTypeID} but model is  not valid ", model.AssetTypeID);
                return View(model);
            }
            var Entity = new AssetType
            {
                AssetTypeID = model.AssetTypeID,
                AssetTypeName = model.AssetTypeName
            };
            await _service.UpdateAsync(Entity);
            _logger.LogInformation(" updating Asset Type with ID {AssetTypeID}", model.AssetTypeID);
            
            // Add success message
            TempData["SuccessMessage"] = $"Asset Type '{model.AssetTypeName}' updated successfully!";
            
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Asset Type with ID {AssetTypeID}", model.AssetTypeID);
            
            // Add error message
            TempData["ErrorMessage"] = "An error occurred while updating the asset type. Please try again.";
            return View(model);
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var assetType = await _service.GetByIdAsync(id);
            if (assetType == null)
            {
                var errorMessage = "Asset Type not found.";
                
                // Check if this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = errorMessage });
                }
                
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction(nameof(Index));
            }

            await _service.DeleteAsync(id);
            _logger.LogInformation(" deleting Asset Type with ID {AssetTypeID}", id);
            
            var successMessage = $"Asset Type '{assetType.AssetTypeName}' deleted successfully!";
            
            // Check if this is an AJAX request
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = successMessage });
            }
            
            // Add success message
            TempData["SuccessMessage"] = successMessage;
            
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Asset Type with ID {AssetTypeID}", id);
            
            var errorMessage = "An error occurred while deleting the asset type. Please try again.";
            
            // Check if this is an AJAX request
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = errorMessage });
            }
            
            // Add error message
            TempData["ErrorMessage"] = errorMessage;
            return RedirectToAction(nameof(Index));
        }
    }
}