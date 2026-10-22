using Firmeza.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Firmeza.Infrastructure.Identity;

/// <summary>
/// Extended user entity for ASP.NET Core Identity with Guid keys.
/// Placed in Firmeza.Infrastructure to keep Firmeza.Domain free from framework-specific Microsoft Identity dependencies.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public Guid? ClientId { get; set; }
    public Client? Client { get; set; }
}
