namespace Firmeza.Application.DTOs.Excel;

/// <summary>
/// Resultado detallado de la carga y procesamiento masivo de datos desnormalizados de Excel.
/// </summary>
public class ExcelImportResultDto
{
    public int TotalRowsRead { get; set; }
    public int TotalRowsProcessed { get; set; }
    public int ClientsCreated { get; set; }
    public int ClientsUpdated { get; set; }
    public int ProductsCreated { get; set; }
    public int ProductsUpdated { get; set; }
    public int SalesCreated { get; set; }
    public List<ExcelRowErrorDto> Errors { get; set; } = new();

    public bool HasErrors => Errors.Count > 0;
    public string Summary => $"Procesadas {TotalRowsProcessed} filas. Clientes: {ClientsCreated} creados / {ClientsUpdated} actualizados. Productos: {ProductsCreated} creados / {ProductsUpdated} actualizados. Ventas: {SalesCreated} creadas. Errores: {Errors.Count}.";
}

/// <summary>
/// Detalle de inconsistencia o error encontrado en una fila específica.
/// </summary>
public class ExcelRowErrorDto
{
    public int RowNumber { get; set; }
    public string SheetName { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
}
