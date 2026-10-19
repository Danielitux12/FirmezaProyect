namespace Firmeza.Domain.Entities;

/// <summary>
/// Base class containing common properties for all entities.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}
