using Microsoft.AspNetCore.Mvc;
using ShopInspector.Application.Interfaces;
using ShopInspector.Web.Areas.Admin.Models;
using ShopInspector.Core.Entities;
using Microsoft.AspNetCore.Hosting;

namespace ShopInspector.Web.Controllers;

[Route("PublicInspection")]
public class PublicInspectionController : Controller
{
    private readonly IAssetService _assetService;
    private readonly IAssetCheckListService _assetCheckListService;
    private readonly IAssetInspectionService _inspectionService;
    private readonly IEmployeeService _employeeService;
    private readonly IInspectionFrequencyService _frequencyService;
    private readonly IFileService _fileService;
    private readonly IInspectionPhotoService _photoService;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<PublicInspectionController> _logger;

    public PublicInspectionController(
        IAssetService assetService,
        IAssetCheckListService assetCheckListService,
        IAssetInspectionService inspectionService,
        IEmployeeService employeeService,
        IInspectionFrequencyService frequencyService,
        IFileService fileService,
        IInspectionPhotoService photoService,
         ILogger<PublicInspectionController> logger,
    IWebHostEnvironment env)
    {
        _assetService = assetService;
        _assetCheckListService = assetCheckListService;
        _inspectionService = inspectionService;
        _employeeService = employeeService;
        _frequencyService = frequencyService;
        _fileService = fileService;
        _photoService = photoService;
        _logger = logger;
        _env = env;
    }

