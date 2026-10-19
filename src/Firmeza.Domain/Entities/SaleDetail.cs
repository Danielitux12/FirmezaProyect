namespace Firmeza.Domain.Entities;

/// <summary>
/// Represents an item line within a sale.
/// </summary>
public class SaleDetail : BaseEntity
{
    // Relationship with Sale
    public Guid SaleId { get; set; }
    public Sale? Sale { get; set; }

    // Relationship with Product
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
}