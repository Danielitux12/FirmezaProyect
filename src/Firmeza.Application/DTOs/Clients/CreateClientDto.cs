namespace Firmeza.Application.DTOs.Clients;

/// <summary>
/// DTO for creating/POST a new client.
/// </summary>
public class CreateClientDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña opcional para crear automáticamente el usuario de acceso al portal al crear el cliente.
    /// </summary>
    public string? Password { get; set; }
}
