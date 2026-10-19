using Firmeza.Application.DTOs.SaleDetails;

namespace Firmeza.Application.Services.SaleDetails;

public interface ISaleDetailService
{
    Task<IEnumerable<SaleDetailDto>> GetAllAsync();
    Task<SaleDetailDto?> GetByIdAsync(Guid id);
    Task<SaleDetailDto> CreateAsync(CreateSaleDetailDto dto);
    Task<bool> DeleteAsync(Guid id);
}
