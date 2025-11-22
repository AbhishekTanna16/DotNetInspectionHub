using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopInspector.Application.Interfaces;
using ShopInspector.Core.Entities;
using ShopInspector.Web.Areas.Admin.Models;

namespace ShopInspector.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class InspectionController : Controller
{
    private readonly IAssetService _assetService;
    private readonly IAssetCheckListService _assetCheckListService;
    private readonly IAssetInspectionService _inspectionService;
    private readonly IFileService _fileService;
    private readonly IEmployeeService _employeeService;
    private readonly IInspectionFrequencyService _frequencyService;
    private readonly IInspectionPhotoService _photoService;
    private readonly ILogger<InspectionController> _logger;

    public InspectionController(
        IAssetService assetService,
        IAssetCheckListService assetCheckListService,
        IAssetInspectionService inspectionService,
        IFileService fileService,
        IEmployeeService employeeService,
        IInspectionFrequencyService frequencyService,
        IInspectionPhotoService photoService,
        ILogger<InspectionController> logger)
    {
        _assetService = assetService;
        _assetCheckListService = assetCheckListService;
        _inspectionService = inspectionService;
        _fileService = fileService;
        _employeeService = employeeService;
        _frequencyService = frequencyService;
        _photoService = photoService;
        _logger = logger;
    }

