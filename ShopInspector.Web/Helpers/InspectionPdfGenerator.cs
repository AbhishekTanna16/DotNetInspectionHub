using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ShopInspector.Core.Entities;

public class InspectionPdfGenerator : IDocument
{
    private readonly AssetInspection _insp;

    public InspectionPdfGenerator(AssetInspection insp)
    {
        _insp = insp;
    }

    public DocumentMetadata GetMetadata() => new DocumentMetadata { Title = "Inspection Report" };

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(20);
            page.Size(PageSizes.A4);

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().AlignCenter().Text($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm}");
        });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text($"Inspection Report").FontSize(18).Bold();
                col.Item().Text($"#{_insp.AssetInspectionID}").FontSize(12);
            });
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingVertical(10).Column(col =>
        {
            col.Spacing(10);

            col.Item().Text($"Asset: {_insp.Asset?.AssetName}").FontSize(13);
            col.Item().Text($"Inspector: {_insp.InspectorName}");
            col.Item().Text($"Date: {_insp.InspectionDate:yyyy-MM-dd HH:mm}");
            col.Item().Text($"Employee: {_insp.Employee?.EmployeeName}");
            col.Item().Text($"Freequency: {_insp.InspectionFrequency.FrequencyName}");

            col.Item().PaddingTop(10).Text("Checklist").FontSize(14).Bold();

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(1);
                });

                table.Header(h =>
                {
                    h.Cell().Text("Item").SemiBold();
                    h.Cell().Text("Status").SemiBold();
                });

                foreach (var item in _insp.AssetInspectionCheckLists)
                {
                    table.Cell().Text(item.AssetCheckList?.InspectionCheckList?.InspectionCheckListName ?? "N/A");
                    table.Cell().Text(item.IsChecked ? "OK" : "Not OK");
                }
            });

            if (!string.IsNullOrEmpty(_insp.Attachment))
            {
                col.Item().PaddingTop(15).Text("Photo").FontSize(14).Bold();

                col.Item().Element(container =>
                {
                    try
                    {
                        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", _insp.Attachment.TrimStart('/'));
                        container.Image(fullPath, ImageScaling.FitWidth);
                    }
                    catch
                    {
                        container.Text("Image could not be loaded.");
                    }
                });
            }
        });
    }

    public static byte[] Generate(AssetInspection insp)
    {
        return new InspectionPdfGenerator(insp).GeneratePdf();
    }
}
