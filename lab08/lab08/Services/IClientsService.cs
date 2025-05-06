using lab08.Models.DTOs;

namespace lab08.Services;

public interface IClientsService
{
    public Task<int> CreateClientAsync(CreateClientDto createClientDto);
    public Task RegisterClientForTrip(int id, int tripId);
    
    public Task DeleteClientFromTrip(int id, int tripId);
}