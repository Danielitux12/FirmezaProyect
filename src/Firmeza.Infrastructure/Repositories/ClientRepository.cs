using Firmeza.Domain.Entities;
using Firmeza.Domain.Interfaces;
using Firmeza.Infrastructure.Persistence;

namespace Firmeza.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Client entity.
/// </summary>
public class ClientRepository : BaseRepository<Client>, IClientRepository
{
    public ClientRepository(ApplicationDbContext context) : base(context)
    {
    }
}
