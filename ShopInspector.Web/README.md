# ShopInspector

A mobile-friendly equipment inspection web application for machine shops. Supports QR code asset access, public inspection submission (no login required), and an Admin area for full data management. Built with .NET 9, Razor Pages + MVC hybrid, EF Core + SQL Server, Serilog logging, QuestPDF export, and Bootstrap 5 responsive UI.

## Table of Contents
1. Overview
2. Core Features
3. Technology Stack
4. Architecture & Project Structure
5. Data Model Summary
6. Inspection Workflow (Public + QR)
7. Admin Area Capabilities
8. Security Model
9. Mobile UX Enhancements
10. PDF Report Generation
11. QR Code Generation Flow
12. File Upload & Image Handling
13. Logging (Serilog)
14. Setup & Installation
15. Running Migrations
16. Default Credentials
17. Configuration (appsettings.json)
18. Extensibility Points
19. Troubleshooting
20. License / Usage Notes

---

## 1. Overview
ShopInspector enables quick equipment inspections directly from a mobile device. A QR code affixed to an asset opens its public inspection start page. Users complete checklists, capture photos, add notes, and submit. Administrators manage all master data (assets, checklists, employees, frequencies, companies) and inspect history with export to PDF.

Public routes remain open; Admin routes are protected with role-based authorization.

---

## 2. Core Features
- Public inspection submission (anonymous or named)
- QR code deep link (Asset → Inspection Start)
- Asset-specific dynamic checklist
- Photo capture (mobile camera) with validation
- Progress-friendly responsive forms
- Inspection history & detail views
- PDF report export (QuestPDF)
- Admin CRUD management for:
  - Assets / Asset Types
  - Inspection Check Lists
  - Asset ↔ Checklist mappings
  - Inspection Frequencies
  - Companies
  - Employees
  - Inspections + Photo viewing
- Secure file upload (size, type)
- Serilog structured logging
- Anti-forgery protection on admin forms

---

## 3. Technology Stack
- Framework: .NET 9 (ASP.NET Core Razor Pages + MVC Controllers)
- UI: Bootstrap 5 (custom responsive tweaks)
- Database: SQL Server (Entity Framework Core)
- PDF: QuestPDF Community license
- Logging: Serilog (Console + Rolling File)
- QR Codes: Generated server-side (`QRCodeService`)
- Auth: Cookie-based, role "Admin"
- Frontend Enhancements: Vanilla JS (`admin-actions.js`, `inspection-compress.js`)

---

## 4. Architecture & Project Structure

The solution follows a Clean Architecture-inspired layering:

- Core (Entities)
- Application (Interfaces, Services, DTOs, Helpers)
- Infrastructure (Data, EF Core Migrations, Repositories, File & QR services)
- Web (UI – Razor Pages + MVC, Controllers, Views, Static assets)

Key folders:
- `ShopInspector.Core/Entities` – POCO entity definitions
- `ShopInspector.Infrastructure/Data/AppDbContext.cs` – EF Core context
- `ShopInspector.Infrastructure/Repositories` – Repository implementations
- `ShopInspector.Application/Services` – Business services (consume repositories)
- `ShopInspector.Web/Areas/Admin` – Admin controllers & views
- `ShopInspector.Web/Controllers/PublicInspectionController.cs` – Public inspection flow
- `ShopInspector.Application/Helpers/InspectionPdfGenerator.cs` – PDF export logic
- `ShopInspector.Web/wwwroot/js` – Client scripts
- `ShopInspector.Web/Program.cs` – Composition root, Serilog, auth, routing

Razor Pages root configured as `/Views/Pages` for hybrid usage (`builder.Services.AddRazorPages(options => options.RootDirectory = "/Views/Pages"; )`).

---

