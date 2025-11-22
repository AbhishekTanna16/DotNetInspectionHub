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
    ILogger<AssetCheckListController> _logger;
    private readonly IInspectionCheckListService _inspectionCheckListService; // create this if not yet
    private readonly IAssetTypeService _assetTypeService; // optional

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
            var list = await _assetService.GetAllAsync();
            var dtoList = list.Select(a => new AssetDropdownDto
            {
                AssetID = a.AssetID,
                AssetName = a.AssetName
            }).ToList();

            _logger.LogInformation("Get  All Asset From service");
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
            var list = await _inspectionCheckListService.GetAllAsync();
            _logger.LogInformation("Get  All inspectionCheckList From service ");
            return Json(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving InspectionCheckList");
            throw;
        }
    }
    public async Task<IActionResult> Index()
    {
        try
        {
            var list = await _service.GetAllAsync();
            _logger.LogInformation("Get  All AssetCheckList From service ");
            return View(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving AssetCheckList");
            throw;
        }
    }
    public async Task<IActionResult> Create()
    {
        try
        {
            var vm = new AssetCheckListEditViewModel
            {
                Assets = (await _assetService.GetAllAsync()).Select(a => new SelectListItem(a.AssetName, a.AssetID.ToString())),
                InspectionCheckLists = (await _inspectionCheckListService.GetAllAsync()).Select(i => new SelectListItem(i.InspectionCheckListName, i.InspectionCheckListID.ToString()))
            };
            _logger.LogInformation("Call insert view for  AssetCheckList so get Assets and  InspectionCheckLists Data  From service ");
            return View("Create", vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error preparing Create view for AssetCheckList");
            throw;
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
                vm.Assets = (await _assetService.GetAllAsync()).Select(a => new SelectListItem(a.AssetName, a.AssetID.ToString()));
                vm.InspectionCheckLists = (await _inspectionCheckListService.GetAllAsync()).Select(i => new SelectListItem(i.InspectionCheckListName, i.InspectionCheckListID.ToString()));
                _logger.LogInformation("Create AssetCheckList but data not pass properly so ModelState.IsValid is not valid  {AssetCheckListID}", vm.AssetID);
                return View(vm);
            }

            var entity = new AssetCheckList
            {
                AssetID = vm.AssetID,
                InspectionCheckListID = vm.InspectionCheckListID,
                DisplayOrder = vm.DisplayOrder,
                Active = vm.Active
            };

            await _service.AddAsync(entity);
            _logger.LogInformation("Create AssetCheckList Sucessfully {AssetCheckListID}", entity.AssetID);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating AssetCheckList   {AssetCheckListID}", vm.AssetID);
            throw;
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var entity = await _service.GetByIdAsync(id);
            if (entity == null) return NotFound();

            var vm = new AssetCheckListEditViewModel
            {
                AssetCheckListID = entity.AssetCheckListID,
                AssetID = entity.AssetID,
                InspectionCheckListID = entity.InspectionCheckListID,
                DisplayOrder = entity.DisplayOrder,
                Active = entity.Active,
                Assets = (await _assetService.GetAllAsync()).Select(a => new SelectListItem(a.AssetName, a.AssetID.ToString(), a.AssetID == entity.AssetID)),
                InspectionCheckLists = (await _inspectionCheckListService.GetAllAsync()).Select(i => new SelectListItem(i.InspectionCheckListName, i.InspectionCheckListID.ToString(), i.InspectionCheckListID == entity.InspectionCheckListID))
            };
            _logger.LogInformation("Edit view for  AssetCheckList so get Assets and  InspectionCheckLists and all data with specific id Data   From service {AssetCheckListID}", id);
            return View(vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving AssetCheckList with ID {AssetCheckListID}", id);
            throw;
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

                vm.Assets = (await _assetService.GetAllAsync()).Select(a => new SelectListItem(a.AssetName, a.AssetID.ToString(), a.AssetID == vm.AssetID));
                vm.InspectionCheckLists = (await _inspectionCheckListService.GetAllAsync()).Select(i => new SelectListItem(i.InspectionCheckListName, i.InspectionCheckListID.ToString(), i.InspectionCheckListID == vm.InspectionCheckListID));
                _logger.LogInformation("Edit AssetCheckList but data not pass properly so ModelState.IsValid is not valid   {AssetCheckListID}", vm.AssetID);
                return View(vm);
            }

            var entity = await _service.GetByIdAsync(vm.AssetCheckListID!.Value);
            if (entity == null) return NotFound();

            entity.AssetID = vm.AssetID;
            entity.InspectionCheckListID = vm.InspectionCheckListID;
            entity.DisplayOrder = vm.DisplayOrder;
            entity.Active = vm.Active;

            await _service.UpdateAsync(entity);
            _logger.LogInformation("Update AssetCheckList Sucessfully{AssetCheckListID}", entity.AssetID);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating AssetCheckList with ID {AssetCheckListID}", vm.AssetCheckListID);
            throw;
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _service.DeleteAsync(id);
            _logger.LogInformation("Delete AssetCheckList sucessfully {AssetCheckListID}", id);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting AssetCheckList with ID {AssetCheckListID}", id);
            throw;
        }
    }

    // Optional: view mapping by asset
    public async Task<IActionResult> ByAsset(int assetId)
    {
        try
        {
            var list = await _service.GetByAssetIdAsync(assetId);
            _logger.LogError("get Asset data by specific AssetID {AssetID}", assetId);
            return View("Index", list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving AssetCheckList for AssetID {AssetID}", assetId);
            throw;
        }
    }
}
