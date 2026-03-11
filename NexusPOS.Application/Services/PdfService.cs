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
                page.Size(226, 0);
                page.Margin(5);
                page.DefaultTextStyle(x => x.FontSize(7));

                page.Header().Element(c => ComposeHeader(c, billData));
                page.Content().Element(c => ComposeContent(c, billData));
                page.Footer().Element(c => ComposeFooter(c, billData));
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container, TabBillData billData)
    {
        container.Column(column =>
        {
            column.Item().PaddingBottom(4).AlignCenter().Text("━━━━━━━━━━━━━━━━━━");

            column.Item().AlignCenter().Text("BOHUCO")
                .FontSize(16).Bold().FontColor(Colors.Black);
            
            column.Item().AlignCenter().Text("SISTEMAS DE COMANDAS")
                .FontSize(6).FontColor(Colors.Grey.Darken1);

            column.Item().PaddingTop(2).AlignCenter().Text("RNC: 000-000000-0")
                .FontSize(5).FontColor(Colors.Grey.Medium);

            column.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Black);

            column.Item().PaddingTop(4).Column(c =>
            {
                c.Item().AlignCenter().Text($"#{billData.TabId}").FontSize(9).Bold();
                c.Item().PaddingTop(2).Text($"Cliente: {billData.CustomerName}").FontSize(7);
                c.Item().Text($"Mesa: {billData.Location}").FontSize(7);
                c.Item().Text($"Mesero: {billData.WaiterName}").FontSize(7);
                c.Item().Text($"Fecha: {billData.OpenedAt:dd/MM/yyyy}  {billData.OpenedAt:HH:mm}").FontSize(7);
            });

            column.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Black);
        });
    }

    private void ComposeContent(IContainer container, TabBillData billData)
    {
        container.PaddingTop(4).Column(column =>
        {
            column.Item().Row(r =>
            {
                r.RelativeItem(3).Text("ARTICULO").Bold().FontSize(6);
                r.RelativeItem(1).AlignRight().Text("CANT").Bold().FontSize(6);
                r.RelativeItem(1).AlignRight().Text("PRECIO").Bold().FontSize(6);
                r.RelativeItem(1).AlignRight().Text("IMPORTE").Bold().FontSize(6);
            });

            column.Item().PaddingTop(2).LineHorizontal(1).LineColor(Colors.Grey.Medium);

            foreach (var item in billData.Items)
            {
                column.Item().PaddingVertical(3).Row(r =>
                {
                    r.RelativeItem(3).Text(item.ProductName.Length > 20 
                        ? item.ProductName.Substring(0, 20) 
                        : item.ProductName).FontSize(7);
                    r.RelativeItem(1).AlignRight().Text(item.Quantity.ToString()).FontSize(7);
                    r.RelativeItem(1).AlignRight().Text($"${item.UnitPrice:N2}").FontSize(7);
                    r.RelativeItem(1).AlignRight().Text($"${item.Total:N2}").FontSize(7);
                });

                if (!string.IsNullOrEmpty(item.Notes))
                {
                    column.Item().Text($"   └ {item.Notes}").FontSize(6).FontColor(Colors.Grey.Darken1);
                }
            }

            column.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Black);

            column.Item().PaddingTop(4).Column(c =>
            {
                c.Item().Row(r =>
                {
                    r.RelativeItem().Text("Subtotal:");
                    r.ConstantItem(45).AlignRight().Text($"${billData.Subtotal:N2}");
                });
                
                c.Item().Row(r =>
                {
                    r.RelativeItem().Text("ITBIS (18%):");
                    r.ConstantItem(45).AlignRight().Text($"${billData.Tax:N2}");
                });

                c.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Black);

                c.Item().PaddingTop(4).Row(r =>
                {
                    r.RelativeItem().Text("TOTAL A PAGAR:").FontSize(10).Bold();
                    r.ConstantItem(45).AlignRight().Text($"${billData.Total:N2}").FontSize(10).Bold();
                });
            });
        });
    }

    private void ComposeFooter(IContainer container, TabBillData billData)
    {
        container.Column(column =>
        {
            column.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Black);

            column.Item().PaddingTop(4).AlignCenter().Text("¡GRACIAS POR SU VISITA!")
                .FontSize(8).Bold();

            column.Item().PaddingTop(2).AlignCenter().Text("Vuelva pronto")
                .FontSize(6).FontColor(Colors.Grey.Darken1);

            column.Item().PaddingTop(4).AlignCenter().Text("Powered by BohucoPOS")
                .FontSize(5).FontColor(Colors.Grey.Medium);

            column.Item().PaddingTop(4).AlignCenter().Text("━━━━━━━━━━━━━━━━━━");
        });
    }
}