    public async Task<IActionResult> Asset()
    {
        try
        {
            var assets = await _assetService.GetAllAsync();
            _logger.LogError("retrieving Assets");
            return View("Asset", assets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Assets");
            throw;
        }
    }

    public async Task<IActionResult> Start(int assetId)
    {
        try
        {
            var asset = await _assetService.GetByIdAsync(assetId);
            if (asset == null) return NotFound();

            var mappings = await _assetCheckListService.GetByAssetIdAsync(assetId);
            var items = mappings.Select(m => new InspectionItemViewModel
            {
                AssetCheckListID = m.AssetCheckListID,
                InspectionCheckListID = m.InspectionCheckListID,
                InspectionCheckListName = m.InspectionCheckList?.InspectionCheckListName ?? string.Empty,
                InspectionCheckListDescription = m.InspectionCheckList?.InspectionCheckListDescription
            }).ToList();

            var employees = (await _employeeService.GetAllAsync())
                .Select(e => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(e.EmployeeName, e.EmployeeID.ToString()));
            var frequencies = (await _frequencyService.GetAllAsync())
                .Select(f => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(f.FrequencyName, f.InspectionFrequencyID.ToString()));

            var vm = new InspectionStartViewModel
            {
                AssetID = asset.AssetID,
                AssetName = asset.AssetName,
                Items = items,
                Employees = employees,
                Frequencies = frequencies
            };
            _logger.LogError("starting inspection for AssetID {AssetID}", assetId);
            return View("Start", vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting inspection for AssetID {AssetID}", assetId);
            throw;
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> Submit([FromForm] InspectionSubmitModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var vm = await BuildInspectionStartViewModelAsync(model.AssetID);
                _logger.LogError("submitting inspection for AssetID Model is not valid  {AssetID}", model.AssetID);
                return View("Start", vm);
            }

            var inspection = new AssetInspection
            {
                AssetID = model.AssetID,
                InspectorName = string.IsNullOrWhiteSpace(model.InspectorName) ? "Anonymous" : model.InspectorName,
                InspectionDate = DateTime.UtcNow,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = User?.Identity?.Name ?? "admin",
                EmployeeID = model.EmployeeID ?? 0,
                ThirdParty = model.ThirdParty ?? false,
                InspectionFrequencyID = model.InspectionFrequencyID ?? 0,
                Attachment = ""
            };
            await _inspectionService.AddAsync(inspection);

            var displayOrder = 0;
            if (model.Files != null)
            {
                foreach (var file in model.Files)
                {
                    if (!_fileService.IsAllowedImage(file)) continue;
                    var savedRelative = await _fileService.SaveImageAsync(file, inspection.AssetInspectionID);
                    if (string.IsNullOrEmpty(savedRelative)) continue;

                    var photo = new InspectionPhoto
                    {
                        AssetInspectionID = inspection.AssetInspectionID,
                        PhotoPath = savedRelative,
                        UploadedOn = DateTime.UtcNow,
                        DisplayOrder = displayOrder
                    };
                    await _photoService.AddAsync(photo);

                    if (displayOrder == 0)
                    {
                        inspection.Attachment = savedRelative;
                        await _inspectionService.UpdateAsync(inspection);
                    }
                    displayOrder++;
                }
            }

            if (model.Items != null)
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
            _logger.LogInformation(" submitting inspection for AssetID {AssetID}", model.AssetID);
            return RedirectToAction(nameof(Thanks), new { assetId = inspection.AssetID, id = inspection.AssetInspectionID });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting inspection for AssetID {AssetID}", model.AssetID);
            throw;
        }
    }

    public IActionResult Thanks(int assetId, int id)
    {
        try
        {
            ViewData["AssetID"] = assetId;
            ViewData["InspectionID"] = id;
            _logger.LogInformation("displaying Thanks page for AssetID {AssetID}, InspectionID {InspectionID}", assetId, id);
            return View("Thanks");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error displaying Thanks page for AssetID {AssetID}, InspectionID {InspectionID}", assetId, id);
            throw;
        }
    }

    public async Task<IActionResult> History(int assetId)
    {
        try
        {
            var list = await _inspectionService.GetByAssetIdAsync(assetId);
            _logger.LogInformation("retrieving inspection history for AssetID {AssetID}", assetId);
            return View("History", list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving inspection history for AssetID {AssetID}", assetId);
            throw;
        }
    }

    public async Task<IActionResult> Details(int inspectionId)
    {
        try
        {
            var insp = await _inspectionService.GetByIdAsync(inspectionId);
            _logger.LogInformation(" retrieving inspection details for InspectionID but notfound {InspectionID}", inspectionId);
            if (insp == null) return NotFound();
            _logger.LogInformation(" retrieving inspection details for InspectionID {InspectionID}", inspectionId);
            return View("Details", insp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving inspection details for InspectionID {InspectionID}", inspectionId);
            throw;
        }
    }

    private async Task<InspectionStartViewModel> BuildInspectionStartViewModelAsync(int assetId)
    {
        try
        {
            var asset = await _assetService.GetByIdAsync(assetId);
            var mappings = await _assetCheckListService.GetByAssetIdAsync(assetId);
            var items = mappings.Select(m => new InspectionItemViewModel
            {
                AssetCheckListID = m.AssetCheckListID,
                InspectionCheckListID = m.InspectionCheckListID,
                InspectionCheckListName = m.InspectionCheckList?.InspectionCheckListName ?? string.Empty,
                InspectionCheckListDescription = m.InspectionCheckList?.InspectionCheckListDescription
            }).ToList();

            var employees = (await _employeeService.GetAllAsync())
                .Select(e => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(e.EmployeeName, e.EmployeeID.ToString()));
            var frequencies = (await _frequencyService.GetAllAsync())
                .Select(f => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(f.FrequencyName, f.InspectionFrequencyID.ToString()));
            _logger.LogInformation("building InspectionStartViewModel for AssetID {AssetID}", assetId);
            return new InspectionStartViewModel
            {
                AssetID = asset?.AssetID ?? assetId,
                AssetName = asset?.AssetName ?? "Unknown Asset",
                Items = items,
                Employees = employees,
                Frequencies = frequencies
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building InspectionStartViewModel for AssetID {AssetID}", assetId);
            throw;
        }
    }
    public async Task<IActionResult> Export(int inspectionId)
    {
        try
        {
            var insp = await _inspectionService.GetByIdAsync(inspectionId);
            if (insp == null) return NotFound();

            var webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var photoFiles = (insp.Photos ?? new List<InspectionPhoto>())
                .OrderBy(p => p.DisplayOrder)
                .Select(p =>
                {
                    var rel = p.PhotoPath.TrimStart('/', '\\');
                    var physical = Path.Combine(webRoot, rel.Replace('/', Path.DirectorySeparatorChar));
                    return (Label: Path.GetFileName(p.PhotoPath), PhysicalPath: physical);
                })
                .ToList();

            var pdfBytes = InspectionPdfGenerator.Generate(insp);
            var fileName = $"Inspection_{inspectionId}.pdf";
            _logger.LogInformation(" exporting inspection PDF for InspectionID {InspectionID}", inspectionId);
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting inspection PDF for InspectionID {InspectionID}", inspectionId);
            throw;
        }
    }
}