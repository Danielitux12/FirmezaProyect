using System.Drawing;
using Firmeza.Application.DTOs.Clients;
using Firmeza.Application.DTOs.Products;
using Firmeza.Application.DTOs.Sales;
using Firmeza.Application.Interfaces;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Firmeza.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de exportación a Excel utilizando EPPlus.
/// </summary>
public class ExcelExportService : IExcelExportService
{
    private readonly ILogger<ExcelExportService> _logger;

    static ExcelExportService()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public ExcelExportService(ILogger<ExcelExportService> logger)
    {
        _logger = logger;
    }

    public async Task<byte[]> ExportProductsToExcelAsync(IEnumerable<ProductDto> products)
    {
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Productos");

        string[] headers = { "#", "ID", "Nombre", "Descripción", "Precio (COP)", "Stock", "Estado" };
        ApplyHeaderStyles(ws, headers);

        int row = 2;
        int index = 1;
        foreach (var p in products)
        {
            ws.Cells[row, 1].Value = index++;
            ws.Cells[row, 2].Value = p.Id.ToString();
            ws.Cells[row, 3].Value = p.Name;
            ws.Cells[row, 4].Value = p.Description;
            ws.Cells[row, 5].Value = p.Price;
            ws.Cells[row, 5].Style.Numberformat.Format = "$#,##0.00";
            ws.Cells[row, 6].Value = p.Stock;
            ws.Cells[row, 6].Style.Numberformat.Format = "#,##0";
            ws.Cells[row, 7].Value = p.IsActive ? "Activo" : "Inactivo";

            ApplyRowZebra(ws, row, headers.Length);
            row++;
        }

        ws.Cells.AutoFitColumns();
        _logger.LogInformation("Exportados {Count} productos a Excel.", row - 2);
        return await package.GetAsByteArrayAsync();
    }

    public async Task<byte[]> ExportClientsToExcelAsync(IEnumerable<ClientDto> clients)
    {
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Clientes");

        string[] headers = { "#", "ID", "Nombre / Razón Social", "Email", "Teléfono", "Dirección", "Estado" };
        ApplyHeaderStyles(ws, headers);

        int row = 2;
        int index = 1;
        foreach (var c in clients)
        {
            ws.Cells[row, 1].Value = index++;
            ws.Cells[row, 2].Value = c.Id.ToString();
            ws.Cells[row, 3].Value = c.Name;
            ws.Cells[row, 4].Value = c.Email;
            ws.Cells[row, 5].Value = c.Phone;
            ws.Cells[row, 6].Value = c.Address;
            ws.Cells[row, 7].Value = c.IsActive ? "Activo" : "Inactivo";

            ApplyRowZebra(ws, row, headers.Length);
            row++;
        }

        ws.Cells.AutoFitColumns();
        _logger.LogInformation("Exportados {Count} clientes a Excel.", row - 2);
        return await package.GetAsByteArrayAsync();
    }

    public async Task<byte[]> ExportSalesToExcelAsync(IEnumerable<SaleDto> sales)
    {
        using var package = new ExcelPackage();

        // Hoja 1: Resumen de Ventas
        var wsSales = package.Workbook.Worksheets.Add("Ventas");
        string[] salesHeaders = { "#", "ID Venta", "Fecha", "Cliente ID", "Cliente", "Total (COP)", "Cantidad Líneas" };
        ApplyHeaderStyles(wsSales, salesHeaders);

        // Hoja 2: Detalle de Ítems Vendidos
        var wsDetails = package.Workbook.Worksheets.Add("Detalle_Ventas");
        string[] detailHeaders = { "ID Venta", "Fecha", "Cliente", "ID Producto", "Producto", "Cantidad", "Precio Unitario", "Subtotal" };
        ApplyHeaderStyles(wsDetails, detailHeaders);

        int saleRow = 2;
        int detailRow = 2;
        int index = 1;

        foreach (var s in sales)
        {
            wsSales.Cells[saleRow, 1].Value = index++;
            wsSales.Cells[saleRow, 2].Value = s.Id.ToString();
            wsSales.Cells[saleRow, 3].Value = s.Date.ToString("yyyy-MM-dd HH:mm");
            wsSales.Cells[saleRow, 4].Value = s.ClientId.ToString();
            wsSales.Cells[saleRow, 5].Value = s.ClientName ?? "N/A";
            wsSales.Cells[saleRow, 6].Value = s.Total;
            wsSales.Cells[saleRow, 6].Style.Numberformat.Format = "$#,##0.00";
            wsSales.Cells[saleRow, 7].Value = s.SaleDetails.Count;

            ApplyRowZebra(wsSales, saleRow, salesHeaders.Length);
            saleRow++;

            foreach (var d in s.SaleDetails)
            {
                wsDetails.Cells[detailRow, 1].Value = s.Id.ToString();
                wsDetails.Cells[detailRow, 2].Value = s.Date.ToString("yyyy-MM-dd HH:mm");
                wsDetails.Cells[detailRow, 3].Value = s.ClientName ?? "N/A";
                wsDetails.Cells[detailRow, 4].Value = d.ProductId.ToString();
                wsDetails.Cells[detailRow, 5].Value = d.ProductName ?? "N/A";
                wsDetails.Cells[detailRow, 6].Value = d.Quantity;
                wsDetails.Cells[detailRow, 7].Value = d.UnitPrice;
                wsDetails.Cells[detailRow, 7].Style.Numberformat.Format = "$#,##0.00";
                wsDetails.Cells[detailRow, 8].Value = d.Subtotal;
                wsDetails.Cells[detailRow, 8].Style.Numberformat.Format = "$#,##0.00";

                ApplyRowZebra(wsDetails, detailRow, detailHeaders.Length);
                detailRow++;
            }
        }

        wsSales.Cells.AutoFitColumns();
        wsDetails.Cells.AutoFitColumns();

        _logger.LogInformation("Exportadas {SalesCount} ventas con {DetailsCount} líneas a Excel.", saleRow - 2, detailRow - 2);
        return await package.GetAsByteArrayAsync();
    }

    private static void ApplyHeaderStyles(ExcelWorksheet ws, string[] headers)
    {
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cells[1, i + 1];
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(15, 23, 42)); // Slate 900
            cell.Style.Font.Color.SetColor(Color.White);
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        }
        ws.Row(1).Height = 26;
    }

    private static void ApplyRowZebra(ExcelWorksheet ws, int row, int colCount)
    {
        if (row % 2 == 0)
        {
            for (int col = 1; col <= colCount; col++)
            {
                ws.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(248, 250, 252));
            }
        }
    }
}
