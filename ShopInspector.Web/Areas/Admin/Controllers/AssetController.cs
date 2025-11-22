using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ShopInspector.Application.Interfaces;
using ShopInspector.Core.Entities;
using ShopInspector.Web.Areas.Admin.Models;

namespace ShopInspector.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class AssetController : Controller
{
    private readonly IAssetService _assetService;
    private readonly IAssetTypeService _assetTypeService;
    private readonly IEmployeeService _employeeService;
    private readonly ICompanyService _companyService;
    private readonly IQRCodeService _qrCodeService;
    private readonly IConfiguration _configuration;
    ILogger<AssetController> _logger;

    public AssetController(
        IAssetService assetService,
        IAssetTypeService assetTypeService,
        IEmployeeService employeeService,
        ICompanyService companyService,
        ILogger<AssetController> logger,
        IConfiguration configuration,
        IQRCodeService qrCodeService)
    {
        _assetService = assetService;
        _assetTypeService = assetTypeService;
        _employeeService = employeeService;
        _companyService = companyService;
        _qrCodeService = qrCodeService;
        _configuration = configuration;
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

    // ==============================
    // LIST
    // ==============================
    public async Task<IActionResult> Index()
    {
        try
        {
            var assets = await _assetService.GetAllAsync();
            _logger.LogInformation(" retrieving Assets");
            return View(assets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Assets");
            throw;
        }
    }


    // ==============================
    // CREATE (GET)
    // ==============================
    public async Task<IActionResult> Create()
    {
        try
        {
            var vm = new AssetEditViewModel
            {

                AssetTypes = (await _assetTypeService.GetAllAsync())
                                .Select(t => new SelectListItem(t.AssetTypeName, t.AssetTypeID.ToString()))
                                .ToList(),

                //Employees = (await _employeeService.GetAllAsync())
                //                .Select(e => new SelectListItem(e.EmployeeName, e.EmployeeID.ToString()))
                //                .ToList(),

                //Companies = (await _companyService.GetAllAsync())
                //                .Select(c => new SelectListItem(c.CompanyName, c.CompanyID.ToString()))
                //                .ToList()
            };
            _logger.LogInformation(" loading Create Asset view");
            return View(vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading Create Asset view");
            throw;
        }
    }

    // ==============================
    // CREATE (POST)
    // ==============================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AssetEditViewModel vm)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                vm.AssetTypes = (await _assetTypeService.GetAllAsync())
                    .Select(t => new SelectListItem(t.AssetTypeName, t.AssetTypeID.ToString()))
                    .ToList();

                vm.Employees = (await _employeeService.GetAllAsync())
                    .Select(e => new SelectListItem(e.EmployeeName, e.EmployeeID.ToString()))
                    .ToList();
                _logger.LogInformation(" creating Asset but model is not valid ", vm.AssetID);
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
            _logger.LogInformation(" creating Asset Successfully for {AssetName}", vm.AssetName);
            
            // Add success message for user feedback
            TempData["SuccessMessage"] = $"Asset '{vm.AssetName}' created successfully!";
            
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Asset {AssetName}", vm.AssetName);
            
            // Add error message and reload form
            TempData["ErrorMessage"] = "An error occurred while creating the asset. Please try again.";
            
            vm.AssetTypes = (await _assetTypeService.GetAllAsync())
                .Select(t => new SelectListItem(t.AssetTypeName, t.AssetTypeID.ToString()))
                .ToList();
                
            return View(vm);
        }
    }

    // ==============================
    // EDIT (GET)
    // ==============================
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var asset = await _assetService.GetByIdAsync(id);
            _logger.LogInformation(" loading Edit view but Asset ID not found  {AssetID}", id);
            if (asset == null) return NotFound();

            var vm = new AssetEditViewModel
            {
                AssetID = asset.AssetID,
                AssetName = asset.AssetName,
                AssetLocation = asset.AssetLocation,
                AssetTypeID = asset.AssetTypeID,
                AssetCode = asset.AssetCode,
                Department = asset.Department,
                Active = asset.Active,

                AssetTypes = (await _assetTypeService.GetAllAsync())
                    .Select(t => new SelectListItem(
                        t.AssetTypeName, t.AssetTypeID.ToString(), t.AssetTypeID == asset.AssetTypeID)),

                Employees = (await _employeeService.GetAllAsync())
                    .Select(e => new SelectListItem(e.EmployeeName, e.EmployeeID.ToString())),

                Companies = (await _companyService.GetAllAsync())
                    .Select(c => new SelectListItem(
                        c.CompanyName, c.CompanyID.ToString()))
            };
            _logger.LogInformation(" loading Edit view for Asset ID {AssetID}", id);
            return View(vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading Edit view for Asset ID {AssetID}", id);
            throw;
        }
    }

    // ==============================
    // EDIT (POST)
    // ==============================
    [HttpPost]
     [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AssetEditViewModel vm)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                vm.AssetTypes = (await _assetTypeService.GetAllAsync())
                    .Select(t => new SelectListItem(t.AssetTypeName, t.AssetTypeID.ToString(), t.AssetTypeID == vm.AssetTypeID));

                vm.Employees = (await _employeeService.GetAllAsync())
                    .Select(e => new SelectListItem(e.EmployeeName, e.EmployeeID.ToString()));

                vm.Companies = (await _companyService.GetAllAsync())
                    .Select(c => new SelectListItem(c.CompanyName, c.CompanyID.ToString()));
                _logger.LogError(" updating Asset ID but Model is not valid  {AssetID}", vm.AssetID);
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
            _logger.LogInformation(" updating Asset ID {AssetID}", vm.AssetID);
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
            _logger.LogInformation("deleting Asset ID {AssetID}", id);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Asset ID {AssetID}", id);
            throw;
        }
    }
    public async Task<IActionResult> GenerateQr(int id)
    {
        try
        {
            var asset = await _assetService.GetByIdAsync(id);
            _logger.LogInformation(" generating QR code but Asset ID Not Found {AssetID}", id);
            if (asset == null) return NotFound();
            
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
            _logger.LogInformation(" generating QR code for Asset ID {AssetID}", id);
            return View("QRCode", savedPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating QR code for Asset ID {AssetID}", id);
            throw;
        }
    }
}
