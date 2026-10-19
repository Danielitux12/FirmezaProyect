using Firmeza.Application.DTOs.SaleDetails;
using Firmeza.Domain.Entities;

namespace Firmeza.Application.Services.SaleDetails;

public class SaleDetailService : ISaleDetailService
{
    private static readonly List<SaleDetail> _details = new();

    public Task<IEnumerable<SaleDetailDto>> GetAllAsync()
    {
        var result = _details
            .Where(d => d.IsActive)
            .Select(d => new SaleDetailDto
            {
                Id = d.Id,
                ProductId = d.ProductId,
                ProductName = d.Product?.Name,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                Subtotal = d.Subtotal
            });

        return Task.FromResult(result);
    }

    public Task<SaleDetailDto?> GetByIdAsync(Guid id)
    {
        var detail = _details.FirstOrDefault(d => d.Id == id && d.IsActive);
        if (detail == null) return Task.FromResult<SaleDetailDto?>(null);

        return Task.FromResult<SaleDetailDto?>(new SaleDetailDto
        {
            Id = detail.Id,
            ProductId = detail.ProductId,
            ProductName = detail.Product?.Name,
            Quantity = detail.Quantity,
            UnitPrice = detail.UnitPrice,
            Subtotal = detail.Subtotal
        });
    }

    public Task<SaleDetailDto> CreateAsync(CreateSaleDetailDto dto)
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

        _details.Add(detail);

        return Task.FromResult(new SaleDetailDto
        {
            Id = detail.Id,
            ProductId = detail.ProductId,
            Quantity = detail.Quantity,
            UnitPrice = detail.UnitPrice,
            Subtotal = detail.Subtotal
        });
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        var detail = _details.FirstOrDefault(d => d.Id == id && d.IsActive);
        if (detail == null) return Task.FromResult(false);

        detail.IsActive = false;
        return Task.FromResult(true);
    }
}
