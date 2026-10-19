namespace Firmeza.Application.DTOs.Products;

/// <summary>
/// DTO for creating/POST a new product.
/// </summary>
public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}
