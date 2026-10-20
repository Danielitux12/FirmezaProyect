using Firmeza.Application.DTOs.SaleDetails;
using Firmeza.Domain.Entities;
using Firmeza.Domain.Interfaces;

namespace Firmeza.Application.Services.SaleDetails;

public class SaleDetailService : ISaleDetailService
{
    private readonly ISaleDetailRepository _saleDetailRepository;

    public SaleDetailService(ISaleDetailRepository saleDetailRepository)
    {
        _saleDetailRepository = saleDetailRepository;
    }

    public async Task<IEnumerable<SaleDetailDto>> GetAllAsync()
    {
        var details = await _saleDetailRepository.GetAllAsync();
        return details.Select(d => new SaleDetailDto
        {
            Id = d.Id,
            ProductId = d.ProductId,
            ProductName = d.Product?.Name,
            Quantity = d.Quantity,
            UnitPrice = d.UnitPrice,
            Subtotal = d.Subtotal
        });
    }

    public async Task<SaleDetailDto?> GetByIdAsync(Guid id)
    {
        var detail = await _saleDetailRepository.GetByIdAsync(id);
        if (detail == null) return null;

        return new SaleDetailDto
        {
            Id = detail.Id,
            ProductId = detail.ProductId,
            ProductName = detail.Product?.Name,
            Quantity = detail.Quantity,
            UnitPrice = detail.UnitPrice,
            Subtotal = detail.Subtotal
        };
    }

    public async Task<SaleDetailDto> CreateAsync(CreateSaleDetailDto dto)
    {
        var detail = new SaleDetail
        {
            Id = Guid.NewGuid(),
            ProductId = dto.ProductId,
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice,
            Subtotal = dto.Quantity * dto.UnitPrice,
            FechaCreacion = DateTime.UtcNow,
            IsActive = true
        };

        var created = await _saleDetailRepository.AddAsync(detail);

        return new SaleDetailDto
        {
            Id = created.Id,
            ProductId = created.ProductId,
            Quantity = created.Quantity,
            UnitPrice = created.UnitPrice,
            Subtotal = created.Subtotal
        };
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _saleDetailRepository.DeleteAsync(id);
    }
}
