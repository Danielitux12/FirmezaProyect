using Firmeza.Application.DTOs.Sales;

namespace Firmeza.Application.Interfaces;

/// <summary>
/// Contrato para la generación de comprobantes de venta en formato PDF.
/// </summary>
public interface IPdfGeneratorService
{
    /// <summary>
    /// Genera el documento PDF del comprobante de venta, lo almacena en disco y retorna los bytes del contenido y la ruta física.
    /// </summary>
    Task<(string FilePath, byte[] Content)> GenerateReceiptPdfAsync(SaleDto sale);

    /// <summary>
    /// Genera un documento PDF con el catálogo completo de productos.
    /// </summary>
    Task<byte[]> GenerateProductsListPdfAsync(IEnumerable<Firmeza.Application.DTOs.Products.ProductDto> products);

    /// <summary>
    /// Genera un documento PDF con el listado de clientes registrados.
    /// </summary>
    Task<byte[]> GenerateClientsListPdfAsync(IEnumerable<Firmeza.Application.DTOs.Clients.ClientDto> clients);

    /// <summary>
    /// Genera un documento PDF con el reporte general de ventas y pedidos.
    /// </summary>
    Task<byte[]> GenerateSalesListPdfAsync(IEnumerable<SaleDto> sales);
}
