using Firmeza.Application.DTOs.SaleDetails;

namespace Firmeza.Application.DTOs.Sales;

/// <summary>
/// DTO for reading/GET sale data.
/// </summary>
public class SaleDto
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public decimal Total { get; set; }
    public Guid ClientId { get; set; }
    public string? ClientName { get; set; }
    public List<SaleDetailDto> SaleDetails { get; set; } = new();
}
