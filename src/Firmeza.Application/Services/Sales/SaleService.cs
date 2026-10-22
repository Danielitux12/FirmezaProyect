using Firmeza.Application.DTOs.SaleDetails;
using Firmeza.Application.DTOs.Sales;
using Firmeza.Application.Interfaces;
using Firmeza.Domain.Entities;
using Firmeza.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Firmeza.Application.Services.Sales;

public class SaleService : ISaleService
{
    private readonly ISaleRepository _saleRepository;
    private readonly IProductRepository _productRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IPdfGeneratorService _pdfGeneratorService;
    private readonly IEmailService _emailService;
    private readonly ILogger<SaleService> _logger;

    public SaleService(
        ISaleRepository saleRepository,
        IProductRepository productRepository,
        IClientRepository clientRepository,
        IPdfGeneratorService pdfGeneratorService,
        IEmailService emailService,
        ILogger<SaleService> logger)
    {
        _saleRepository = saleRepository;
        _productRepository = productRepository;
        _clientRepository = clientRepository;
        _pdfGeneratorService = pdfGeneratorService;
        _emailService = emailService;
        _logger = logger;
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
            ClientEmail = s.Client?.Email,
            ReceiptPdfUrl = $"/recibos/recibo_{s.Id}.pdf",
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
            ClientEmail = sale.Client?.Email,
            ReceiptPdfUrl = $"/recibos/recibo_{sale.Id}.pdf",
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
        if (dto.SaleDetails == null || !dto.SaleDetails.Any())
        {
            throw new InvalidOperationException("La venta debe contener al menos un producto.");
        }

        // Obtener cliente para asociar datos de contacto y comprobante
        var client = await _clientRepository.GetByIdAsync(dto.ClientId);

        // Agrupar cantidades por producto por si el cliente envió la misma línea varias veces
        var groupedRequests = dto.SaleDetails
            .GroupBy(d => d.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

        if (groupedRequests.Any(g => g.Value <= 0))
        {
            throw new InvalidOperationException("La cantidad de cada producto debe ser mayor a cero.");
        }

        var productsMap = new Dictionary<Guid, Product>();
        var validationErrors = new List<string>();

        // Validar existencia, estado activo y stock disponible de cada producto en la base de datos
        foreach (var (productId, requestedQty) in groupedRequests)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null || !product.IsActive)
            {
                validationErrors.Add($"El producto con ID '{productId}' no existe o no se encuentra activo.");
                continue;
            }

            if (product.Stock < requestedQty)
            {
                validationErrors.Add($"'{product.Name}': solo quedan {product.Stock} unidades disponibles (solicitaste {requestedQty}).");
            }

            productsMap[productId] = product;
        }

        if (validationErrors.Any())
        {
            throw new InvalidOperationException(string.Join(" | ", validationErrors));
        }

        var saleId = Guid.NewGuid();
        var details = new List<SaleDetail>();

        // Construir líneas de detalle usando EXCLUSIVAMENTE el precio real de la base de datos
        foreach (var d in dto.SaleDetails)
        {
            var product = productsMap[d.ProductId];
            var realUnitPrice = product.Price;
            var subtotal = d.Quantity * realUnitPrice;

            details.Add(new SaleDetail
            {
                Id = Guid.NewGuid(),
                SaleId = saleId,
                ProductId = product.Id,
                Quantity = d.Quantity,
                UnitPrice = realUnitPrice,
                Subtotal = subtotal,
                FechaCreacion = DateTime.UtcNow,
                IsActive = true
            });
        }

        // Descontar el stock de cada producto en memoria para la transacción
        foreach (var (productId, requestedQty) in groupedRequests)
        {
            var product = productsMap[productId];
            product.Stock -= requestedQty;
        }

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

        // Persistir venta y descuento de stock de forma atómica en una sola transacción
        var created = await _saleRepository.CreateSaleWithStockAsync(sale, productsMap.Values);

        var saleDto = new SaleDto
        {
            Id = created.Id,
            Date = created.Date,
            Total = created.Total,
            ClientId = created.ClientId,
            ClientName = client?.Name,
            ClientEmail = client?.Email,
            ReceiptPdfUrl = $"/recibos/recibo_{created.Id}.pdf",
            SaleDetails = created.SaleDetails.Select(d => new SaleDetailDto
            {
                Id = d.Id,
                ProductId = d.ProductId,
                ProductName = productsMap.TryGetValue(d.ProductId, out var p) ? p.Name : null,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                Subtotal = d.Subtotal
            }).ToList()
        };

        // Generar comprobante PDF automáticamente
        byte[]? pdfBytes = null;
        try
        {
            var pdfResult = await _pdfGeneratorService.GenerateReceiptPdfAsync(saleDto);
            pdfBytes = pdfResult.Content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar comprobante PDF para la venta {SaleId}. La venta fue registrada.", created.Id);
        }

        // Enviar comprobante por correo electrónico vía SMTP de Gmail
        if (pdfBytes != null && !string.IsNullOrWhiteSpace(saleDto.ClientEmail))
        {
            try
            {
                await _emailService.SendSaleReceiptAsync(
                    saleDto.ClientEmail,
                    saleDto.ClientName ?? "Cliente",
                    created.Id,
                    saleDto.Total,
                    pdfBytes,
                    $"recibo_{created.Id}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar comprobante por correo para la venta {SaleId}. La venta fue registrada con éxito.", created.Id);
            }
        }

        return saleDto;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _saleRepository.DeleteAsync(id);
    }
}

