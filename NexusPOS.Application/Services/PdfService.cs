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
                page.Margin(4);
                page.DefaultTextStyle(x => x.FontSize(7));

                page.Header().Element(c => ComposeHeader(c, billData));
                page.Content().Element(c => ComposeContent(c, billData));
                page.Footer().Element(c => ComposeFooter(c));
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container, TabBillData billData)
    {
        container.Column(column =>
        {
            column.Item().AlignCenter().Text("BOHUCO").FontSize(10).Bold();
            column.Item().AlignCenter().Text("SISTEMAS DE COMANDAS").FontSize(5);
            column.Item().PaddingTop(2).AlignCenter().Text("RNC: 000-000000-0").FontSize(4);
            column.Item().PaddingTop(3).LineHorizontal(1).LineColor(Colors.Black);
            column.Item().PaddingTop(2).Text($"CONSUMIDOR: #{billData.TabId}").FontSize(6);
            column.Item().Text($"CLIENTE: {billData.CustomerName}").FontSize(6);
            column.Item().Text($"UBICACION: {billData.Location}").FontSize(6);
            column.Item().Text($"MESERO: {billData.WaiterName}").FontSize(6);
            column.Item().Text($"FECHA: {billData.OpenedAt:dd/MM/yyyy HH:mm}").FontSize(6);
            column.Item().PaddingTop(2).LineHorizontal(1).LineColor(Colors.Black);
        });
    }

    private void ComposeContent(IContainer container, TabBillData billData)
    {
        container.PaddingTop(2).Column(column =>
        {
            column.Item().Row(r =>
            {
                r.RelativeItem().Text("ARTICULO").Bold().FontSize(5);
                r.ConstantItem(20).AlignRight().Text("CANT").Bold().FontSize(5);
                r.ConstantItem(25).AlignRight().Text("PRECIO").Bold().FontSize(5);
                r.ConstantItem(30).AlignRight().Text("TOTAL").Bold().FontSize(5);
            });

            column.Item().PaddingTop(1).LineHorizontal(1).LineColor(Colors.Grey.Medium);

            foreach (var item in billData.Items)
            {
                column.Item().PaddingVertical(1).Row(r =>
                {
                    var name = item.ProductName.Length > 18 ? item.ProductName.Substring(0, 18) : item.ProductName;
                    r.RelativeItem().Text(name).FontSize(6);
                    r.ConstantItem(20).AlignRight().Text(item.Quantity.ToString()).FontSize(6);
                    r.ConstantItem(25).AlignRight().Text($"${item.UnitPrice:N2}").FontSize(6);
                    r.ConstantItem(30).AlignRight().Text($"${item.Total:N2}").FontSize(6);
                });

                if (!string.IsNullOrEmpty(item.Notes))
                {
                    column.Item().Text($"   - {item.Notes}").FontSize(5).FontColor(Colors.Grey.Darken1);
                }
            }

            column.Item().PaddingTop(3).LineHorizontal(1).LineColor(Colors.Black);

            column.Item().PaddingTop(2).Column(c =>
            {
                c.Item().Row(r =>
                {
                    r.RelativeItem().Text("Subtotal:");
                    r.ConstantItem(40).AlignRight().Text($"${billData.Subtotal:N2}");
                });
                
                c.Item().Row(r =>
                {
                    r.RelativeItem().Text("ITBIS (18%):");
                    r.ConstantItem(40).AlignRight().Text($"${billData.Tax:N2}");
                });

                c.Item().PaddingTop(2).LineHorizontal(1).LineColor(Colors.Black);

                c.Item().PaddingTop(2).Row(r =>
                {
                    r.RelativeItem().Text("TOTAL:").Bold().FontSize(8);
                    r.ConstantItem(40).AlignRight().Text($"${billData.Total:N2}").Bold().FontSize(8);
                });
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Black);
            column.Item().PaddingTop(3).AlignCenter().Text("GRACIAS POR SU VISITA").FontSize(7).Bold();
            column.Item().PaddingTop(1).AlignCenter().Text("Vuelva pronto").FontSize(5).FontColor(Colors.Grey.Darken1);
            column.Item().PaddingTop(3).AlignCenter().Text("Powered by BohucoPOS").FontSize(4).FontColor(Colors.Grey.Medium);
        });
    }
}
