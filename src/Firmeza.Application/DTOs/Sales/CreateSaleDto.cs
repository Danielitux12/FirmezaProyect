using Firmeza.Application.DTOs.SaleDetails;

namespace Firmeza.Application.DTOs.Sales;

/// <summary>
/// DTO for creating/POST a new sale.
/// </summary>
public class CreateSaleDto
{
    public Guid ClientId { get; set; }
    public List<CreateSaleDetailDto> SaleDetails { get; set; } = new();
}