    // Public inspection home - asset selection with QR support
    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(int? assetId)
    {
        try
        {
            // If assetId provided (from QR code), redirect to start inspection
            if (assetId.HasValue && assetId.Value > 0)
            {
                return RedirectToAction(nameof(Start), new { assetId = assetId.Value });
            }

            // Show asset selection page
            var assets = await _assetService.GetAllAsync();
            _logger.LogInformation("Loading public inspection home page");
            return View("~/Views/Pages/Index.cshtml", new InspectionStartViewModel 
            { 
                Assets = assets.Select(a => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(
                    $"{a.AssetName} ({a.AssetCode})", a.AssetID.ToString())).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading public inspection home");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("Assets")]
    public async Task<IActionResult> Assets()
    {
        try
        {
            var assets = await _assetService.GetAllAsync();
            _logger.LogInformation(" retrieving Assets for Public Inspection");
            return View("~/Views/PublicInspection/Assets.cshtml", assets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Assets for Public Inspection");
            return StatusCode(500, "Internal server error");
        }
    }

    // Accept /PublicInspection/Start?assetId=5 and /PublicInspection/Start/5
    [HttpGet("Start")]
    [HttpGet("Start/{assetId:int}")]
    public async Task<IActionResult> Start(int assetId)
    {
        try
        {
            var asset = await _assetService.GetByIdAsync(assetId);
            if (asset == null) 
            {
                _logger.LogWarning("Asset not found for ID {AssetID}", assetId);
                return NotFound($"Asset with ID {assetId} not found");
            }

            _logger.LogInformation("Starting inspection for Asset ID {AssetID} - {AssetName}", assetId, asset.AssetName);

            // Load checklist items mapped to this asset
            var mappings = await _assetCheckListService.GetByAssetIdAsync(assetId);
            _logger.LogInformation("Found {Count} checklist mappings for Asset ID {AssetID}", mappings?.Count ?? 0, assetId);

            var items = new List<InspectionItemViewModel>();
            
            if (mappings != null && mappings.Any())
            {
                items = mappings
                    .Where(m => m.Active) // Only include active mappings
                    .OrderBy(m => m.DisplayOrder)
                    .Select(m => new InspectionItemViewModel
                    {
                        AssetCheckListID = m.AssetCheckListID,
                        InspectionCheckListID = m.InspectionCheckListID,
                        InspectionCheckListName = m.InspectionCheckList?.InspectionCheckListName ?? $"Checklist {m.InspectionCheckListID}",
                        InspectionCheckListDescription = m.InspectionCheckList?.InspectionCheckListDescription,
                        InspectionCheckListTitle = m.InspectionCheckList?.InspectionCheckListTitle ?? string.Empty,
                        Active = m.Active
                    })
                    .ToList();

                _logger.LogInformation("Processed {Count} active checklist items for Asset ID {AssetID}", items.Count, assetId);
            }
            else
            {
                _logger.LogWarning("No checklist mappings found for Asset ID {AssetID}. You may need to configure checklists for this asset in the admin panel.", assetId);
            }

            // Dropdowns
            var employees = (await _employeeService.GetAllAsync())
                .Select(e => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(e.EmployeeName, e.EmployeeID.ToString()));
            var frequencies = (await _frequencyService.GetAllAsync())
                .Select(f => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(f.FrequencyName, f.InspectionFrequencyID.ToString()));

            // Last inspection summary for this asset
            var history = await _inspectionService.GetByAssetIdAsync(assetId);
            var last = history?.OrderByDescending(i => i.InspectionDate).FirstOrDefault();
            LastInspectionSummary? lastSummary = null;
            if (last != null)
            {
                lastSummary = new LastInspectionSummary
                {
                    InspectionId = last.AssetInspectionID,
                    InspectionDate = last.InspectionDate,
                    InspectorName = last.InspectorName,
                    EmployeeName = last.Employee?.EmployeeName,
                    FrequencyName = last.InspectionFrequency?.FrequencyName,
                    ThirdParty = last.ThirdParty ?? false,
                    Attachment = string.IsNullOrWhiteSpace(last.Attachment) ? null : last.Attachment,
                    PhotoCount = last.Photos?.Count ?? 0
                };
            }

            var vm = new InspectionStartViewModel
            {
                AssetID = asset.AssetID,
                AssetName = asset.AssetName,
                AssetCode = asset.AssetCode,
                AssetLocation = asset.AssetLocation,
                Items = items,
                Employees = employees,
                Frequencies = frequencies,
                LastInspection = lastSummary
            };
            
            _logger.LogInformation("Successfully prepared inspection start view for Asset ID {AssetID} with {ItemCount} checklist items", assetId, items.Count);
            return View("~/Views/PublicInspection/Start.cshtml", vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting inspection for Asset ID {AssetID}", assetId);
            return StatusCode(500, "Internal server error");
        }
    }

    // Debug endpoint to help troubleshoot checklist data
    [HttpGet("Debug/{assetId:int}")]
    public async Task<IActionResult> Debug(int assetId)
    {
        try
        {
            var asset = await _assetService.GetByIdAsync(assetId);
            var mappings = await _assetCheckListService.GetAllAsync(); // Get all mappings
            var assetMappings = mappings.Where(m => m.AssetID == assetId).ToList();

            var debugInfo = new
            {
                AssetExists = asset != null,
                AssetInfo = asset != null ? new { asset.AssetID, asset.AssetName, asset.AssetCode, asset.Active } : null,
                TotalMappings = mappings.Count,
                AssetMappingsCount = assetMappings.Count,
                AssetMappings = assetMappings.Select(m => new
                {
                    m.AssetCheckListID,
                    m.AssetID,
                    m.InspectionCheckListID,
                    m.DisplayOrder,
                    m.Active,
                    ChecklistName = m.InspectionCheckList?.InspectionCheckListName,
                    ChecklistActive = m.InspectionCheckList?.Active
                }).ToList()
            };

            return Json(debugInfo);
        }
        catch (Exception ex)
        {
            return Json(new { Error = ex.Message });
        }
    }

    [HttpGet("History/{assetId}")]
    public async Task<IActionResult> History(int assetId)
    {
        try
        {
            var list = await _inspectionService.GetByAssetIdAsync(assetId);
            _logger.LogInformation(" History inspection for Asset ID {AssetID}", assetId);
            return View("History", list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error History inspection for Asset ID {AssetID}", assetId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("Details/{inspectionId}")]
    public async Task<IActionResult> Details(int inspectionId)
    {
        try
        {
            var insp = await _inspectionService.GetByIdAsync(inspectionId);
            _logger.LogWarning(" Details inspection but ths Asset ID not found {AssetID}", inspectionId);
            if (insp == null) return NotFound();
            _logger.LogInformation(" Details inspection for Asset ID {AssetID}", inspectionId);
            return View("Details", insp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Details inspection for Asset ID {AssetID}", inspectionId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("Submit")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> Submit([FromForm] InspectionSubmitModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Submit inspection for Asset ID but Model is not valid {AssetID}", model.AssetID);
                return RedirectToAction(nameof(Start), new { assetId = model.AssetID });
            }

            // Handle general remarks when no checklist items are configured
            var generalRemarks = Request.Form["GeneralRemarks"].ToString();
            
            var inspection = new AssetInspection
            {
                AssetID = model.AssetID,
                InspectorName = string.IsNullOrWhiteSpace(model.InspectorName) ? "Anonymous" : model.InspectorName,
                InspectionDate = DateTime.UtcNow,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = User?.Identity?.Name ?? "public",
                EmployeeID = model.EmployeeID ?? 0,
                ThirdParty = model.ThirdParty ?? false,
                InspectionFrequencyID = model.InspectionFrequencyID ?? 0,
                Attachment = "",
                // Store general remarks in a field that can accommodate it
                // You might want to add a GeneralRemarks field to AssetInspection entity
                // For now, we'll use a comment or notes field if available
            };

            await _inspectionService.AddAsync(inspection);

            var order = 0;
            if (model.Files != null)
            {
                foreach (var f in model.Files)
                {
                    if (!_fileService.IsAllowedImage(f)) continue;
                    var path = await _fileService.SaveImageAsync(f, inspection.AssetInspectionID);
                    if (string.IsNullOrEmpty(path)) continue;

                    var photo = new InspectionPhoto
                    {
                        AssetInspectionID = inspection.AssetInspectionID,
                        PhotoPath = path,
                        UploadedOn = DateTime.UtcNow,
                        DisplayOrder = order
                    };
                    await _photoService.AddAsync(photo);

                    if (order == 0)
                    {
                        inspection.Attachment = path;
                        await _inspectionService.UpdateAsync(inspection);
                    }
                    order++;
                }
            }

            // Handle checklist items if they exist
            if (model.Items != null && model.Items.Any())
            {
                foreach (var it in model.Items)
                {
                    var row = new AssetInspectionCheckList
                    {
                        AssetInspectionID = inspection.AssetInspectionID,
                        AssetCheckListID = it.AssetCheckListID,
                        IsChecked = it.IsChecked,
                        Remarks = it.Remarks
                    };
                    await _inspectionService.AddCheckListRowAsync(row);
                }
            }
            // Handle general remarks when no checklist items are configured
            else if (!string.IsNullOrWhiteSpace(generalRemarks))
            {
                // Create a special checklist entry to store general remarks
                // This assumes you have a way to store general inspection notes
                // You might need to create a special AssetCheckList entry for general notes
                // or modify the data model to include general remarks
                _logger.LogInformation("Storing general remarks for inspection {InspectionID}: {Remarks}", 
                    inspection.AssetInspectionID, generalRemarks);
                
                // For now, you could store this in the inspection entity itself if there's a suitable field
                // Or create a special checklist entry for general remarks
            }

            _logger.LogInformation("Submit inspection for Asset ID {AssetID} with inspection ID {InspectionID}", 
                model.AssetID, inspection.AssetInspectionID);
            
            return RedirectToAction(nameof(Thanks), new { assetId = inspection.AssetID, id = inspection.AssetInspectionID });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Submit inspection for Asset ID {AssetID}", model.AssetID);
            return RedirectToAction(nameof(Start), new { assetId = model.AssetID });
        }
    }

    [HttpPost("Start")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartPost([FromForm] int selectedAssetId)
    {
        try
        {
            if (selectedAssetId <= 0)
            {
                // Return to asset selection with error
                var assets = await _assetService.GetAllAsync();
                ModelState.AddModelError(string.Empty, "Please select an asset.");
                return View("~/Views/Pages/Index.cshtml", new InspectionStartViewModel 
                { 
                    Assets = assets.Select(a => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(
                        $"{a.AssetName} ({a.AssetCode})", a.AssetID.ToString())).ToList()
                });
            }
            return RedirectToAction(nameof(Start), new { assetId = selectedAssetId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing asset selection");
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet("Export/{inspectionId:int}")]
    public async Task<IActionResult> Export(int inspectionId)
    {
        try
        {
            var insp = await _inspectionService.GetByIdAsync(inspectionId);
            if (insp == null) return NotFound();
            var pdfBytes = InspectionPdfGenerator.Generate(insp);
            _logger.LogError( "Export inspection suceessfully for Inspection ID {InspectionID}", inspectionId);
            return File(pdfBytes, "application/pdf", $"Inspection_{inspectionId}.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Export inspection for Inspection ID {InspectionID}", inspectionId);
            return StatusCode(500, "Internal server error");
        }
    }
    [HttpGet("Thanks")]
    public IActionResult Thanks(int assetId, int id)
    {
        try
        {
            ViewData["AssetID"] = assetId;
            ViewData["InspectionID"] = id;
            _logger.LogInformation(" Thanks Page   inspection for Inspection ID {InspectionID}", id);
            return View("~/Views/PublicInspection/Thanks.cshtml");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Thanks Page   inspection for Inspection ID {InspectionID}",id);
            return RedirectToAction(nameof(Start), new { assetId = assetId });
        }
    }
}