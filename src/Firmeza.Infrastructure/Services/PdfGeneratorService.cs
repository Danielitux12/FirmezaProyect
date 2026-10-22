using System.Globalization;
using Firmeza.Application.DTOs.Clients;
using Firmeza.Application.DTOs.Products;
using Firmeza.Application.DTOs.Sales;
using Firmeza.Application.Interfaces;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Firmeza.Infrastructure.Services;

/// <summary>
/// Implementación de generación de comprobantes PDF usando QuestPDF con diseño profesional y cálculo de IVA colombiano (19%).
/// </summary>
public class PdfGeneratorService : IPdfGeneratorService
{
    private readonly ILogger<PdfGeneratorService> _logger;

    static PdfGeneratorService()
    {
        // Configuración de licencia comunitaria y compatibilidad de fuentes de QuestPDF
        QuestPDF.Settings.License = LicenseType.Community;
        QuestPDF.Settings.ThrowOnMissingFontFamilies = false;
        QuestPDF.Settings.UseSystemFonts = true;
    }

    public PdfGeneratorService(ILogger<PdfGeneratorService> logger)
    {
        _logger = logger;
    }

    public async Task<(string FilePath, byte[] Content)> GenerateReceiptPdfAsync(SaleDto sale)
    {
        var webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var receiptsDir = Path.Combine(webRoot, "recibos");

        if (!Directory.Exists(receiptsDir))
        {
            Directory.CreateDirectory(receiptsDir);
        }

        var fileName = $"recibo_{sale.Id}.pdf";
        var filePath = Path.Combine(receiptsDir, fileName);

        CultureInfo copCulture;
        try
        {
            copCulture = new CultureInfo("es-CO");
        }
        catch
        {
            copCulture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            copCulture.NumberFormat.CurrencySymbol = "$";
            copCulture.NumberFormat.CurrencyGroupSeparator = ".";
            copCulture.NumberFormat.CurrencyDecimalSeparator = ",";
        }

        // Cálculo de IVA colombiano del 19%
        var subtotalBruto = sale.SaleDetails.Sum(d => d.Subtotal);
        var iva19 = subtotalBruto * 0.19m;
        var totalConIva = subtotalBruto + iva19;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9.5f).FontFamily("Lato").FontColor(Colors.Grey.Darken3));

                // Encabezado
                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem(6).Column(c =>
                        {
                            c.Item().Text("FIRMEZA").FontSize(22).Bold().FontColor("#0f172a");
                            c.Item().Text("Materiales y Soluciones para la Construcción").FontSize(10).FontColor(Colors.Grey.Medium);
                            c.Item().Text("NIT: 900.123.456-1").FontSize(8.5f);
                            c.Item().Text("Dirección: Cra 15 # 80-20, Bogotá D.C., Colombia").FontSize(8.5f);
                            c.Item().Text("Email: contacto@firmeza.com | PBX: (601) 745-9000").FontSize(8.5f);
                        });

                        row.RelativeItem(4).Column(c =>
                        {
                            c.Item().Border(1).BorderColor("#0284c7").Background("#f0f9ff").Padding(8).Column(box =>
                            {
                                box.Item().Text("COMPROBANTE DE COMPRA").FontSize(11).Bold().FontColor("#0369a1").AlignCenter();
                                box.Item().Text($"No. {sale.Id.ToString().Substring(0, 8).ToUpper()}").FontSize(12).Bold().AlignCenter();
                                box.Item().Text($"Fecha: {sale.Date.ToLocalTime():yyyy-MM-dd HH:mm}").FontSize(8.5f).AlignCenter();
                                box.Item().Text("Régimen Común").FontSize(8).FontColor(Colors.Grey.Medium).AlignCenter();
                            });
                        });
                    });

                    col.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    // Información del Cliente
                    col.Item().Background("#f8fafc").Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Row(row =>
                    {
                        row.RelativeItem(6).Column(c =>
                        {
                            c.Item().Text("DATOS DEL CLIENTE").FontSize(9).Bold().FontColor("#334155");
                            c.Item().Text($"Nombre / Razón Social: {sale.ClientName ?? "Cliente Registrado"}").Bold();
                            c.Item().Text($"Correo Electrónico: {sale.ClientEmail ?? "No registrado"}");
                        });

                        row.RelativeItem(4).Column(c =>
                        {
                            c.Item().Text("DETALLES DE LA OPERACIÓN").FontSize(9).Bold().FontColor("#334155");
                            c.Item().Text($"ID Cliente: {sale.ClientId.ToString().Substring(0, 8)}...");
                            c.Item().Text("Forma de Pago: Contado / Transferencia");
                        });
                    });

                    col.Item().PaddingVertical(8);
                });

                // Contenido: Tabla de Ítems
                page.Content().Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(35); // #
                            columns.RelativeColumn(5);  // Producto
                            columns.ConstantColumn(60); // Cantidad
                            columns.RelativeColumn(2);  // Precio Unitario
                            columns.RelativeColumn(2);  // Subtotal
                        });

                        // Cabecera de la tabla
                        table.Header(header =>
                        {
                            header.Cell().Background("#0f172a").Padding(6).Text("#").FontColor(Colors.White).Bold().AlignCenter();
                            header.Cell().Background("#0f172a").Padding(6).Text("Descripción del Producto").FontColor(Colors.White).Bold();
                            header.Cell().Background("#0f172a").Padding(6).Text("Cant.").FontColor(Colors.White).Bold().AlignCenter();
                            header.Cell().Background("#0f172a").Padding(6).Text("Precio Unit.").FontColor(Colors.White).Bold().AlignRight();
                            header.Cell().Background("#0f172a").Padding(6).Text("Subtotal").FontColor(Colors.White).Bold().AlignRight();
                        });

                        // Filas de productos
                        int index = 1;
                        foreach (var item in sale.SaleDetails)
                        {
                            var bgColor = index % 2 == 0 ? "#f8fafc" : "#ffffff";

                            table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(index.ToString()).AlignCenter();
                            table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(item.ProductName ?? $"Producto {item.ProductId}").SemiBold();
                            table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(item.Quantity.ToString()).AlignCenter();
                            table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(item.UnitPrice.ToString("C0", copCulture)).AlignRight();
                            table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(item.Subtotal.ToString("C0", copCulture)).AlignRight();

                            index++;
                        }
                    });

                    // Bloque de Totales (Subtotal + IVA 19% + Total)
                    col.Item().PaddingTop(12).AlignRight().Width(260).Border(1).BorderColor(Colors.Grey.Lighten2).Background("#f8fafc").Padding(10).Column(totals =>
                    {
                        totals.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Subtotal Base:").FontSize(9.5f);
                            r.RelativeItem().Text(subtotalBruto.ToString("C0", copCulture)).FontSize(9.5f).AlignRight();
                        });

                        totals.Item().Row(r =>
                        {
                            r.RelativeItem().Text("IVA (19% Estándar):").FontSize(9.5f);
                            r.RelativeItem().Text(iva19.ToString("C0", copCulture)).FontSize(9.5f).AlignRight();
                        });

                        totals.Item().PaddingVertical(4).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

                        totals.Item().Row(r =>
                        {
                            r.RelativeItem().Text("TOTAL FINAL:").FontSize(11).Bold().FontColor("#0f172a");
                            r.RelativeItem().Text(totalConIva.ToString("C0", copCulture)).FontSize(12).Bold().FontColor("#059669").AlignRight();
                        });
                    });
                });

                // Pie de Página
                page.Footer().Column(col =>
                {
                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    col.Item().PaddingTop(4).Row(row =>
                    {
                        row.RelativeItem(8).Text("¡Gracias por su compra en Firmeza! Garantía de calidad en todos sus materiales.").FontSize(8).FontColor(Colors.Grey.Medium);
                        row.RelativeItem(4).Text(text =>
                        {
                            text.Span("Página ");
                            text.CurrentPageNumber();
                            text.Span(" de ");
                            text.TotalPages();
                        });
                    });
                });
            });
        });

        var pdfBytes = document.GeneratePdf();
        await File.WriteAllBytesAsync(filePath, pdfBytes);

        _logger.LogInformation("Comprobante PDF generado exitosamente en {FilePath}", filePath);
        return (filePath, pdfBytes);
    }

    public Task<byte[]> GenerateProductsListPdfAsync(IEnumerable<ProductDto> products)
    {
        var copCulture = GetSafeCulture();
        var list = products.ToList();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9f).FontFamily("Lato").FontColor(Colors.Grey.Darken3));

                page.Header().Column(col =>
                {
                    col.Item().Row(r =>
                    {
                        r.RelativeItem(7).Column(c =>
                        {
                            c.Item().Text("FIRMEZA").FontSize(20).Bold().FontColor("#0f172a");
                            c.Item().Text("Catálogo General de Productos y Materiales").FontSize(11).SemiBold().FontColor("#0284c7");
                            c.Item().Text($"Generado el: {DateTime.Now:yyyy-MM-dd HH:mm} | Total: {list.Count} productos").FontSize(8).FontColor(Colors.Grey.Medium);
                        });
                        r.RelativeItem(3).AlignRight().Text("NIT: 900.123.456-1\nBogotá D.C., Colombia").FontSize(8).FontColor(Colors.Grey.Medium);
                    });
                    col.Item().PaddingVertical(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(30);  // #
                        cols.RelativeColumn(3.5f); // Nombre
                        cols.RelativeColumn(4.5f); // Descripción
                        cols.RelativeColumn(2f);   // Precio
                        cols.ConstantColumn(50);  // Stock
                        cols.ConstantColumn(65);  // Estado
                    });

                    table.Header(h =>
                    {
                        h.Cell().Background("#0f172a").Padding(5).Text("#").FontColor(Colors.White).Bold().AlignCenter();
                        h.Cell().Background("#0f172a").Padding(5).Text("Producto").FontColor(Colors.White).Bold();
                        h.Cell().Background("#0f172a").Padding(5).Text("Descripción").FontColor(Colors.White).Bold();
                        h.Cell().Background("#0f172a").Padding(5).Text("Precio").FontColor(Colors.White).Bold().AlignRight();
                        h.Cell().Background("#0f172a").Padding(5).Text("Stock").FontColor(Colors.White).Bold().AlignCenter();
                        h.Cell().Background("#0f172a").Padding(5).Text("Estado").FontColor(Colors.White).Bold().AlignCenter();
                    });

                    int i = 1;
                    foreach (var p in list)
                    {
                        var bg = i % 2 == 0 ? "#f8fafc" : "#ffffff";
                        table.Cell().Background(bg).Padding(5).Text(i.ToString()).AlignCenter();
                        table.Cell().Background(bg).Padding(5).Text(p.Name).SemiBold();
                        table.Cell().Background(bg).Padding(5).Text(p.Description);
                        table.Cell().Background(bg).Padding(5).Text(p.Price.ToString("C0", copCulture)).AlignRight();
                        table.Cell().Background(bg).Padding(5).Text(p.Stock.ToString()).AlignCenter();
                        table.Cell().Background(bg).Padding(5).Text(p.IsActive ? "Activo" : "Inactivo").AlignCenter();
                        i++;
                    }
                });

                page.Footer().Row(r =>
                {
                    r.RelativeItem().Text("Firmeza Materiales - Reporte Confidencial").FontSize(8).FontColor(Colors.Grey.Medium);
                    r.RelativeItem().AlignRight().Text(x =>
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                        x.Span(" de ");
                        x.TotalPages();
                    });
                });
            });
        });

        return Task.FromResult(document.GeneratePdf());
    }

    public Task<byte[]> GenerateClientsListPdfAsync(IEnumerable<ClientDto> clients)
    {
        var list = clients.ToList();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9f).FontFamily("Lato").FontColor(Colors.Grey.Darken3));

                page.Header().Column(col =>
                {
                    col.Item().Row(r =>
                    {
                        r.RelativeItem(7).Column(c =>
                        {
                            c.Item().Text("FIRMEZA").FontSize(20).Bold().FontColor("#0f172a");
                            c.Item().Text("Directorio Oficial de Clientes y Contratistas").FontSize(11).SemiBold().FontColor("#0284c7");
                            c.Item().Text($"Generado el: {DateTime.Now:yyyy-MM-dd HH:mm} | Total: {list.Count} clientes").FontSize(8).FontColor(Colors.Grey.Medium);
                        });
                        r.RelativeItem(3).AlignRight().Text("NIT: 900.123.456-1\nBogotá D.C., Colombia").FontSize(8).FontColor(Colors.Grey.Medium);
                    });
                    col.Item().PaddingVertical(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(30);   // #
                        cols.RelativeColumn(3.5f);  // Nombre
                        cols.RelativeColumn(3.5f);  // Email
                        cols.RelativeColumn(2.5f);  // Teléfono
                        cols.RelativeColumn(3.5f);  // Dirección
                        cols.ConstantColumn(60);   // Estado
                    });

                    table.Header(h =>
                    {
                        h.Cell().Background("#0f172a").Padding(5).Text("#").FontColor(Colors.White).Bold().AlignCenter();
                        h.Cell().Background("#0f172a").Padding(5).Text("Cliente / Razón Social").FontColor(Colors.White).Bold();
                        h.Cell().Background("#0f172a").Padding(5).Text("Correo Electrónico").FontColor(Colors.White).Bold();
                        h.Cell().Background("#0f172a").Padding(5).Text("Teléfono").FontColor(Colors.White).Bold();
                        h.Cell().Background("#0f172a").Padding(5).Text("Dirección").FontColor(Colors.White).Bold();
                        h.Cell().Background("#0f172a").Padding(5).Text("Estado").FontColor(Colors.White).Bold().AlignCenter();
                    });

                    int i = 1;
                    foreach (var c in list)
                    {
                        var bg = i % 2 == 0 ? "#f8fafc" : "#ffffff";
                        table.Cell().Background(bg).Padding(5).Text(i.ToString()).AlignCenter();
                        table.Cell().Background(bg).Padding(5).Text(c.Name).SemiBold();
                        table.Cell().Background(bg).Padding(5).Text(c.Email);
                        table.Cell().Background(bg).Padding(5).Text(c.Phone);
                        table.Cell().Background(bg).Padding(5).Text(c.Address);
                        table.Cell().Background(bg).Padding(5).Text(c.IsActive ? "Activo" : "Inactivo").AlignCenter();
                        i++;
                    }
                });

                page.Footer().Row(r =>
                {
                    r.RelativeItem().Text("Firmeza Materiales - Reporte Administrativo").FontSize(8).FontColor(Colors.Grey.Medium);
                    r.RelativeItem().AlignRight().Text(x =>
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                        x.Span(" de ");
                        x.TotalPages();
                    });
                });
            });
        });

        return Task.FromResult(document.GeneratePdf());
    }

    public Task<byte[]> GenerateSalesListPdfAsync(IEnumerable<SaleDto> sales)
    {
        var copCulture = GetSafeCulture();
        var list = sales.ToList();
        var totalVentas = list.Sum(s => s.Total);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9f).FontFamily("Lato").FontColor(Colors.Grey.Darken3));

                page.Header().Column(col =>
                {
                    col.Item().Row(r =>
                    {
                        r.RelativeItem(7).Column(c =>
                        {
                            c.Item().Text("FIRMEZA").FontSize(20).Bold().FontColor("#0f172a");
                            c.Item().Text("Reporte Consolidado de Ventas").FontSize(11).SemiBold().FontColor("#0284c7");
                            c.Item().Text($"Generado el: {DateTime.Now:yyyy-MM-dd HH:mm} | Órdenes: {list.Count} | Total Facturado: {totalVentas.ToString("C0", copCulture)}").FontSize(8).FontColor(Colors.Grey.Medium);
                        });
                        r.RelativeItem(3).AlignRight().Text("NIT: 900.123.456-1\nBogotá D.C., Colombia").FontSize(8).FontColor(Colors.Grey.Medium);
                    });
                    col.Item().PaddingVertical(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(30);   // #
                        cols.ConstantColumn(80);   // No. Venta
                        cols.ConstantColumn(95);   // Fecha
                        cols.RelativeColumn(4f);   // Cliente
                        cols.ConstantColumn(60);   // Ítems
                        cols.RelativeColumn(2.5f); // Total
                    });

                    table.Header(h =>
                    {
                        h.Cell().Background("#0f172a").Padding(5).Text("#").FontColor(Colors.White).Bold().AlignCenter();
                        h.Cell().Background("#0f172a").Padding(5).Text("No. Orden").FontColor(Colors.White).Bold().AlignCenter();
                        h.Cell().Background("#0f172a").Padding(5).Text("Fecha").FontColor(Colors.White).Bold();
                        h.Cell().Background("#0f172a").Padding(5).Text("Cliente").FontColor(Colors.White).Bold();
                        h.Cell().Background("#0f172a").Padding(5).Text("Ítems").FontColor(Colors.White).Bold().AlignCenter();
                        h.Cell().Background("#0f172a").Padding(5).Text("Total").FontColor(Colors.White).Bold().AlignRight();
                    });

                    int i = 1;
                    foreach (var s in list)
                    {
                        var bg = i % 2 == 0 ? "#f8fafc" : "#ffffff";
                        table.Cell().Background(bg).Padding(5).Text(i.ToString()).AlignCenter();
                        table.Cell().Background(bg).Padding(5).Text($"#{s.Id.ToString()[..8].ToUpper()}").AlignCenter();
                        table.Cell().Background(bg).Padding(5).Text(s.Date.ToString("yyyy-MM-dd HH:mm"));
                        table.Cell().Background(bg).Padding(5).Text(s.ClientName ?? s.ClientId.ToString()).SemiBold();
                        table.Cell().Background(bg).Padding(5).Text(s.SaleDetails.Count.ToString()).AlignCenter();
                        table.Cell().Background(bg).Padding(5).Text(s.Total.ToString("C0", copCulture)).AlignRight().Bold();
                        i++;
                    }
                });

                page.Footer().Row(r =>
                {
                    r.RelativeItem().Text("Firmeza Materiales - Reporte Financiero").FontSize(8).FontColor(Colors.Grey.Medium);
                    r.RelativeItem().AlignRight().Text(x =>
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                        x.Span(" de ");
                        x.TotalPages();
                    });
                });
            });
        });

        return Task.FromResult(document.GeneratePdf());
    }

    private static CultureInfo GetSafeCulture()
    {
        try
        {
            return new CultureInfo("es-CO");
        }
        catch
        {
            var culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            culture.NumberFormat.CurrencySymbol = "$";
            culture.NumberFormat.CurrencyGroupSeparator = ".";
            culture.NumberFormat.CurrencyDecimalSeparator = ",";
            return culture;
        }
    }
}
