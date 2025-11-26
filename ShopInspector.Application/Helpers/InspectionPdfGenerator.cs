using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ShopInspector.Core.Entities;

namespace ShopInspector.Application.Helpers;

public static class InspectionPdfGenerator
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="insp"></param>
    /// <param name="photoFiles"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static byte[] Generate(AssetInspection insp, IReadOnlyList<(string Label, string PhysicalPath)> photoFiles)
    {
        if (insp == null)
        {
            throw new ArgumentNullException(nameof(insp), "Inspection cannot be null");
        }

        if (photoFiles == null)
        {
            throw new ArgumentNullException(nameof(photoFiles), "Photo files collection cannot be null");
        }

        try
        {
            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Inspection Report").FontSize(22).SemiBold();
                        col.Item().Text($"#{insp.AssetInspectionID}").FontSize(12).FontColor(Colors.Grey.Darken2);
                    });

                    page.Content().Column(column =>
                    {
                        column.Spacing(12);
                        column.Item().Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(3);
                                c.RelativeColumn(3);
                                c.RelativeColumn(3);
                            });

                            t.Cell().Element(HeadCell).Text("Asset:");
                            t.Cell().Text(insp.Asset?.AssetName ?? "N/A");
                            t.Cell().Element(HeadCell).Text("Code:");
                            t.Cell().Text(insp.Asset?.AssetCode ?? "N/A");
                            t.Cell().Element(HeadCell).Text("Inspector:");
                            t.Cell().Text(insp.InspectorName ?? "Anonymous");
                            t.Cell().Element(HeadCell).Text("Employee:");
                            t.Cell().Text(insp.Employee?.EmployeeName ?? "N/A");
                            t.Cell().Element(HeadCell).Text("Frequency:");
                            t.Cell().Text(insp.InspectionFrequency?.FrequencyName ?? "N/A");
                            t.Cell().Element(HeadCell).Text("Date:");
                            t.Cell().Text(insp.InspectionDate.ToString("yyyy-MM-dd HH:mm"));
                            t.Cell().Element(HeadCell).Text("Third Party:");
                            t.Cell().Text((insp.ThirdParty ?? false) ? "Yes" : "No");
                        });
                        column.Item().Text("Checklist").Bold().FontSize(14);
                        var list = insp.AssetInspectionCheckLists ?? new List<AssetInspectionCheckList>();
                        if (list.Count == 0)
                        {
                            column.Item().Text("No checklist items.").Italic().FontColor(Colors.Grey.Darken2);
                        }
                        else
                        {
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(c =>
                                {
                                    c.RelativeColumn(5);
                                    c.ConstantColumn(60);
                                    c.RelativeColumn(4);
                                });

                                table.Header(h =>
                                {
                                    h.Cell().Background(Colors.Blue.Darken2).Padding(4).Text("Item").FontColor(Colors.White).Bold();
                                    h.Cell().Background(Colors.Blue.Darken2).Padding(4).AlignCenter().Text("Status").FontColor(Colors.White).Bold();
                                    h.Cell().Background(Colors.Blue.Darken2).Padding(4).Text("Remarks").FontColor(Colors.White).Bold();
                                });

                                foreach (var row in list)
                                {
                                    var itemName = row?.AssetCheckList?.InspectionCheckList?.InspectionCheckListName ?? "Unknown";
                                    var isChecked = row?.IsChecked ?? false;
                                    var status = isChecked ? "OK" : "Fail";
                                    var bg = isChecked ? Colors.Green.Lighten5 : Colors.Red.Lighten5;
                                    var remarks = string.IsNullOrWhiteSpace(row?.Remarks) ? "-" : row.Remarks;

                                    table.Cell().Background(bg).Padding(4).Text(itemName);
                                    table.Cell().Background(bg).Padding(4).AlignCenter().Text(status)
                                        .FontColor(isChecked ? Colors.Green.Darken2 : Colors.Red.Darken2).SemiBold();
                                    table.Cell().Background(bg).Padding(4).Text(remarks);
                                }
                            });
                        }
                        var notes = list.Where(x => x != null && !string.IsNullOrWhiteSpace(x.Remarks)).ToList();
                        if (notes.Count > 0)
                        {
                            column.Item().Text("Notes").Bold().FontSize(14);
                            foreach (var n in notes)
                            {
                                var nm = n?.AssetCheckList?.InspectionCheckList?.InspectionCheckListName ?? "Item";
                                var remarks = n?.Remarks ?? "";
                                column.Item().Text($"• {nm}: {remarks}");
                            }
                        }
                        column.Item().Text($"Photos ({photoFiles.Count})").Bold().FontSize(14);
                        if (photoFiles.Count == 0)
                        {
                            column.Item().Text("No photos uploaded.").Italic().FontColor(Colors.Grey.Darken2);
                        }
                        else
                        {
                            var validPhotoFiles = photoFiles
                                .Where(p => !string.IsNullOrWhiteSpace(p.PhysicalPath) && 
                                           !string.IsNullOrWhiteSpace(p.Label))
                                .ToList();

                            if (validPhotoFiles.Count == 0)
                            {
                                column.Item().Text("No valid photos found.").Italic().FontColor(Colors.Grey.Darken2);
                            }
                            else
                            {
                                // Grid 3 columns
                                var chunkSize = 3;
                                for (int i = 0; i < validPhotoFiles.Count; i += chunkSize)
                                {
                                    var slice = validPhotoFiles.Skip(i).Take(chunkSize).ToList();
                                    column.Item().Row(r =>
                                    {
                                        foreach (var p in slice)
                                        {
                                            r.RelativeColumn().Border(1).Padding(4).Column(cc =>
                                            {
                                                try
                                                {
                                                    if (System.IO.File.Exists(p.PhysicalPath))
                                                    {
                                                        cc.Item().Height(120).Image(p.PhysicalPath, ImageScaling.FitArea);
                                                    }
                                                    else
                                                    {
                                                        cc.Item().Height(120).Background(Colors.Grey.Lighten3)
                                                            .AlignCenter().AlignMiddle().Text("Missing").FontColor(Colors.Red.Darken2);
                                                    }
                                                }
                                                catch (Exception)
                                                {
                                                    cc.Item().Height(120).Background(Colors.Grey.Lighten3)
                                                        .AlignCenter().AlignMiddle().Text("Error").FontColor(Colors.Red.Darken2);
                                                }

                                                cc.Item().Text(p.Label ?? "Unknown").FontSize(8).FontColor(Colors.Grey.Darken2);
                                            });
                                        }
                                    });
                                }
                            }
                        }

                        // Generated info
                        column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        column.Item().Text($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss 'UTC'}")
                            .FontSize(8).FontColor(Colors.Grey.Darken2);
                    });
                });
            });

            using var ms = new MemoryStream();
            doc.GeneratePdf(ms);
            return ms.ToArray();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to generate PDF for inspection {insp?.AssetInspectionID ?? 0}: {ex.Message}", ex);
        }
    }
    /// <summary>
    /// Creates a styled container representing a header cell with a light grey background, padding, and semi-bold text.
    /// </summary>
    /// <param name="c">The container to which the header cell styling will be applied.</param>
    /// <returns>A container with the applied header cell styling.</returns>
    private static IContainer HeadCell(IContainer c) =>
        c.Background(Colors.Grey.Lighten3).Padding(4).DefaultTextStyle(x => x.SemiBold());
}