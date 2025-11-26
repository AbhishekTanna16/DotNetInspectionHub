using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Helpers;
using ShopInspector.Application.Interfaces;
using ShopInspector.Core.Entities;
using ShopInspector.Web.Areas.Admin.Models;

namespace ShopInspector.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class AssetController : Controller
{
    private readonly IAssetService _assetService;
    private readonly IQRCodeService _qrCodeService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AssetController> _logger;

    public AssetController(
        IAssetService assetService,
        ILogger<AssetController> logger,
        IConfiguration configuration,
        IQRCodeService qrCodeService)
    {
        _assetService = assetService;
        _qrCodeService = qrCodeService;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult CheckAuth()
    {
        return Ok(new { authenticated = User.Identity?.IsAuthenticated == true });
    }

    public async Task<IActionResult> Index(int pageIndex = 1, int pageSize = 5, string? searchTerm = null)
    {
        try
        {
            PaginatedList<Asset> assets;
            
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                assets = await _assetService.SerchAssetAsync(searchTerm.Trim(), pageIndex, pageSize);
                ViewData["SearchTerm"] = searchTerm.Trim();
            }
            else
            {
                assets = await _assetService.GetAllAsync(pageIndex: pageIndex, pageSize: pageSize);
                ViewData["SearchTerm"] = "";
            }
            
            // Pass pagination info to view
            ViewData["PageSize"] = pageSize;
            ViewData["CurrentPage"] = pageIndex;
            
            _logger.LogInformation("Successfully retrieved Assets for page {PageIndex} with {PageSize} items per page. Search term: '{SearchTerm}'", 
                pageIndex, pageSize, searchTerm ?? "none");
                
            return View(assets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Assets for page {PageIndex}. Search term: '{SearchTerm}'", pageIndex, searchTerm ?? "none");
            throw;
        }
    }

    // AJAX endpoint for loading table with pagination and search
    public async Task<IActionResult> LoadTable(int pageIndex = 1, int pageSize = 5, string? searchTerm = null)
    {
        try
        {
            PaginatedList<Asset> assets;
            
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                assets = await _assetService.SerchAssetAsync(searchTerm.Trim(), pageIndex, pageSize);
            }
            else
            {
                assets = await _assetService.GetAllAsync(pageIndex: pageIndex, pageSize: pageSize);
            }
            
            _logger.LogInformation("LoadTable: Retrieved {Count} Assets for page {PageIndex} with {PageSize} items per page. Search term: '{SearchTerm}'", 
                assets.Count, pageIndex, pageSize, searchTerm ?? "none");
            
            return PartialView("_TablePartial", assets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LoadTable Error retrieving Assets for page {PageIndex}. Search term: '{SearchTerm}'", pageIndex, searchTerm ?? "none");
            return PartialView("_TablePartial", new PaginatedList<Asset>(new List<Asset>(), 0, pageIndex, pageSize));
        }
    }

    public async Task<IActionResult> Create()
    {
        try
        {
            var vm = new AssetEditViewModel();
            
            // Use the clean repository method to get all dropdown data
            var dropdownData = await _assetService.GetAssetFormDropdownDataAsync();
            vm.PopulateDropdownsForCreate(dropdownData.AssetTypes, dropdownData.Employees, dropdownData.Companies);

            _logger.LogInformation("Loading Create Asset view");
            return View(vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading Create Asset view");
            throw;
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AssetEditViewModel vm)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                // Repopulate dropdowns using clean method
                var dropdownData = await _assetService.GetAssetFormDropdownDataAsync();
                vm.PopulateDropdownsForCreate(dropdownData.AssetTypes, dropdownData.Employees, dropdownData.Companies);

                _logger.LogWarning("Creating Asset but model is not valid");
                return View(vm);
            }

            var entity = new Asset
            {
                AssetName = vm.AssetName,
                AssetCode = vm.AssetCode,
                AssetLocation = vm.AssetLocation,
                AssetTypeID = vm.AssetTypeID,
                Department = vm.Department,
                Active = vm.Active,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = User?.Identity?.Name ?? "system"
            };

            await _assetService.AddAsync(entity);
            _logger.LogInformation("Creating Asset Successfully for {AssetName}", vm.AssetName);
            
            // Add success message for user feedback
            TempData["SuccessMessage"] = $"Asset '{vm.AssetName}' created successfully!";
            
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Asset {AssetName}", vm.AssetName);
            
            // Add error message and reload form
            TempData["ErrorMessage"] = "An error occurred while creating the asset. Please try again.";
            
            // Repopulate dropdowns
            var dropdownData = await _assetService.GetAssetFormDropdownDataAsync();
            vm.PopulateDropdownsForCreate(dropdownData.AssetTypes, dropdownData.Employees, dropdownData.Companies);
                
            return View(vm);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var asset = await _assetService.GetByIdAsync(id);
            if (asset == null) 
            {
                _logger.LogWarning("Asset ID not found {AssetID}", id);
                return NotFound();
            }

            var vm = new AssetEditViewModel
            {
                AssetID = asset.AssetID,
                AssetName = asset.AssetName,
                AssetLocation = asset.AssetLocation,
                AssetTypeID = asset.AssetTypeID,
                AssetCode = asset.AssetCode,
                Department = asset.Department,
                Active = asset.Active
            };

            // Use the clean repository method to get all dropdown data
            var dropdownData = await _assetService.GetAssetFormDropdownDataAsync();
            vm.PopulateDropdowns(dropdownData.AssetTypes, dropdownData.Employees, dropdownData.Companies);

            _logger.LogInformation("Loading Edit view for Asset ID {AssetID}", id);
            return View(vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading Edit view for Asset ID {AssetID}", id);
            throw;
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AssetEditViewModel vm)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                // Repopulate dropdowns using clean method
                var dropdownData = await _assetService.GetAssetFormDropdownDataAsync();
                vm.PopulateDropdowns(dropdownData.AssetTypes, dropdownData.Employees, dropdownData.Companies);

                _logger.LogWarning("Updating Asset ID but Model is not valid {AssetID}", vm.AssetID);
                return View(vm);
            }

            var existing = await _assetService.GetByIdAsync(vm.AssetID!.Value);
            if (existing == null) return NotFound();

            existing.AssetName = vm.AssetName;
            existing.AssetLocation = vm.AssetLocation;
            existing.AssetTypeID = vm.AssetTypeID;
            existing.AssetCode = vm.AssetCode;
            existing.Department = vm.Department;
            existing.Active = vm.Active;

            await _assetService.UpdateAsync(existing);
            _logger.LogInformation("Updating Asset ID {AssetID}", vm.AssetID);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Asset ID {AssetID}", vm.AssetID);
            throw;
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _assetService.DeleteAsync(id);
            _logger.LogInformation("Deleting Asset ID {AssetID}", id);
            
            // Check if this is an AJAX request
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = "Asset deleted successfully!" });
            }
            
            TempData["SuccessMessage"] = "Asset deleted successfully!";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Asset ID {AssetID}", id);
            
            // Check if this is an AJAX request
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = "Error deleting asset. Please try again." });
            }
            
            TempData["ErrorMessage"] = "Error deleting asset. Please try again.";
            return RedirectToAction(nameof(Index));
        }
    }

    public async Task<IActionResult> GenerateQr(int id)
    {
        try
        {
            var asset = await _assetService.GetByIdAsync(id);
            if (asset == null) 
            {
                _logger.LogWarning("Asset ID Not Found for QR generation {AssetID}", id);
                return NotFound();
            }
            
            // Get base URL from configuration, fallback to current request host
            var baseUrl = _configuration["QRCode:BaseUrl"];
            if (string.IsNullOrEmpty(baseUrl))
            {
                var scheme = Request.Scheme;
                var host = Request.Host.Value;
                baseUrl = $"{scheme}://{host}";
            }
            
            var url = $"{baseUrl}/PublicInspection/Start/{id}";
            var (bytes, savedPath) = await _qrCodeService.GenerateQrAsync(url, id, saveToDisk: true);
            Console.WriteLine("QR URL = " + url);
            _logger.LogInformation("Generating QR code for Asset ID {AssetID}", id);
            return View("QRCode", savedPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating QR code for Asset ID {AssetID}", id);
            throw;
        }
    }
}
