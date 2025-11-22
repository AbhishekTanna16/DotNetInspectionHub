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

    public async Task<IActionResult> GetAll()
    {
        try
        {
            var list = await _service.GetAllAsync();
            _logger.LogInformation("retrieving Asset Types");
            return Json(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Asset Types");
            throw;
        }
    }
    public async Task<IActionResult> Index()
    {
        try
        {
            var list = await _service.GetAllAsync();
            _logger.LogInformation("retrieving Asset Types");
            return View(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Asset Types");
            throw;
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
            _logger.LogInformation(" creating Asset Type",model.AssetTypeID);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Asset Type");
            throw;
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
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Asset Type with ID {AssetTypeID}", model.AssetTypeID);
            throw;
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _service.DeleteAsync(id);
            _logger.LogInformation(" deleting Asset Type with ID {AssetTypeID}", id);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Asset Type with ID {AssetTypeID}", id);
            throw;
        }
    }
}