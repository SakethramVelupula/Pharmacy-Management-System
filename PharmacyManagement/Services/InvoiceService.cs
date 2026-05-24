using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Data;
using PharmacyManagement.Interface;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PharmacyManagement.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InvoiceService> _logger;

        public InvoiceService(ApplicationDbContext context, ILogger<InvoiceService> logger)
        {
            _context = context;
            _logger = logger;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> GenerateInvoiceAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Drug)
                .FirstOrDefaultAsync(o => o.Id == orderId)
                ?? throw new KeyNotFoundException($"Order {orderId} not found.");

            var placer = await _context.Users.FindAsync(order.PlacedById);
            var payment = await _context.PaymentTransactions
                .FirstOrDefaultAsync(p => p.OrderId == orderId);

            var totalAmount = order.Drug.PricePerUnit * order.Quantity;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Element(ComposeHeader);

                    page.Content().Element(content =>
                    {
                        content.Column(col =>
                        {
                            col.Spacing(15);

                            // Invoice Info
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text($"Invoice #INV-{orderId:D6}").Bold().FontSize(14);
                                    c.Item().Text($"Date: {DateTime.UtcNow:dd MMM yyyy}");
                                    c.Item().Text($"Order Date: {order.PlacedAt:dd MMM yyyy}");
                                    if (order.DateDispensed.HasValue)
                                        c.Item().Text($"Dispensed: {order.DateDispensed:dd MMM yyyy}");
                                });

                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("Bill To:").Bold();
                                    c.Item().Text(placer?.UserName ?? "Unknown");
                                    c.Item().Text(placer?.Email ?? "");
                                    c.Item().Text($"Role: {placer?.Role ?? ""}");
                                });
                            });

                            // Divider
                            col.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

                            // Order Table
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                });

                                // Header
                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(8)
                                        .Text("Drug Name").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(8)
                                        .Text("Unit Price").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(8)
                                        .Text("Quantity").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(8)
                                        .Text("Total").FontColor(Colors.White).Bold();
                                });

                                // Row
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8)
                                    .Text(order.Drug.Name);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8)
                                    .Text($"${order.Drug.PricePerUnit:F2}");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8)
                                    .Text(order.Quantity.ToString());
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8)
                                    .Text($"${totalAmount:F2}");
                            });

                            // Total
                            col.Item().AlignRight().Column(c =>
                            {
                                c.Item().Text($"Subtotal: ${totalAmount:F2}").FontSize(12);
                                c.Item().Text($"Total: ${totalAmount:F2}").Bold().FontSize(14);
                            });

                            // Payment Info
                            col.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                            col.Item().Column(c =>
                            {
                                c.Item().Text("Payment Information").Bold();
                                c.Item().Text($"Method: {order.PaymentMethod}");
                                c.Item().Text($"Status: {payment?.Status ?? "Pending"}");
                                if (!string.IsNullOrEmpty(order.PrescriptionReference))
                                    c.Item().Text($"Prescription Ref: {order.PrescriptionReference}");
                            });

                            // Order Status
                            col.Item().Background(Colors.Green.Lighten4).Padding(10).Column(c =>
                            {
                                c.Item().Text($"Order Status: {order.Status}").Bold().FontColor(Colors.Green.Darken3);
                            });
                        });
                    });

                    page.Footer().AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Pharmacy Management System | Generated on ");
                            x.Span(DateTime.UtcNow.ToString("dd MMM yyyy HH:mm"));
                            x.Span(" | Page ");
                            x.CurrentPageNumber();
                            x.Span(" of ");
                            x.TotalPages();
                        });
                });
            });

            _logger.LogInformation("Invoice generated for Order {OrderId}", orderId);
            return document.GeneratePdf();
        }

        private void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("PHARMACY MANAGEMENT SYSTEM")
                        .Bold().FontSize(20).FontColor(Colors.Blue.Darken2);
                    col.Item().Text("Official Invoice").FontSize(12).FontColor(Colors.Grey.Darken1);
                });

                row.ConstantItem(100).Height(50).Background(Colors.Blue.Darken2)
                    .AlignCenter().AlignMiddle()
                    .Text("INVOICE").FontColor(Colors.White).Bold().FontSize(16);
            });
        }
    }
}
