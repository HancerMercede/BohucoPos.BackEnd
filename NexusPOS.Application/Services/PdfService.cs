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
                page.Size(200, 0);
                page.Margin(10);
                page.DefaultTextStyle(x => x.FontSize(8));

                page.Header().Element(c => ComposeHeader(c, billData));
                page.Content().Element(c => ComposeContent(c, billData));
                page.Footer().AlignCenter().Text("Gracias por su visita").FontSize(7);
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container, TabBillData billData)
    {
        container.Column(column =>
        {
            column.Item().AlignCenter().Text("BohucoPOS")
                .FontSize(14).Bold();
            
            column.Item().AlignCenter().Text("Sistemas de Comandas")
                .FontSize(7).FontColor(Colors.Grey.Darken1);

            column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

            column.Item().PaddingTop(5).Column(c =>
            {
                c.Item().Text($"Cuenta: {billData.TabId}");
                c.Item().Text($"Cliente: {billData.CustomerName}");
                c.Item().Text($"Mesa: {billData.Location}");
                c.Item().Text($"Mesero: {billData.WaiterName}");
                c.Item().Text($"Fecha: {billData.OpenedAt:dd/MM/yyyy} {billData.OpenedAt:HH:mm}");
            });

            column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
        });
    }

    private void ComposeContent(IContainer container, TabBillData billData)
    {
        container.PaddingTop(5).Column(column =>
        {
            foreach (var item in billData.Items)
            {
                column.Item().PaddingVertical(2).Column(c =>
                {
                    c.Item().Row(r =>
                    {
                        r.RelativeItem().Text($"{item.Quantity}x {item.ProductName}");
                        r.ConstantItem(50).AlignRight().Text($"${item.Total:N2}");
                    });
                    if (!string.IsNullOrEmpty(item.Notes))
                        c.Item().Text($"   {item.Notes}").FontSize(7).FontColor(Colors.Grey.Darken1);
                });
            }

            column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

            column.Item().PaddingTop(5).Column(c =>
            {
                c.Item().Row(r =>
                {
                    r.RelativeItem().Text("Subtotal:");
                    r.ConstantItem(50).AlignRight().Text($"${billData.Subtotal:N2}");
                });
                c.Item().Row(r =>
                {
                    r.RelativeItem().Text("IVA (18%):");
                    r.ConstantItem(50).AlignRight().Text($"${billData.Tax:N2}");
                });
                c.Item().PaddingTop(3).Row(r =>
                {
                    r.RelativeItem().Text("TOTAL:").Bold();
                    r.ConstantItem(50).AlignRight().Text($"${billData.Total:N2}").Bold();
                });
            });
        });
    }
}
