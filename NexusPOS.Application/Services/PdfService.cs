using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using NexusPOS.Application.DTOs;
using NexusPOS.Application.Interfaces;

namespace NexusPOS.Application.Services;

public class PdfService : IPdfService
{
    public PdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateBillPdf(TabBillData billData)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Element(c => ComposeHeader(c, billData));
                page.Content().Element(c => ComposeContent(c, billData));
                page.Footer().AlignCenter().Text("Gracias por su visita - BOHUCO POS");
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container, TabBillData billData)
    {
        container.Column(column =>
        {
            column.Item().Text("BOHUCO")
                .FontSize(24).Bold().FontColor(Colors.Blue.Darken2);
            
            column.Item().Text("Restaurante & Bar")
                .FontSize(14).FontColor(Colors.Grey.Darken1);

            column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

            column.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text($"Mesa: {billData.TabId}");
                    c.Item().Text($"Cliente: {billData.CustomerName}");
                    c.Item().Text($"Ubicación: {billData.Location}");
                });

                row.RelativeItem().AlignRight().Column(c =>
                {
                    c.Item().Text($"Mesero: {billData.WaiterName}");
                    c.Item().Text($"Fecha: {billData.OpenedAt:dd/MM/yyyy}");
                    c.Item().Text($"Hora: {billData.OpenedAt:HH:mm}");
                });
            });

            column.Item().PaddingTop(15).Text("CUENTA")
                .FontSize(16).Bold().FontColor(Colors.Blue.Darken2);
        });
    }

    private void ComposeContent(IContainer container, TabBillData billData)
    {
        container.PaddingTop(10).Column(column =>
        {
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(40);
                    columns.RelativeColumn(3);
                    columns.ConstantColumn(50);
                    columns.ConstantColumn(70);
                    columns.ConstantColumn(70);
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("#").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Producto").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Cant").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Precio").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Total").Bold();
                });

                int index = 1;
                foreach (var item in billData.Items)
                {
                    var bgColor = index % 2 == 0 ? Colors.White : Colors.Grey.Lighten5;

                    table.Cell().Background(bgColor).Padding(5).Text(index.ToString());
                    table.Cell().Background(bgColor).Padding(5).Column(c =>
                    {
                        c.Item().Text(item.ProductName);
                        if (!string.IsNullOrEmpty(item.Notes))
                            c.Item().Text($"   ({item.Notes})").FontSize(9).FontColor(Colors.Grey.Darken1);
                    });
                    table.Cell().Background(bgColor).Padding(5).AlignRight().Text(item.Quantity.ToString());
                    table.Cell().Background(bgColor).Padding(5).AlignRight().Text($"${item.UnitPrice:N2}");
                    table.Cell().Background(bgColor).Padding(5).AlignRight().Text($"${item.Total:N2}");

                    index++;
                }
            });

            column.Item().PaddingTop(15).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

            column.Item().PaddingTop(10).AlignRight().Column(c =>
            {
                c.Item().Row(r =>
                {
                    r.RelativeItem().Text("Subtotal:");
                    r.ConstantItem(100).AlignRight().Text($"${billData.Subtotal:N2}");
                });
                c.Item().PaddingTop(5).Row(r =>
                {
                    r.RelativeItem().Text("IVA (18%):");
                    r.ConstantItem(100).AlignRight().Text($"${billData.Tax:N2}");
                });
                c.Item().PaddingTop(10).Text($"TOTAL: ${billData.Total:N2}")
                    .FontSize(16).Bold().FontColor(Colors.Blue.Darken2);
            });
        });
    }
}
