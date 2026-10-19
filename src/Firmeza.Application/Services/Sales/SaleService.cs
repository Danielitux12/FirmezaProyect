using Firmeza.Application.DTOs.SaleDetails;
using Firmeza.Application.DTOs.Sales;
using Firmeza.Domain.Entities;

namespace Firmeza.Application.Services.Sales;

public class SaleService : ISaleService
{
    private static readonly List<Sale> _sales = new();

    public Task<IEnumerable<SaleDto>> GetAllAsync()
    {
        var result = _sales
            .Where(s => s.IsActive)
            .Select(s => new SaleDto
            {
                Id = s.Id,
                Date = s.Date,
                Total = s.Total,
                ClientId = s.ClientId,
                ClientName = s.Client?.Name,
                SaleDetails = s.SaleDetails.Select(d => new SaleDetailDto
                {
                    Id = d.Id,
                    ProductId = d.ProductId,
                    ProductName = d.Product?.Name,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    Subtotal = d.Subtotal
                }).ToList()
            });

        return Task.FromResult(result);
    }

    public Task<SaleDto?> GetByIdAsync(Guid id)
    {
        var sale = _sales.FirstOrDefault(s => s.Id == id && s.IsActive);
        if (sale == null) return Task.FromResult<SaleDto?>(null);

        return Task.FromResult<SaleDto?>(new SaleDto
        {
            Id = sale.Id,
            Date = sale.Date,
            Total = sale.Total,
            ClientId = sale.ClientId,
            ClientName = sale.Client?.Name,
            SaleDetails = sale.SaleDetails.Select(d => new SaleDetailDto
            {
                Id = d.Id,
                ProductId = d.ProductId,
                ProductName = d.Product?.Name,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                Subtotal = d.Subtotal
            }).ToList()
        });
    }

    public Task<SaleDto> CreateAsync(CreateSaleDto dto)
    {
        var saleId = Guid.NewGuid();
        var details = dto.SaleDetails.Select(d => new SaleDetail
        {
            Id = Guid.NewGuid(),
            SaleId = saleId,
            ProductId = d.ProductId,
            Quantity = d.Quantity,
            UnitPrice = d.UnitPrice,
            Subtotal = d.Quantity * d.UnitPrice,
            FechaCreacion = DateTime.UtcNow,
            IsActive = true
        }).ToList();

        var sale = new Sale
        {
            Id = saleId,
            ClientId = dto.ClientId,
            Date = DateTime.UtcNow,
            Total = details.Sum(d => d.Subtotal),
            SaleDetails = details,
            FechaCreacion = DateTime.UtcNow,
            IsActive = true
        };

        _sales.Add(sale);

        return Task.FromResult(new SaleDto
        {
            Id = sale.Id,
            Date = sale.Date,
            Total = sale.Total,
            ClientId = sale.ClientId,
            SaleDetails = details.Select(d => new SaleDetailDto
            {
                Id = d.Id,
                ProductId = d.ProductId,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                Subtotal = d.Subtotal
            }).ToList()
        });
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        var sale = _sales.FirstOrDefault(s => s.Id == id && s.IsActive);
        if (sale == null) return Task.FromResult(false);

        sale.IsActive = false;
        return Task.FromResult(true);
    }
}
