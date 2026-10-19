using Firmeza.Application.DTOs.Clients;
using Firmeza.Domain.Entities;

namespace Firmeza.Application.Services.Clients;

public class ClientService : IClientService
{
    private static readonly List<Client> _clients = new();

    public Task<IEnumerable<ClientDto>> GetAllAsync()
    {
        var result = _clients
            .Where(c => c.IsActive)
            .Select(c => new ClientDto
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email,
                Phone = c.Phone,
                Address = c.Address,
                IsActive = c.IsActive
            });

        return Task.FromResult(result);
    }

    public Task<ClientDto?> GetByIdAsync(Guid id)
    {
        var client = _clients.FirstOrDefault(c => c.Id == id && c.IsActive);
        if (client == null) return Task.FromResult<ClientDto?>(null);

        return Task.FromResult<ClientDto?>(new ClientDto
        {
            Id = client.Id,
            Name = client.Name,
            Email = client.Email,
            Phone = client.Phone,
            Address = client.Address,
            IsActive = client.IsActive
        });
    }

    public Task<ClientDto> CreateAsync(CreateClientDto dto)
    {
        var client = new Client
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            FechaCreacion = DateTime.UtcNow,
            IsActive = true
        };

        _clients.Add(client);

        return Task.FromResult(new ClientDto
        {
            Id = client.Id,
            Name = client.Name,
            Email = client.Email,
            Phone = client.Phone,
            Address = client.Address,
            IsActive = client.IsActive
        });
    }

    public Task<bool> UpdateAsync(Guid id, CreateClientDto dto)
    {
        var client = _clients.FirstOrDefault(c => c.Id == id && c.IsActive);
        if (client == null) return Task.FromResult(false);

        client.Name = dto.Name;
        client.Email = dto.Email;
        client.Phone = dto.Phone;
        client.Address = dto.Address;

        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        var client = _clients.FirstOrDefault(c => c.Id == id && c.IsActive);
        if (client == null) return Task.FromResult(false);

        client.IsActive = false;
        return Task.FromResult(true);
    }
}
