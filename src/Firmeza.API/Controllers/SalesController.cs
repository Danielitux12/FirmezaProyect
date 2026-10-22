using Firmeza.Application.DTOs.Sales;
using Firmeza.Application.Services.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SalesController : ControllerBase
{
    private readonly ISaleService _saleService;
    private readonly ILogger<SalesController> _logger;

    public SalesController(ISaleService saleService, ILogger<SalesController> logger)
    {
        _saleService = saleService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Client")]
    public async Task<ActionResult<IEnumerable<SaleDto>>> GetAll()
    {
        var sales = await _saleService.GetAllAsync();

        if (User.IsInRole("Client"))
        {
            var clientIdClaim = User.FindFirst("clientId")?.Value;
            if (string.IsNullOrEmpty(clientIdClaim) || !Guid.TryParse(clientIdClaim, out var clientId))
            {
                return Forbid();
            }
            sales = sales.Where(s => s.ClientId == clientId);
        }

        return Ok(sales);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Client")]
    public async Task<ActionResult<SaleDto>> GetById(Guid id)
    {
        var sale = await _saleService.GetByIdAsync(id);
        if (sale == null)
        {
            return NotFound();
        }

        if (User.IsInRole("Client"))
        {
            var clientIdClaim = User.FindFirst("clientId")?.Value;
            if (string.IsNullOrEmpty(clientIdClaim) || !Guid.TryParse(clientIdClaim, out var clientId) || sale.ClientId != clientId)
            {
                return Forbid();
            }
        }

        return Ok(sale);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Client")]
    public async Task<ActionResult<SaleDto>> Create([FromBody] CreateSaleDto dto)
    {
        if (User.IsInRole("Client"))
        {
            var clientIdClaim = User.FindFirst("clientId")?.Value;
            if (string.IsNullOrEmpty(clientIdClaim) || !Guid.TryParse(clientIdClaim, out var clientId))
            {
                return Forbid();
            }
            // Forzar el ClientId del usuario autenticado para evitar manipulación
            dto.ClientId = clientId;
        }

        try
        {
            var created = await _saleService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}/receipt")]
    [Authorize(Roles = "Admin,Client")]
    public async Task<IActionResult> DownloadReceipt(Guid id, [FromServices] Firmeza.Application.Interfaces.IPdfGeneratorService pdfGeneratorService)
    {
        var sale = await _saleService.GetByIdAsync(id);
        if (sale == null)
        {
            return NotFound(new { message = "Venta no encontrada." });
        }

        if (User.IsInRole("Client"))
        {
            var clientIdClaim = User.FindFirst("clientId")?.Value;
            if (string.IsNullOrEmpty(clientIdClaim) || !Guid.TryParse(clientIdClaim, out var clientId) || sale.ClientId != clientId)
            {
                return Forbid();
            }
        }

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "recibos", $"recibo_{sale.Id}.pdf");

        // 1. Si el archivo ya existe físicamente en wwwroot/recibos, leerlo y retornarlo
        if (System.IO.File.Exists(filePath))
        {
            try
            {
                var existingBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                return File(existingBytes, "application/pdf", $"recibo_{sale.Id}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo leer el archivo existente {FilePath}. Se intentará regenerar bajo demanda.", filePath);
            }
        }

        // 2. Si no existe físicamente (ventas anteriores o archivo eliminado), generarlo bajo demanda
        try
        {
            var result = await pdfGeneratorService.GenerateReceiptPdfAsync(sale);
            return File(result.Content, "application/pdf", $"recibo_{sale.Id}.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar comprobante bajo demanda para la venta {SaleId}", sale.Id);
            return NotFound(new { message = "Comprobante no disponible" });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _saleService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
