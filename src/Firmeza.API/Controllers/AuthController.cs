using Firmeza.API.DTOs.Auth;
using Firmeza.API.Services;
using Firmeza.Domain.Entities;
using Firmeza.Domain.Interfaces;
using Firmeza.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IClientRepository _clientRepository;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IJwtTokenService jwtTokenService,
        IClientRepository clientRepository)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtTokenService = jwtTokenService;
        _clientRepository = clientRepository;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            return Unauthorized(new { message = "Credenciales incorrectas." });
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!isPasswordValid)
        {
            return Unauthorized(new { message = "Credenciales incorrectas." });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var (token, expiresAt) = _jwtTokenService.GenerateToken(user, roles);

        return Ok(new AuthResponseDto
        {
            Token = token,
            Email = user.Email ?? string.Empty,
            Role = roles.FirstOrDefault() ?? "Client",
            ClientId = user.ClientId,
            ExpiresAt = expiresAt
        });
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
    {
        // Validar rol (por defecto Client)
        var targetRole = string.IsNullOrWhiteSpace(dto.Role) ? "Client" : dto.Role.Trim();
        if (!string.Equals(targetRole, "Admin", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(targetRole, "Client", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "El rol debe ser 'Admin' o 'Client'." });
        }
        targetRole = string.Equals(targetRole, "Admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "Client";

        // Solo un Admin puede registrar a otro usuario con rol Admin (excepto en el arranque inicial si no hay usuarios)
        if (targetRole == "Admin")
        {
            var usersCount = await _userManager.Users.CountAsync();
            if (usersCount > 0)
            {
                if (!User.Identity?.IsAuthenticated == true || !User.IsInRole("Admin"))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new
                    {
                        message = "Solo un usuario con rol Admin puede registrar nuevos administradores."
                    });
                }
            }
        }

        // Verificar si el correo ya está registrado
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
        {
            return Conflict(new { message = "El correo ya se encuentra registrado." });
        }

        Guid? associatedClientId = null;

        // Si el rol es Client: vincular a cliente existente o auto-crear uno nuevo
        if (targetRole == "Client")
        {
            if (dto.ClientId.HasValue)
            {
                var clientExists = await _clientRepository.GetByIdAsync(dto.ClientId.Value);
                if (clientExists == null)
                {
                    return BadRequest(new { message = $"No se encontró ningún cliente con el ID {dto.ClientId.Value}." });
                }
                associatedClientId = dto.ClientId.Value;
            }
            else
            {
                // Auto-registro: crear automáticamente el registro de cliente en la base de datos
                var newClient = new Client
                {
                    Id = Guid.NewGuid(),
                    Name = string.IsNullOrWhiteSpace(dto.Name) ? dto.Email.Split('@')[0] : dto.Name.Trim(),
                    Email = dto.Email,
                    Phone = dto.Phone ?? string.Empty,
                    Address = dto.Address ?? string.Empty,
                    FechaCreacion = DateTime.UtcNow,
                    IsActive = true
                };
                await _clientRepository.AddAsync(newClient);
                associatedClientId = newClient.Id;
            }
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = dto.Email,
            Email = dto.Email,
            ClientId = associatedClientId
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                message = "Error al crear el usuario.",
                errors = result.Errors.Select(e => e.Description)
            });
        }

        if (!await _roleManager.RoleExistsAsync(targetRole))
        {
            await _roleManager.CreateAsync(new IdentityRole<Guid>(targetRole));
        }

        await _userManager.AddToRoleAsync(user, targetRole);

        var roles = await _userManager.GetRolesAsync(user);
        var (token, expiresAt) = _jwtTokenService.GenerateToken(user, roles);

        return CreatedAtAction(nameof(Login), new AuthResponseDto
        {
            Token = token,
            Email = user.Email ?? string.Empty,
            Role = targetRole,
            ClientId = user.ClientId,
            ExpiresAt = expiresAt
        });
    }
}
