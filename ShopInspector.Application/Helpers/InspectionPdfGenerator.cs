using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ShopInspector.Core.Entities;

namespace ShopInspector.Application.Helpers;

public static class InspectionPdfGenerator
{
    public static byte[] Generate(AssetInspection insp, IReadOnlyList<(string Label, string PhysicalPath)> photoFiles)
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

                    // Summary
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
                        t.Cell().Text((bool)insp.ThirdParty ? "Yes" : "No");
                    });

                    // Checklist
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
                                var itemName = row.AssetCheckList?.InspectionCheckList?.InspectionCheckListName ?? "Unknown";
                                var status = row.IsChecked ? "OK" : "Fail";
                                var bg = row.IsChecked ? Colors.Green.Lighten5 : Colors.Red.Lighten5;

                                table.Cell().Background(bg).Padding(4).Text(itemName);
                                table.Cell().Background(bg).Padding(4).AlignCenter().Text(status)
                                    .FontColor(row.IsChecked ? Colors.Green.Darken2 : Colors.Red.Darken2).SemiBold();
                                table.Cell().Background(bg).Padding(4).Text(string.IsNullOrWhiteSpace(row.Remarks) ? "-" : row.Remarks);
                            }
                        });
                    }

                    // Notes
                    var notes = list.Where(x => !string.IsNullOrWhiteSpace(x.Remarks)).ToList();
                    if (notes.Count > 0)
                    {
                        column.Item().Text("Notes").Bold().FontSize(14);
                        foreach (var n in notes)
                        {
                            var nm = n.AssetCheckList?.InspectionCheckList?.InspectionCheckListName ?? "Item";
                            column.Item().Text($"• {nm}: {n.Remarks}");
                        }
                    }

                    // Photos
                    column.Item().Text($"Photos ({photoFiles.Count})").Bold().FontSize(14);
                    if (photoFiles.Count == 0)
                    {
                        column.Item().Text("No photos uploaded.").Italic().FontColor(Colors.Grey.Darken2);
                    }
                    else
                    {
                        // Grid 3 columns
                        var chunkSize = 3;
                        for (int i = 0; i < photoFiles.Count; i += chunkSize)
                        {
                            var slice = photoFiles.Skip(i).Take(chunkSize).ToList();
                            column.Item().Row(r =>
                            {
                                foreach (var p in slice)
                                {
                                    r.RelativeColumn().Border(1).Padding(4).Column(cc =>
                                    {
                                        if (System.IO.File.Exists(p.PhysicalPath))
                                            cc.Item().Height(120).Image(p.PhysicalPath, ImageScaling.FitArea);
                                        else
                                            cc.Item().Height(120).Background(Colors.Grey.Lighten3)
                                                .AlignCenter().AlignMiddle().Text("Missing").FontColor(Colors.Red.Darken2);

                                        cc.Item().Text(p.Label).FontSize(8).FontColor(Colors.Grey.Darken2);
                                    });
                                }
                            });
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

    private static IContainer HeadCell(IContainer c) =>
        c.Background(Colors.Grey.Lighten3).Padding(4).DefaultTextStyle(x => x.SemiBold());
}