using Firmeza.Application.DTOs.Excel;
using Firmeza.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.API.Controllers;

/// <summary>
/// Controlador para la carga y procesamiento masivo de datos mediante hojas de cálculo Excel.
/// Accesible exclusivamente para usuarios con rol 'Admin'.
/// </summary>
[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class ExcelImportController : ControllerBase
{
    private readonly IExcelImportService _excelImportService;
    private readonly ILogger<ExcelImportController> _logger;

    public ExcelImportController(
        IExcelImportService excelImportService,
        ILogger<ExcelImportController> logger)
    {
        _excelImportService = excelImportService;
        _logger = logger;
    }

    /// <summary>
    /// Carga masiva de datos desnormalizados (Clientes, Productos y/o Ventas) desde un archivo .xlsx.
    /// </summary>
    /// <param name="file">Archivo Excel (.xlsx) con datos desnormalizados.</param>
    /// <returns>Resumen del procesamiento con conteo de creados, actualizados y log de inconsistencias.</returns>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ExcelImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ExcelImportResultDto>> UploadExcel(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Debes seleccionar un archivo Excel válido para procesar." });
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".xlsx")
        {
            return BadRequest(new { message = "Formato no permitido. Solo se aceptan archivos con extensión .xlsx." });
        }

        // Límite de 15 MB
        if (file.Length > 15 * 1024 * 1024)
        {
            return BadRequest(new { message = "El archivo excede el tamaño máximo permitido de 15 MB." });
        }

        try
        {
            _logger.LogInformation("Iniciando procesamiento de carga masiva de Excel: {FileName} ({Size} bytes)", file.FileName, file.Length);

            using var stream = file.OpenReadStream();
            var result = await _excelImportService.ImportDesnormalizedDataAsync(stream);

            _logger.LogInformation("Procesamiento de Excel {FileName} finalizado: {Summary}", file.FileName, result.Summary);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado durante la importación del archivo Excel {FileName}", file.FileName);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "Ocurrió un error inesperado al procesar el archivo Excel.",
                detail = ex.Message
            });
        }
    }
}
