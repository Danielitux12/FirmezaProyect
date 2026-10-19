using Firmeza.Application.DTOs.Products;

namespace Firmeza.Application.Services.Products;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllAsync();
    Task<ProductDto?> GetByIdAsync(Guid id);
    Task<ProductDto> CreateAsync(CreateProductDto dto);
    Task<bool> UpdateAsync(Guid id, CreateProductDto dto);
    Task<bool> DeleteAsync(Guid id);
}
