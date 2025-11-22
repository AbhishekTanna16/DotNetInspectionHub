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

    public async Task<IActionResult> Index()
    {
        try
        {
            var items = await _service.GetAllAsync();
            _logger.LogInformation("retrieving Inspection Check Lists");
            return View(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Inspection Check Lists");
            throw;
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
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Inspection Check List", model.InspectionCheckListID);
            throw;
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var item = await _service.GetByIdAsync(id);
            _logger.LogInformation(" retrieving Inspection Check List for editing not found",id);
            if (item == null) return NotFound();
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
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Inspection Check List", model.InspectionCheckListID);
            throw;
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _service.DeleteAsync(id);
            _logger.LogInformation("deleting Inspection Check List ",id);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Inspection Check List", id);
            throw;
        }
    }
}