## 5. Data Model Summary (Simplified)
- Asset
- AssetType
- InspectionCheckList (master checklist items)
- AssetCheckList (mapping Asset → Checklist item + DisplayOrder + Active)
- InspectionFrequency
- Employee
- Company
- AssetInspection (header)
- AssetInspectionCheckList (inspection result per checklist item + IsChecked + Remarks)
- InspectionPhoto (photo per inspection)

---

## 6. Inspection Workflow (Public + QR)
1. User scans QR (e.g., `https://yourhost/PublicInspection/Start/{assetId}`).
2. Public Start page loads:
   - Asset metadata
   - Ordered active checklist items
   - Optional past inspection summary
   - Dropdowns (Employee, Frequency)
3. User fills form, toggles checklist items, adds remarks, captures photos.
4. Submission (POST `/PublicInspection/Submit`) creates:
   - `AssetInspection`
   - Photos (first photo saved as cover `Attachment`)
   - Checklist result rows
5. Redirect to Thanks page `/PublicInspection/Thanks?assetId=...&id=...`

History:
- `/PublicInspection/History/{assetId}`
Details:
- `/PublicInspection/Details/{inspectionId}`

No authentication required for public routes due to `[AllowAnonymous]`.

---

## 7. Admin Area Capabilities
All under `/Admin/*` protected by `[Authorize(Policy = "AdminOnly")]`.

Controllers include:
- `AssetController`
- `AssetTypeController`
- `InspectionCheckListController`
- `AssetCheckListController`
- `InspectionFrequencyController`
- `EmployeeController`
- `CompanyController`
- `InspectionController` (Manual admin inspections + PDF export)
- `DashboardController`

Admin UX uses `admin-actions.js` for:
- Declarative navigation (`data-action="navigate"`)
- AJAX delete (`data-action="delete"`)
- Save actions (`data-action="save"`)
- QR Generation (`data-action="generate-qr"` → `/Admin/Asset/GenerateQr/{id}`)

---

## 8. Security Model
- Authentication: Cookie scheme "Cookies"
- Authorization Policy: "AdminOnly" => Role "Admin"
- Public controller explicitly `[AllowAnonymous]`
- Anti-forgery: `@Html.AntiForgeryToken()` required on modifying forms
- SQL injection: Entity Framework parameterization
- HTTPS enforced (`app.UseHttpsRedirection()`)
- File Upload Validation:
  - Content type & extension checked in `FileService`
  - Size limited via `FormOptions.MultipartBodyLengthLimit`
- Login path: `/Account/Login`
- Hardcoded demo credentials (must be replaced): `admin / password`

---

## 9. Mobile UX Enhancements
Implemented:
- Responsive layout (Bootstrap grid, fluid images)
- Tappable target sizes (buttons ≥ 44px height)
- Camera capture: `<input type="file" accept="image/*" capture="environment">`
- Compression script (`inspection-compress.js`) pre-submit (if implemented)
- Viewport meta in `_Layout.cshtml`
- Simplified forms, large hit areas, stacked content
- Toast notifications for feedback (`admin-actions.js`)

---

## 10. PDF Report Generation
QuestPDF used via `InspectionPdfGenerator`:

Signature:

It:
- Renders summary metadata table
- Renders checklist with color-coded pass/fail
- Includes remarks / notes
- Embeds images if physical files exist (3-column grid)
- Adds generation timestamp (UTC)

Usage (Admin):


---

## 11. QR Code Generation Flow
- Admin clicks "Generate QR" button (`data-action="generate-qr"`)
- Calls `/Admin/Asset/GenerateQr/{id}`
- Service builds URL to public Start route (`/PublicInspection/Start/{assetId}`)
- QR image saved & path returned
- Modal displays image + download link

---

## 12. File Upload & Image Handling
- Validation in `FileService.IsAllowedImage(IFormFile file)`
- Saved path format: `/uploads/inspect/{inspectionId}/{guid}.jpg`
- First uploaded image becomes inspection `Attachment`
- Max request body size: 50MB (configurable)
- Client compression (optional) before transmit

