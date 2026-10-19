using Firmeza.Application.DTOs.Sales;

namespace Firmeza.Application.Services.Sales;

public interface ISaleService
{
    Task<IEnumerable<SaleDto>> GetAllAsync();
    Task<SaleDto?> GetByIdAsync(Guid id);
    Task<SaleDto> CreateAsync(CreateSaleDto dto);
    Task<bool> DeleteAsync(Guid id);
}
