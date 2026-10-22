using Firmeza.Application.DTOs.Excel;
using Firmeza.Application.Interfaces;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;

namespace Firmeza.Infrastructure.Services;

/// <summary>
/// Implementación de lectura de hojas de cálculo .xlsx utilizando EPPlus.
/// </summary>
public class ExcelReaderService : IExcelReaderService
{
    private readonly ILogger<ExcelReaderService> _logger;

    static ExcelReaderService()
    {
        // Licencia no comercial de EPPlus
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public ExcelReaderService(ILogger<ExcelReaderService> logger)
    {
        _logger = logger;
    }

    public async Task<List<ExcelRowData>> ReadExcelRowsAsync(Stream excelStream)
    {
        var result = new List<ExcelRowData>();

        using var memoryStream = new MemoryStream();
        await excelStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        using var package = new ExcelPackage(memoryStream);

        if (package.Workbook.Worksheets.Count == 0)
        {
            _logger.LogWarning("El libro de Excel no contiene ninguna hoja.");
            return result;
        }

        foreach (var worksheet in package.Workbook.Worksheets)
        {
            if (worksheet.Dimension == null) continue;

            int startRow = worksheet.Dimension.Start.Row;
            int endRow = worksheet.Dimension.End.Row;
            int startCol = worksheet.Dimension.Start.Column;
            int endCol = worksheet.Dimension.End.Column;

            if (startRow >= endRow) continue; // No hay filas de datos además del encabezado

            // Mapear encabezados de columna (Fila 1)
            var columnHeaders = new Dictionary<int, string>();
            for (int col = startCol; col <= endCol; col++)
            {
                var headerVal = worksheet.Cells[startRow, col].Text?.Trim();
                if (!string.IsNullOrWhiteSpace(headerVal))
                {
                    var normalizedKey = ExcelRowData.NormalizeKey(headerVal);
                    columnHeaders[col] = normalizedKey;
                }
            }

            if (columnHeaders.Count == 0) continue;

            // Leer filas de datos (a partir de startRow + 1)
            for (int row = startRow + 1; row <= endRow; row++)
            {
                var rowData = new ExcelRowData
                {
                    RowIndex = row,
                    SheetName = worksheet.Name
                };

                bool hasAnyData = false;

                foreach (var (col, headerKey) in columnHeaders)
                {
                    var cellText = worksheet.Cells[row, col].Text?.Trim() ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(cellText))
                    {
                        hasAnyData = true;
                        rowData.Values[headerKey] = cellText;
                    }
                }

                if (hasAnyData)
                {
                    result.Add(rowData);
                }
            }
        }

        _logger.LogInformation("Lectura de Excel completada. Filas con datos leídas: {Count}", result.Count);
        return result;
    }
}
