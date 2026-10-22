using Firmeza.Application.DTOs.Excel;

namespace Firmeza.Application.Interfaces;

/// <summary>
/// Contrato de infraestructura para la lectura de libros y hojas Excel a estructuras tabulares genéricas.
/// </summary>
public interface IExcelReaderService
{
    /// <summary>
    /// Lee todas las filas con contenido del archivo Excel y normaliza los encabezados de columna.
    /// </summary>
    Task<List<ExcelRowData>> ReadExcelRowsAsync(Stream excelStream);
}
