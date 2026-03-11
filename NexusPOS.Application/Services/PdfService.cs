using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using NexusPOS.Application.DTOs;
using NexusPOS.Application.Interfaces;

namespace NexusPOS.Application.Services;

public class PdfService : IPdfService
{
    private static class Theme
    {
        public const string Black       = "#000000";
        public const string DarkGray    = "#1F2937";
        public const string MediumGray  = "#4B5563";
        public const string LightGray   = "#9CA3AF";
        public const string BorderLight = "#D1D5DB";
    }

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
                page.ContinuousSize(227, Unit.Point);
                page.MarginHorizontal(10);
                page.MarginVertical(8);
                page.DefaultTextStyle(x => x
                    .FontFamily("Arial")
                    .FontSize(8)
                    .FontColor(Theme.DarkGray));

                page.Header().Element(c => ComposeHeader(c, billData));
                page.Content().Element(c => ComposeContent(c, billData));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container, TabBillData billData)
    {
        container.Column(col =>
        {
            col.Item().AlignCenter().Text("BOHUCO POS")
                .FontSize(15).Bold().FontColor(Theme.Black);

            col.Item().AlignCenter().Text("Sistemas de Comandas")
                .FontSize(6.5f).FontColor(Theme.LightGray);

            col.Item().PaddingTop(2).AlignCenter().Text("RNC: 1-01-12345-6")
                .FontSize(6.5f).FontColor(Theme.MediumGray);

            col.Item().PaddingVertical(5).LineHorizontal(0.5f)
                .LineColor(Theme.BorderLight);

            col.Item().AlignCenter()
                .Text(billData.InvoiceNumber ?? $"CUENTA-{billData.TabId}")
                .FontSize(8).Bold().FontColor(Theme.DarkGray);

            col.Item().PaddingTop(1).AlignCenter()
                .Text($"NCF: {billData.Ncf ?? "—"}")
                .FontSize(6.5f).FontColor(Theme.LightGray);

            col.Item().PaddingVertical(5).LineHorizontal(0.5f)
                .LineColor(Theme.BorderLight);

            col.Item().Column(c =>
            {
                InfoRow(c, "Cuenta",  billData.TabId);
                InfoRow(c, "Cliente", billData.CustomerName);
                InfoRow(c, "Mesa",    billData.Location,   bold: true);
                InfoRow(c, "Mesero",  billData.WaiterName);
                InfoRow(c, "Fecha",
                    $"{billData.OpenedAt:dd/MM/yyyy}  {billData.OpenedAt:HH:mm}");
            });

            col.Item().PaddingTop(6).LineHorizontal(0.5f)
                .LineColor(Theme.BorderLight);
        });
    }

    private void ComposeContent(IContainer container, TabBillData billData)
    {
        container.PaddingTop(5).Column(col =>
        {
            col.Item()
                .BorderBottom(0.5f).BorderColor(Theme.BorderLight)
                .PaddingBottom(3)
                .Row(row =>
                {
                    row.RelativeItem()
                        .Text("DESCRIPCION")
                        .FontSize(6.5f).Bold().FontColor(Theme.MediumGray);
                    row.ConstantItem(22)
                        .AlignCenter()
                        .Text("CANT")
                        .FontSize(6.5f).Bold().FontColor(Theme.MediumGray);
                    row.ConstantItem(50)
                        .AlignRight()
                        .Text("IMPORTE")
                        .FontSize(6.5f).Bold().FontColor(Theme.MediumGray);
                });

            var items = billData.Items.ToList();
            for (int i = 0; i < items.Count; i++)
            {
                var item   = items[i];
                var isLast = i == items.Count - 1;

                col.Item()
                    .BorderBottom(isLast ? 0 : 0.3f)
                    .BorderColor(Theme.BorderLight)
                    .PaddingVertical(4)
                    .Column(c =>
                    {
                        c.Item().Row(r =>
                        {
                            r.RelativeItem()
                                .Text(item.ProductName)
                                .FontSize(8).Bold();

                            r.ConstantItem(22)
                                .AlignCenter()
                                .Text($"x{item.Quantity}")
                                .FontSize(8).FontColor(Theme.MediumGray);

                            r.ConstantItem(50)
                                .AlignRight()
                                .Text($"RD$ {item.Total:N2}")
                                .FontSize(8).Bold();
                        });

                        c.Item().PaddingTop(1)
                            .Text($"RD$ {item.UnitPrice:N2} c/u  ·  {item.Destination ?? "General"}")
                            .FontSize(6).FontColor(Theme.LightGray);

                        if (!string.IsNullOrWhiteSpace(item.Notes))
                            c.Item().PaddingTop(1)
                                .Text($"Nota: {item.Notes}")
                                .FontSize(6.5f).FontColor(Theme.MediumGray);
                    });
            }

            col.Item().PaddingTop(4).LineHorizontal(0.5f)
                .LineColor(Theme.BorderLight);

            col.Item().PaddingTop(5).Column(c =>
            {
                TotalRow(c, "Subtotal",    $"RD$ {billData.Subtotal:N2}");
                TotalRow(c, "ITBIS (18%)", $"RD$ {billData.Tax:N2}");
            });

            col.Item()
                .PaddingTop(4)
                .BorderTop(1.5f).BorderColor(Theme.DarkGray)
                .PaddingTop(4)
                .Row(row =>
                {
                    row.RelativeItem()
                        .Text("TOTAL")
                        .FontSize(11).Bold().FontColor(Theme.Black);
                    row.ConstantItem(70)
                        .AlignRight()
                        .Text($"RD$ {billData.Total:N2}")
                        .FontSize(11).Bold().FontColor(Theme.Black);
                });

            col.Item().PaddingTop(6).LineHorizontal(0.5f)
                .LineColor(Theme.BorderLight);

            col.Item().PaddingTop(4).Row(row =>
            {
                row.RelativeItem()
                    .Text("Forma de pago")
                    .FontSize(7).FontColor(Theme.LightGray);
                row.AutoItem()
                    .Text(billData.PaymentMethod ?? "—")
                    .FontSize(7).Bold().FontColor(Theme.DarkGray);
            });

            if (!string.IsNullOrWhiteSpace(billData.BusinessRnc))
            {
                col.Item().PaddingTop(2).Row(row =>
                {
                    row.RelativeItem()
                        .Text($"RNC: {billData.BusinessRnc}")
                        .FontSize(6.5f).FontColor(Theme.LightGray);
                    row.AutoItem()
                        .Text($"NCF: {billData.Ncf ?? "—"}")
                        .FontSize(6.5f).FontColor(Theme.LightGray);
                });
            }
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().PaddingTop(8).LineHorizontal(0.5f)
                .LineColor(Theme.BorderLight);

            col.Item().PaddingTop(6).AlignCenter()
                .Text("¡Gracias por su visita!")
                .FontSize(8).Bold().FontColor(Theme.DarkGray);

            col.Item().PaddingTop(1).AlignCenter()
                .Text("Comprobante Fiscal Digital")
                .FontSize(6).FontColor(Theme.LightGray);

            col.Item().PaddingTop(6).AlignCenter()
                .Text("- - - - - - - - - - - - - - - - - - - - - - -")
                .FontSize(7).FontColor(Theme.LightGray);
        });
    }

    private static void InfoRow(ColumnDescriptor col, string label, string value, bool bold = false)
    {
        col.Item().PaddingBottom(2).Row(row =>
        {
            row.ConstantItem(42)
                .Text(label)
                .FontSize(7).FontColor(Theme.LightGray);
            row.RelativeItem()
                .Text(value)
                .FontSize(7)
                .FontColor(bold ? Theme.Black : Theme.DarkGray);
        });
    }

    private static void TotalRow(ColumnDescriptor col, string label, string value)
    {
        col.Item().PaddingBottom(2).Row(row =>
        {
            row.RelativeItem()
                .Text(label)
                .FontSize(8).FontColor(Theme.LightGray);
            row.ConstantItem(70)
                .AlignRight()
                .Text(value)
                .FontSize(8).FontColor(Theme.DarkGray);
        });
    }
}
