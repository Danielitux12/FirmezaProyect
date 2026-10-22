using Firmeza.Application.DTOs.Excel;

namespace Firmeza.Application.Interfaces;

/// <summary>
/// Contrato para el procesamiento, normalización, validación y persistencia de datos desnormalizados provenientes de Excel.
/// </summary>
public interface IExcelImportService
{
    /// <summary>
    /// Procesa datos desnormalizados (clientes, productos, ventas) desde un flujo de archivo Excel.
    /// </summary>
    Task<ExcelImportResultDto> ImportDesnormalizedDataAsync(Stream fileStream);
}
