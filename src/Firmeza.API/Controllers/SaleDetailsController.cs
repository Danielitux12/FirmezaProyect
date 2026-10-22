using Firmeza.Application.DTOs.SaleDetails;
using Firmeza.Application.Services.SaleDetails;
using Firmeza.Application.Services.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SaleDetailsController : ControllerBase
{
    private readonly ISaleDetailService _saleDetailService;
    private readonly ISaleService _saleService;

    public SaleDetailsController(ISaleDetailService saleDetailService, ISaleService saleService)
    {
        _saleDetailService = saleDetailService;
        _saleService = saleService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Client")]
    public async Task<ActionResult<IEnumerable<SaleDetailDto>>> GetAll()
    {
        var details = await _saleDetailService.GetAllAsync();

        if (User.IsInRole("Client"))
        {
            var clientIdClaim = User.FindFirst("clientId")?.Value;
            if (string.IsNullOrEmpty(clientIdClaim) || !Guid.TryParse(clientIdClaim, out var clientId))
            {
                return Forbid();
            }

            var clientSales = (await _saleService.GetAllAsync()).Where(s => s.ClientId == clientId);
            var validDetailIds = clientSales.SelectMany(s => s.SaleDetails).Select(d => d.Id).ToHashSet();

            details = details.Where(d => validDetailIds.Contains(d.Id));
        }

        return Ok(details);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Client")]
    public async Task<ActionResult<SaleDetailDto>> GetById(Guid id)
    {
        var detail = await _saleDetailService.GetByIdAsync(id);
        if (detail == null)
        {
            return NotFound();
        }

        if (User.IsInRole("Client"))
        {
            var clientIdClaim = User.FindFirst("clientId")?.Value;
            if (string.IsNullOrEmpty(clientIdClaim) || !Guid.TryParse(clientIdClaim, out var clientId))
            {
                return Forbid();
            }

            var clientSales = (await _saleService.GetAllAsync()).Where(s => s.ClientId == clientId);
            var belongsToClient = clientSales.Any(s => s.SaleDetails.Any(d => d.Id == id));
            if (!belongsToClient)
            {
                return Forbid();
            }
        }

        return Ok(detail);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SaleDetailDto>> Create([FromBody] CreateSaleDetailDto dto)
    {
        var created = await _saleDetailService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _saleDetailService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
