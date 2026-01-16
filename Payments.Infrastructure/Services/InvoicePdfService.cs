using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Payments.Core.Services;
using Payments.Domain.Entities;
using System.Threading.Tasks;
using System;

namespace Payments.Infrastructure.Services
{
    public class InvoicePdfService : IPdfService<Invoice>
    {
        public Task<byte[]> GeneratePdfAsync(Invoice invoice)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("FACTURA DE SERVICIOS").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
                            col.Item().Text($"{invoice.InvoiceId.Value}").FontSize(10);
                        });

                        row.ConstantItem(100).Height(50).Placeholder(); 
                    });

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(x =>
                    {
                        x.Spacing(20);

                        x.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("CLIENTE").SemiBold();
                                c.Item().Text(invoice.UserName);
                                c.Item().Text($"ID Usuario: {invoice.UserId.Value}").FontSize(10);
                            });

                            row.RelativeItem().Column(c =>
                            {
                                c.Item().AlignRight().Text("DETALLES").SemiBold();
                                c.Item().AlignRight().Text($"Fecha: {invoice.CreatedAt:dd/MM/yyyy}");
                                c.Item().AlignRight().Text($"Pago Ref: {invoice.PaymentId.Value}");
                            });
                        });

                        x.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Descripción");
                                header.Cell().Element(CellStyle).AlignRight().Text("Cantidad");
                                header.Cell().Element(CellStyle).AlignRight().Text("Precio");

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                                }
                            });

                            table.Cell().Element(CellStyle).Text("Reserva de Evento");
                            table.Cell().Element(CellStyle).AlignRight().Text("1");
                            table.Cell().Element(CellStyle).AlignRight().Text($"{invoice.Total.Value} {invoice.Currency.Value}");

                            static IContainer CellStyle(IContainer container)
                            {
                                return container.PaddingVertical(5);
                            }
                        });

                        x.Item().AlignRight().Text($"TOTAL: {invoice.Total.Value} {invoice.Currency.Value}").FontSize(16).SemiBold();
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                    });
                });
            });

            return Task.FromResult(document.GeneratePdf());
        }
    }
}
