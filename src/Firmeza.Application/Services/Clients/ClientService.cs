using Firmeza.Application.DTOs.Clients;
using Firmeza.Domain.Entities;
using Firmeza.Domain.Interfaces;

namespace Firmeza.Application.Services.Clients;

public class ClientService : IClientService
{
    private readonly IClientRepository _clientRepository;

    public ClientService(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public async Task<IEnumerable<ClientDto>> GetAllAsync()
    {
        var clients = await _clientRepository.GetAllAsync();
        return clients.Select(c => new ClientDto
        {
            Id = c.Id,
            Name = c.Name,
            Email = c.Email,
            Phone = c.Phone,
            Address = c.Address,
            IsActive = c.IsActive
        });
    }

    public async Task<ClientDto?> GetByIdAsync(Guid id)
    {
        var client = await _clientRepository.GetByIdAsync(id);
        if (client == null) return null;

        return new ClientDto
        {
            Id = client.Id,
            Name = client.Name,
            Email = client.Email,
            Phone = client.Phone,
            Address = client.Address,
            IsActive = client.IsActive
        };
    }

    public async Task<ClientDto> CreateAsync(CreateClientDto dto)
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

        var created = await _clientRepository.AddAsync(client);

        return new ClientDto
        {
            Id = created.Id,
            Name = created.Name,
            Email = created.Email,
            Phone = created.Phone,
            Address = created.Address,
            IsActive = created.IsActive
        };
    }

    public async Task<bool> UpdateAsync(Guid id, CreateClientDto dto)
    {
        var client = await _clientRepository.GetByIdAsync(id);
        if (client == null) return false;

        client.Name = dto.Name;
        client.Email = dto.Email;
        client.Phone = dto.Phone;
        client.Address = dto.Address;

        return await _clientRepository.UpdateAsync(client);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _clientRepository.DeleteAsync(id);
    }
}
