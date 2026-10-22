using System.ComponentModel.DataAnnotations;

namespace Firmeza.API.DTOs.Auth;

public class RegisterDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    public string Role { get; set; } = "Client"; // "Admin" o "Client"

    /// <summary>
    /// Nombre del cliente (usado en auto-registro si Role == "Client" y ClientId es null).
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Teléfono de contacto.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Dirección del cliente.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// ID del cliente al que se vincula el usuario (opcional; si no se provee, se crea un registro de cliente automáticamente).
    /// </summary>
    public Guid? ClientId { get; set; }
}
