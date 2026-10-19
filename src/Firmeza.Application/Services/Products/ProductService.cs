using Firmeza.Application.DTOs.Products;
using Firmeza.Domain.Entities;

namespace Firmeza.Application.Services.Products;

public class ProductService : IProductService
{
    private static readonly List<Product> _products = new();

    public Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        var result = _products
            .Where(p => p.IsActive)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                IsActive = p.IsActive
            });

        return Task.FromResult(result);
    }

    public Task<ProductDto?> GetByIdAsync(Guid id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id && p.IsActive);
        if (product == null) return Task.FromResult<ProductDto?>(null);

        return Task.FromResult<ProductDto?>(new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            IsActive = product.IsActive
        });
    }

    public Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Stock = dto.Stock,
            FechaCreacion = DateTime.UtcNow,
            IsActive = true
        };

        _products.Add(product);

        return Task.FromResult(new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            IsActive = product.IsActive
        });
    }

    public Task<bool> UpdateAsync(Guid id, CreateProductDto dto)
    {
        var product = _products.FirstOrDefault(p => p.Id == id && p.IsActive);
        if (product == null) return Task.FromResult(false);

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.Stock = dto.Stock;

        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id && p.IsActive);
        if (product == null) return Task.FromResult(false);

        product.IsActive = false;
        return Task.FromResult(true);
    }
}
