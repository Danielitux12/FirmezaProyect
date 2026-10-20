using Firmeza.Application.DTOs.SaleDetails;
using Firmeza.Application.DTOs.Sales;
using Firmeza.Domain.Entities;
using Firmeza.Domain.Interfaces;

namespace Firmeza.Application.Services.Sales;

public class SaleService : ISaleService
{
    private readonly ISaleRepository _saleRepository;

    public SaleService(ISaleRepository saleRepository)
    {
        _saleRepository = saleRepository;
    }

    public async Task<IEnumerable<SaleDto>> GetAllAsync()
    {
        var sales = await _saleRepository.GetAllAsync();
        return sales.Select(s => new SaleDto
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
    }

    public async Task<SaleDto?> GetByIdAsync(Guid id)
    {
        var sale = await _saleRepository.GetByIdAsync(id);
        if (sale == null) return null;

        return new SaleDto
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
        };
    }

    public async Task<SaleDto> CreateAsync(CreateSaleDto dto)
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

        var created = await _saleRepository.AddAsync(sale);

        return new SaleDto
        {
            Id = created.Id,
            Date = created.Date,
            Total = created.Total,
            ClientId = created.ClientId,
            SaleDetails = created.SaleDetails.Select(d => new SaleDetailDto
            {
                Id = d.Id,
                ProductId = d.ProductId,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                Subtotal = d.Subtotal
            }).ToList()
        };
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _saleRepository.DeleteAsync(id);
    }
}
