using Firmeza.Domain.Entities;
using Firmeza.Domain.Interfaces;
using Firmeza.Infrastructure.Persistence;

namespace Firmeza.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Product entity.
/// </summary>
public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }
}
