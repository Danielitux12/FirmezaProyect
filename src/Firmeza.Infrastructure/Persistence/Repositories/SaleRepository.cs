using Firmeza.Domain.Entities;
using Firmeza.Domain.Interfaces;
using Firmeza.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Sale entity.
/// </summary>
public class SaleRepository : BaseRepository<Sale>, ISaleRepository
{
    public SaleRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<Sale>> GetAllAsync()
    {
        return await _context.Sales
            .Include(s => s.Client)
            .Include(s => s.SaleDetails)
                .ThenInclude(d => d.Product)
            .Where(s => s.IsActive)
            .ToListAsync();
    }

    public override async Task<Sale?> GetByIdAsync(Guid id)
    {
        return await _context.Sales
            .Include(s => s.Client)
            .Include(s => s.SaleDetails)
                .ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);
    }

    public override async Task<Sale> AddAsync(Sale entity)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await _context.Sales.AddAsync(entity);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return entity;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Sale> CreateSaleWithStockAsync(Sale sale, IEnumerable<Product> updatedProducts)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead);
        try
        {
            foreach (var product in updatedProducts)
            {
                _context.Products.Update(product);
            }

            await _context.Sales.AddAsync(sale);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return sale;
        }
        catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23514")
        {
            await transaction.RollbackAsync();
            throw new InvalidOperationException("No hay suficiente stock disponible para uno de los productos.");
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            throw new InvalidOperationException("El inventario fue modificado por otra compra concurrente. Por favor intenta de nuevo.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
