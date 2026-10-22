using Firmeza.Domain.Entities;

namespace Firmeza.Domain.Interfaces;

/// <summary>
/// Repository interface for Sale entity.
/// </summary>
public interface ISaleRepository : IBaseRepository<Sale>
{
    /// <summary>
    /// Persiste la venta y actualiza el stock de los productos involucrados de forma atómica dentro de una única transacción.
    /// </summary>
    Task<Sale> CreateSaleWithStockAsync(Sale sale, IEnumerable<Product> updatedProducts);
}
