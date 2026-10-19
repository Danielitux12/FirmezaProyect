using Firmeza.Application.DTOs.Clients;

namespace Firmeza.Application.Services.Clients;

public interface IClientService
{
    Task<IEnumerable<ClientDto>> GetAllAsync();
    Task<ClientDto?> GetByIdAsync(Guid id);
    Task<ClientDto> CreateAsync(CreateClientDto dto);
    Task<bool> UpdateAsync(Guid id, CreateClientDto dto);
    Task<bool> DeleteAsync(Guid id);
}