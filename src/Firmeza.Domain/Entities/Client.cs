namespace Firmeza.Domain.Entities;

/// <summary>
/// Represents a customer/client entity.
/// </summary>
public class Client : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}