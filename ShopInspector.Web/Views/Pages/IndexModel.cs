using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopInspector.Application.Interfaces;
using ShopInspector.Core.Entities;

namespace ShopInspector.Web.Views.Pages;

public class IndexModel : PageModel
{
    private readonly IAssetService _assetService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IAssetService assetService, ILogger<IndexModel> logger)
    {
        _assetService = assetService;
        _logger = logger;
    }

    public List<Asset> Assets { get; private set; } = new();

    [BindProperty]
    public int? SelectedAssetId { get; set; }

    public IActionResult OnGet(int? assetId)
    {
        // Check if user is authenticated - if so, redirect to admin dashboard
        if (User?.Identity?.IsAuthenticated == true)
        {
            if (assetId.HasValue && assetId.Value > 0)
            {
                // If coming from QR code with assetId, redirect to public inspection
                return Redirect($"/PublicInspection/Start/{assetId.Value}");
            }
            // Authenticated user without assetId goes to admin dashboard
            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }

        // For non-authenticated users coming from QR codes with assetId
        if (assetId.HasValue && assetId.Value > 0)
        {
            return Redirect($"/PublicInspection/Start/{assetId.Value}");
        }

        // Default case - redirect to login since root should show login first
        return Redirect("/Account/Login");
    }

    public async Task<IActionResult> OnPostStartAsync()
    {
        // This should not be reached in normal flow since page redirects on GET
        // But keeping for backwards compatibility
        if (!SelectedAssetId.HasValue || SelectedAssetId.Value <= 0)
        {
            ModelState.AddModelError(string.Empty, "Please select an asset.");
            Assets = await _assetService.GetAllAsync();
            return Page();
        }
        return Redirect($"/PublicInspection/Start/{SelectedAssetId.Value}");
    }
}