Recommended improvements (future):
- EXIF strip
- Thumbnail generation
- Server-side resizing

---

## 13. Logging (Serilog)
Configured in `Program.cs`:
- Console + Rolling File sink (`logs/app-.log`)
- Minimum levels override Microsoft noise
- Enrichment: ProcessId, ThreadId, Environment
- Request logging middleware (`app.UseSerilogRequestLogging`)
Adjust in `appsettings.json` under `"Serilog"` section.

Example:

---

## 14. Setup & Installation

Prerequisites:
- .NET 9 SDK
- SQL Server (LocalDB, Developer, or Azure SQL)
- Node optional (not required)

Steps:


---

## 16. Default Credentials (Demo Only)
- Username: `admin`
- Password: `password`

Replace with a proper credential system (ASP.NET Identity or custom user table) before production.

---

## 17. Configuration (appsettings.json)
Key sections:
- `ConnectionStrings.DefaultConnection`
- `Serilog`
- (Optional future) `FileUpload:MaxSizeMb`
- (Optional future) `QrCode:BasePublicUrl`

---

## 18. Extensibility Points
| Area | How to Extend |
|------|---------------|
| Auth | Replace hardcoded check with DB + hashed passwords |
| Roles | Add policy variations (e.g., "Manager") |
| Checklist | Add categories or severity levels |
| Photos | Add async background processing |
| PDF | Add branding header/footer, company logo injection |
| QR | Add tracking UTM params |

---

## 19. Troubleshooting
| Issue | Cause | Fix |
|-------|-------|-----|
| Admin pages not redirecting to login | Missing `[Authorize]` | Ensure `[Authorize(Policy="AdminOnly")]` present |
| Images not showing in PDF | Physical path mismatch | Verify `wwwroot` path + file existence |
| Large upload failure | Exceeds limit | Adjust `MultipartBodyLengthLimit` |
| QR opens wrong domain | Hardcoded ngrok dev URL | Externalize base URL config |
| EF migration errors | Context mismatch | Use correct `--project` & `--startup-project` |
| 403 on form POST | Missing anti-forgery token | Add `@Html.AntiForgeryToken()` in form |

---

## 20. License / Usage Notes
- QuestPDF used under Community license.
- Replace demo credentials and review security before production.
- Ensure HTTPS termination and secure hosting environment.
- Validate and sanitize any future rich-text user input.

---

## API / Route Quick Reference

Public (Anonymous):
- `GET /PublicInspection/Assets`
- `GET /PublicInspection/Start/{assetId}`
- `POST /PublicInspection/Submit`
- `GET /PublicInspection/History/{assetId}`
- `GET /PublicInspection/Details/{inspectionId}`
- `GET /PublicInspection/Thanks`

Admin (Requires Role=Admin):
- `GET /Admin/Asset`
- `GET /Admin/Asset/Create` / `POST /Admin/Asset/Create`
- `GET /Admin/Asset/Edit/{id}` / `POST /Admin/Asset/Edit`
- `POST /Admin/Asset/Delete/{id}`
- Similar CRUD for AssetType, Company, Employee, InspectionCheckList, InspectionFrequency, AssetCheckList
- `GET /Admin/Inspection/Export/{inspectionId}` (PDF)
- `GET /Admin/Asset/GenerateQr/{id}` (QR image view)

---

## PDF Helper Example


---

## Disclaimer
This repository contains a demonstration authentication approach. Do not deploy with hardcoded credentials. Implement a secure password store and user management before production.

---

## Next Steps (Recommended)
- Integrate ASP.NET Core Identity
- Add audit trails (CreatedBy / ModifiedBy) across entities
- Add automated test suite (unit + integration)
- Add caching for frequently accessed lookup tables
- Add CI pipeline for build + migrations validation

---

Built for reliable, mobile-first inspection capture with a maintainable architecture. Let us know if you need deployment guidance or Identity integration.
