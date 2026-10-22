using Firmeza.Application.DTOs.Clients;
using Firmeza.Application.DTOs.Products;
using Firmeza.Application.DTOs.Sales;

namespace Firmeza.Application.Interfaces;

/// <summary>
/// Contrato para la exportación de listados de entidades a formato Excel (.xlsx).
/// </summary>
public interface IExcelExportService
{
    /// <summary>
    /// Genera un libro Excel con el listado completo de productos.
    /// </summary>
    Task<byte[]> ExportProductsToExcelAsync(IEnumerable<ProductDto> products);

    /// <summary>
    /// Genera un libro Excel con el listado completo de clientes.
    /// </summary>
    Task<byte[]> ExportClientsToExcelAsync(IEnumerable<ClientDto> clients);

    /// <summary>
    /// Genera un libro Excel con el listado completo de ventas y sus líneas de detalle.
    /// </summary>
    Task<byte[]> ExportSalesToExcelAsync(IEnumerable<SaleDto> sales);
}
