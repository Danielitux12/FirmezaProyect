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
}
