using Firmeza.Application.Interfaces;
using Firmeza.Application.Services.Clients;
using Firmeza.Application.Services.Products;
using Firmeza.Application.Services.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.API.Controllers;

/// <summary>
/// Controlador para la exportación de datos del sistema a formatos Excel (.xlsx) y PDF (.pdf).
/// Accesible exclusivamente para usuarios con rol 'Admin'.
/// </summary>
[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class ExportController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IClientService _clientService;
    private readonly ISaleService _saleService;
    private readonly IExcelExportService _excelExportService;
    private readonly IPdfGeneratorService _pdfGeneratorService;
    private readonly ILogger<ExportController> _logger;

    private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private const string PdfContentType = "application/pdf";

    public ExportController(
        IProductService productService,
        IClientService clientService,
        ISaleService saleService,
        IExcelExportService excelExportService,
        IPdfGeneratorService pdfGeneratorService,
        ILogger<ExportController> logger)
    {
        _productService = productService;
        _clientService = clientService;
        _saleService = saleService;
        _excelExportService = excelExportService;
        _pdfGeneratorService = pdfGeneratorService;
        _logger = logger;
    }

    #region Productos

    [HttpGet("products/excel")]
    [HttpGet("/api/Products/export/excel")]
    public async Task<IActionResult> ExportProductsExcel()
    {
        _logger.LogInformation("Iniciando exportación de productos a Excel.");
        var products = await _productService.GetAllAsync();
        var bytes = await _excelExportService.ExportProductsToExcelAsync(products);
        return File(bytes, ExcelContentType, $"productos_firmeza_{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }

    [HttpGet("products/pdf")]
    [HttpGet("/api/Products/export/pdf")]
    public async Task<IActionResult> ExportProductsPdf()
    {
        _logger.LogInformation("Iniciando exportación de catálogo de productos a PDF.");
        var products = await _productService.GetAllAsync();
        var bytes = await _pdfGeneratorService.GenerateProductsListPdfAsync(products);
        return File(bytes, PdfContentType, $"catalogo_productos_firmeza_{DateTime.UtcNow:yyyyMMdd}.pdf");
    }

    #endregion

    #region Clientes

    [HttpGet("clients/excel")]
    [HttpGet("/api/Clients/export/excel")]
    public async Task<IActionResult> ExportClientsExcel()
    {
        _logger.LogInformation("Iniciando exportación de clientes a Excel.");
        var clients = await _clientService.GetAllAsync();
        var bytes = await _excelExportService.ExportClientsToExcelAsync(clients);
        return File(bytes, ExcelContentType, $"clientes_firmeza_{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }

    [HttpGet("clients/pdf")]
    [HttpGet("/api/Clients/export/pdf")]
    public async Task<IActionResult> ExportClientsPdf()
    {
        _logger.LogInformation("Iniciando exportación de directorio de clientes a PDF.");
        var clients = await _clientService.GetAllAsync();
        var bytes = await _pdfGeneratorService.GenerateClientsListPdfAsync(clients);
        return File(bytes, PdfContentType, $"directorio_clientes_firmeza_{DateTime.UtcNow:yyyyMMdd}.pdf");
    }

    #endregion

    #region Ventas

    [HttpGet("sales/excel")]
    [HttpGet("/api/Sales/export/excel")]
    public async Task<IActionResult> ExportSalesExcel()
    {
        _logger.LogInformation("Iniciando exportación de ventas a Excel.");
        var sales = await _saleService.GetAllAsync();
        var bytes = await _excelExportService.ExportSalesToExcelAsync(sales);
        return File(bytes, ExcelContentType, $"ventas_firmeza_{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }

    [HttpGet("sales/pdf")]
    [HttpGet("/api/Sales/export/pdf")]
    public async Task<IActionResult> ExportSalesPdf()
    {
        _logger.LogInformation("Iniciando exportación de reporte de ventas a PDF.");
        var sales = await _saleService.GetAllAsync();
        var bytes = await _pdfGeneratorService.GenerateSalesListPdfAsync(sales);
        return File(bytes, PdfContentType, $"reporte_ventas_firmeza_{DateTime.UtcNow:yyyyMMdd}.pdf");
    }

    #endregion
}
