using Firmeza.Domain.Entities;
using Firmeza.Domain.Interfaces;
using Firmeza.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for SaleDetail entity.
/// </summary>
public class SaleDetailRepository : BaseRepository<SaleDetail>, ISaleDetailRepository
{
    public SaleDetailRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<SaleDetail>> GetAllAsync()
    {
        return await _context.SaleDetails
            .Include(d => d.Product)
            .Where(d => d.IsActive)
            .ToListAsync();
    }

    public override async Task<SaleDetail?> GetByIdAsync(Guid id)
    {
        return await _context.SaleDetails
            .Include(d => d.Product)
            .FirstOrDefaultAsync(d => d.Id == id && d.IsActive);
    }
}
