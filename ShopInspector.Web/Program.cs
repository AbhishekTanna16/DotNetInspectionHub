using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using ShopInspector.Application.Interfaces;
using ShopInspector.Application.Services;
using ShopInspector.Infrastructure.Data;
using ShopInspector.Infrastructure.Repositories;
using ShopInspector.Infrastructure.Services;
using QuestPDF.Infrastructure;
using QuestPDF;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options =>
{
    // Use PORT from environment (Render provides this)
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    options.ListenAnyIP(int.Parse(port));
});

// Database
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// MVC / Razor
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(options =>
{
    // Razor Pages root is under /Views/Pages
    options.RootDirectory = "/Views/Pages";
});
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .MinimumLevel.Override("Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware", LogEventLevel.Error)
    .CreateLogger();

builder.Host.UseSerilog(); // 

// Services / Repos
builder.Services.AddScoped<IAssetRepository, AssetRepository>();
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IAssetTypeRepository, AssetTypeRepository>();
builder.Services.AddScoped<IAssetTypeService, AssetTypeService>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IQRCodeRepository, QRCodeRepository>();
builder.Services.AddScoped<IQRCodeService, QRCodeService>();
builder.Services.AddScoped<IAssetCheckListRepository, AssetCheckListRepository>();
builder.Services.AddScoped<IAssetCheckListService, AssetCheckListService>();
builder.Services.AddScoped<IInspectionCheckListRepository, InspectionCheckListRepository>();
builder.Services.AddScoped<IInspectionCheckListService, InspectionCheckListService>();
builder.Services.AddScoped<IAssetInspectionRepository, AssetInspectionRepository>();
builder.Services.AddScoped<IAssetInspectionService, AssetInspectionService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IInspectionPhotoRepository, InspectionPhotoRepository>();
builder.Services.AddScoped<IInspectionPhotoService, InspectionPhotoService>();
builder.Services.AddScoped<IInspectionFrequencyRepository, InspectionFrequencyRepository>();
builder.Services.AddScoped<IInspectionFrequencyService, InspectionFrequencyService>();

// AuthN / AuthZ
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
});

// Form upload limit
builder.Services.Configure<FormOptions>(opt =>
{
    opt.MultipartBodyLengthLimit = 50 * 1024 * 1024;
});

// QuestPDF license
QuestPDF.Settings.License = LicenseType.Community;

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.Use(async (context, next) =>
{
    if (!context.Request.Headers.ContainsKey("ngrok-skip-browser-warning"))
    {
        context.Request.Headers.Append("ngrok-skip-browser-warning", "true");
    }

    await next();
});
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// Redirect root to admin login
app.MapGet("/", (HttpContext context) =>
{
    // If user is already authenticated, redirect to dashboard
    if (context.User?.Identity?.IsAuthenticated == true)
    {
        return Results.Redirect("/Admin/Dashboard");
    }
    // Otherwise redirect to login
    return Results.Redirect("/Account/Login");
});

// Keep the public inspection route available for QR codes and public access
app.MapGet("/public", () => Results.Redirect("/PublicInspection"));
app.MapGet("/inspection", () => Results.Redirect("/PublicInspection"));

app.MapRazorPages();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();