namespace Firmeza.Domain.Entities;

/// <summary>
/// Represents a sales transaction header.
/// </summary>
public class Sale : BaseEntity
{
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public decimal Total { get; set; }

    // Relationship with Client
    public Guid ClientId { get; set; }
    public Client? Client { get; set; }

    // Items included in the sale
    public List<SaleDetail> SaleDetails { get; set; } = new();
}