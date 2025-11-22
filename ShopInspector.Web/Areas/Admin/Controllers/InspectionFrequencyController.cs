using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopInspector.Application.Interfaces;
using ShopInspector.Core.Entities;
using ShopInspector.Web.Areas.Admin.Models;

namespace ShopInspector.Web.Areas.Admin.Controllers;
[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class InspectionFrequencyController : Controller
{
    private readonly IInspectionFrequencyService _service;
    ILogger<InspectionFrequencyController> logger;
    public InspectionFrequencyController(IInspectionFrequencyService service, ILogger<InspectionFrequencyController> logger)
    {
        _service = service;
        this.logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var items = await _service.GetAllAsync();
            logger.LogError("retrieving Inspection Frequencies");
            return View(items);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving Inspection Frequencies");
            throw;
        }
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InspectionFrequencyViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                logger.LogError("creating Inspection Frequency Model is not valid ");
                return View(model);
            }

            var entity = new InspectionFrequency
            {
                FrequencyName = model.FrequencyName
            };
            await _service.AddAsync(entity);
            logger.LogInformation("creating Inspection Frequency");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating Inspection Frequency");
            throw;
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var item = await _service.GetByIdAsync(id);
            logger.LogInformation("not found  Inspection Frequency with ID  {InspectionFrequencyID}", id);
            if (item == null) return NotFound();
            logger.LogInformation("for update retrieving  Inspection Frequency with ID  {InspectionFrequencyID}", id);
            return View(item);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error retrieving Inspection Frequency with ID {InspectionFrequencyID}", id);
            throw;
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(InspectionFrequencyViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
                return View(model);
            var entity = new InspectionFrequency
            {
                InspectionFrequencyID = model.InspectionFrequencyID,
                FrequencyName = model.FrequencyName
            };
            await _service.UpdateAsync(entity);
            logger.LogInformation("updating Inspection Frequency with ID {InspectionFrequencyID}", model.InspectionFrequencyID);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating Inspection Frequency with ID {InspectionFrequencyID}", model.InspectionFrequencyID);
            throw;
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _service.DeleteAsync(id);
            logger.LogInformation("Delete InspectionFrequency this id  InspectionFrequencyID{InspectionFrequencyID}",id);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting Inspection Frequency with ID {InspectionFrequencyID}", id);
            throw;
        }
    }
}