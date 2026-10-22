namespace Firmeza.API.DTOs.Auth;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public Guid? ClientId { get; set; }
    public DateTime ExpiresAt { get; set; }
}
