namespace Firmeza.Application.DTOs.SaleDetails;

/// <summary>
/// DTO for creating/POST an item line in a sale.
/// </summary>
public class CreateSaleDetailDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
