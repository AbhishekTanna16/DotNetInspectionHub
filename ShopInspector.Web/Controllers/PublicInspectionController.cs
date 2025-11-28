using Microsoft.AspNetCore.Mvc;
using ShopInspector.Application.Interfaces;
using ShopInspector.Web.Areas.Admin.Models;
using ShopInspector.Core.Entities;
using Microsoft.AspNetCore.Hosting;
using ShopInspector.Application.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ShopInspector.Web.Controllers;

[Route("PublicInspection")]
public class PublicInspectionController : Controller
{
    private readonly IAssetService _assetService;
    private readonly IAssetCheckListService _assetCheckListService;
    private readonly IAssetInspectionService _inspectionService;
    private readonly IFileService _fileService;
    private readonly IInspectionPhotoService _photoService;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<PublicInspectionController> _logger;

    public PublicInspectionController(
        IAssetService assetService,
        IAssetCheckListService assetCheckListService,
        IAssetInspectionService inspectionService,
        IFileService fileService,
        IInspectionPhotoService photoService,
        ILogger<PublicInspectionController> logger,
    IWebHostEnvironment env)
    {
        _assetService = assetService;
        _assetCheckListService = assetCheckListService;
        _inspectionService = inspectionService;
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

            // Show asset selection page using the clean repository method
            var assetData = await _assetCheckListService.GetAssetDropdownDataAsync();
            var assets = assetData.Select(a => new SelectListItem(a.DisplayText, a.AssetID.ToString())).ToList();
            
            _logger.LogInformation("Loading public inspection home page with {Count} assets", assets.Count);
            
            var inspectionStartViewModel = new InspectionStartViewModel 
            { 
                Assets = assets
            };
            
            return View("~/Views/Pages/Index.cshtml", inspectionStartViewModel);
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
            var assets = await _assetService.GetAllAsync(null, null);
            _logger.LogInformation("Retrieving Assets for Public Inspection");
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

            // Use the clean repository method to get inspection items
            var itemsData = await _assetCheckListService.GetInspectionItemsForAssetAsync(assetId);
            var items = itemsData.Select(i => new InspectionItemViewModel
            {
                AssetCheckListID = i.AssetCheckListID,
                InspectionCheckListID = i.InspectionCheckListID,
                InspectionCheckListName = i.InspectionCheckListName,
                InspectionCheckListDescription = i.InspectionCheckListDescription,
                InspectionCheckListTitle = i.InspectionCheckListTitle,
                Active = i.Active
            }).ToList();

            _logger.LogInformation("Found {Count} checklist items for Asset ID {AssetID}", items.Count, assetId);

            // Get dropdown data using clean repository methods
            var employeeData = await _assetCheckListService.GetEmployeeDropdownDataAsync();
            var employees = employeeData.Select(e => new SelectListItem(e.EmployeeName, e.EmployeeID.ToString()));

            var frequencyData = await _assetCheckListService.GetInspectionFrequencyDropdownDataAsync();
            var frequencies = frequencyData.Select(f => new SelectListItem(f.FrequencyName, f.InspectionFrequencyID.ToString()));

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
                    InspectorName = last.InspectorName ?? "Unknown",
                  
                    EmployeeName = last.Employee?.EmployeeName ?? "N/A",
                    FrequencyName = last.InspectionFrequency?.FrequencyName ?? "N/A",
                    ThirdParty = last.ThirdParty ?? false,
                    Attachment = string.IsNullOrWhiteSpace(last.Attachment) ? null : last.Attachment,
                    PhotoCount = last.Photos?.Count ?? 0
                };
            }

            var viewModel = new InspectionStartViewModel
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
            
            // Add validation error display if redirected from failed submission
            if (TempData["ValidationErrors"] != null)
            {
                ViewBag.ValidationErrors = TempData["ValidationErrors"].ToString();
            }
            
            _logger.LogInformation("Successfully prepared inspection start view for Asset ID {AssetID} with {ItemCount} checklist items", assetId, items.Count);
            return View("~/Views/PublicInspection/Start.cshtml", viewModel);
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
            var mappings = await _assetCheckListService.GetAllAsync(null,null); // Get all mappings
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
    public async Task<IActionResult> History(int assetId, int pageIndex = 1, int pageSize = 5, string searchTerm = "")
    {
        try
        {
            // Get all inspections for the asset
            var allInspections = await _inspectionService.GetByAssetIdAsync(assetId);
            
            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                allInspections = allInspections.Where(i => 
                    !string.IsNullOrWhiteSpace(i.InspectorName) && 
                    i.InspectorName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            
            // Order by inspection date descending (newest first)
            var orderedInspections = allInspections.OrderByDescending(i => i.InspectionDate).ToList();
            
            // Create paginated list
            var totalCount = orderedInspections.Count;
            var skip = (pageIndex - 1) * pageSize;
            var pagedInspections = orderedInspections.Skip(skip).Take(pageSize).ToList();
            
            var paginatedList = new PaginatedList<AssetInspection>(pagedInspections, totalCount, pageIndex, pageSize);
            
            // Pass pagination and search info to view
            ViewData["PageSize"] = pageSize;
            ViewData["CurrentPage"] = pageIndex;
            ViewData["AssetId"] = assetId;
            ViewData["SearchTerm"] = searchTerm;
            
            _logger.LogInformation("History inspection for Asset ID {AssetID} - Page {PageIndex}, Size {PageSize}, Total {TotalCount}, Search: '{SearchTerm}'", 
                assetId, pageIndex, pageSize, totalCount, searchTerm);
            
            return View("History", paginatedList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error History inspection for Asset ID {AssetID}", assetId);
            return StatusCode(500, "Internal server error");
        }
    }

    // AJAX endpoint for loading history table with pagination and search
    [HttpGet("LoadHistoryTable/{assetId}")]
    public async Task<IActionResult> LoadHistoryTable(int assetId, int pageIndex = 1, int pageSize = 5, string searchTerm = "")
    {
        try
        {
            // Get all inspections for the asset
            var allInspections = await _inspectionService.GetByAssetIdAsync(assetId);
            
            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                allInspections = allInspections.Where(i => 
                    !string.IsNullOrWhiteSpace(i.InspectorName) && 
                    i.InspectorName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            
            // Order by inspection date descending (newest first)
            var orderedInspections = allInspections.OrderByDescending(i => i.InspectionDate).ToList();
            
            // Create paginated list
            var totalCount = orderedInspections.Count;
            var skip = (pageIndex - 1) * pageSize;
            var pagedInspections = orderedInspections.Skip(skip).Take(pageSize).ToList();
            
            var paginatedList = new PaginatedList<AssetInspection>(pagedInspections, totalCount, pageIndex, pageSize);
            
            // Pass pagination and search info to view
            ViewData["AssetId"] = assetId;
            ViewData["SearchTerm"] = searchTerm;
            
            _logger.LogInformation("LoadHistoryTable: Retrieved {Count} inspections for Asset ID {AssetID}, page {PageIndex}, search: '{SearchTerm}'", 
                paginatedList.Count, assetId, pageIndex, searchTerm);
            
            return PartialView("_HistoryTablePartial", paginatedList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LoadHistoryTable Error for Asset ID {AssetID}", assetId);
            return PartialView("_HistoryTablePartial", new PaginatedList<AssetInspection>(new List<AssetInspection>(), 0, pageIndex, pageSize));
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
            // Custom validation for required fields
            if (string.IsNullOrWhiteSpace(model.InspectorName))
            {
                ModelState.AddModelError(nameof(model.InspectorName), "Inspector Name is required");
            }

            if (!model.EmployeeID.HasValue || model.EmployeeID <= 0)
            {
                ModelState.AddModelError(nameof(model.EmployeeID), "Employee selection is required");
            }

            if (!model.InspectionFrequencyID.HasValue || model.InspectionFrequencyID <= 0)
            {
                ModelState.AddModelError(nameof(model.InspectionFrequencyID), "Frequency selection is required");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Submit inspection for Asset ID {AssetID} - Model validation failed: {Errors}", 
                    model.AssetID, string.Join(", ", ModelState.SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage))));
                
                // Redirect back to start with validation errors
                TempData["ValidationErrors"] = string.Join("; ", ModelState.SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage)));
                return RedirectToAction(nameof(Start), new { assetId = model.AssetID });
            }

            // Handle general remarks when no checklist items are configured
            var generalRemarks = Request.Form["GeneralRemarks"].ToString();
            
            var inspection = new AssetInspection
            {
                AssetID = model.AssetID,
                InspectorName = model.InspectorName,
                InspectionDate = DateTime.UtcNow,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = User?.Identity?.Name ?? "public",
                EmployeeID = model.EmployeeID.Value, // Now guaranteed to have a value
                ThirdParty = model.ThirdParty ?? false,
                InspectionFrequencyID = model.InspectionFrequencyID.Value, // Now guaranteed to have a value
                Attachment = "",
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
                // Return to asset selection with error using the clean repository method
                var assetData = await _assetCheckListService.GetAssetDropdownDataAsync();
                var assets = assetData.Select(a => new SelectListItem(a.DisplayText, a.AssetID.ToString())).ToList();
                
                ModelState.AddModelError(string.Empty, "Please select an asset.");
                
                var inspectionStartViewModel = new InspectionStartViewModel 
                { 
                    Assets = assets
                };
                
                return View("~/Views/Pages/Index.cshtml", inspectionStartViewModel);
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
            if (insp == null) 
            {
                _logger.LogWarning("Inspection not found for ID {InspectionID}", inspectionId);
                return NotFound($"Inspection with ID {inspectionId} not found");
            }

            var photos = insp.Photos ?? new List<InspectionPhoto>();
            var photoFiles = new List<(string Label, string PhysicalPath)>();
            
            foreach (var photo in photos)
            {
                if (string.IsNullOrWhiteSpace(photo.PhotoPath))
                {
                    _logger.LogWarning("Photo path is null or empty for photo in inspection {InspectionID}", inspectionId);
                    continue;
                }

                // Use correct path for both development and production (Render)
                var uploadsRoot = _env.IsDevelopment()
                    ? Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, "uploads")
                    : Path.Combine("/var/data", "uploads");
                    
                var physicalPath = Path.Combine(uploadsRoot, photo.PhotoPath.TrimStart('/').Replace("uploads/", ""));
                
                if (!System.IO.File.Exists(physicalPath))
                {
                    _logger.LogWarning("Photo file not found at path: {PhysicalPath} for inspection {InspectionID}", 
                        physicalPath, inspectionId);
                    continue;
                }

                var label = $"Photo {photo.DisplayOrder + 1}";
                photoFiles.Add((label, physicalPath));
            }

            if (insp == null)
            {
                _logger.LogError("Inspection object is null for ID {InspectionID}", inspectionId);
                return StatusCode(500, "Unable to generate PDF: Inspection data is invalid");
            }

            // Use the correct PDF generator from Application layer with enhanced error handling
            byte[] pdfBytes;
            try
            {
                pdfBytes = InspectionPdfGenerator.Generate(insp, photoFiles);
            }
            catch (Exception pdfEx)
            {
                _logger.LogError(pdfEx, "Error generating PDF for inspection {InspectionID}", inspectionId);
                return StatusCode(500, "Error generating PDF report");
            }
            
            _logger.LogInformation("Export inspection successfully for Inspection ID {InspectionID} with {PhotoCount} valid photos", 
                inspectionId, photoFiles.Count);
            
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