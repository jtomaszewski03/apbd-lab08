using lab08.Exceptions;
using lab08.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace lab08.Services;

public class ClientsService : IClientsService
{
    private readonly string _connectionString;

    public ClientsService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default");
    }

    public async Task<int> CreateClientAsync(CreateClientDto createClientDto)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        using (var cmd = new SqlCommand("SELECT 1 FROM Client WHERE Pesel = @Pesel", conn))
        {
            cmd.Parameters.AddWithValue("@Pesel", createClientDto.Pesel);
            var reader = await cmd.ExecuteScalarAsync();
            if (reader != null)
            {
                throw new InvalidOperationException($"Client {createClientDto.Pesel} already exists.");
            }
        }
        string command = @"INSERT INTO Client
            OUTPUT INSERTED.IdClient
            VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel)";

        using (var cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@FirstName", createClientDto.FirstName);
            cmd.Parameters.AddWithValue("@LastName", createClientDto.LastName);
            cmd.Parameters.AddWithValue("@Email", createClientDto.Email);
            cmd.Parameters.AddWithValue("@Telephone", createClientDto.Telephone);
            cmd.Parameters.AddWithValue("@Pesel", createClientDto.Pesel);
            
            var clientId = (int) await cmd.ExecuteScalarAsync();
            return clientId;
        }
    }

    public async Task RegisterClientForTrip(int id, int tripId)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using (var cmd = new SqlCommand("SELECT 1 FROM Client WHERE IdClient = @id", conn))
        {
            cmd.Parameters.AddWithValue("@id", id);
            var reader = await cmd.ExecuteScalarAsync();
            if (reader == null)
            {
                throw new NotFoundException("Client not found.");
            }
        }
        
        await using (var cmd = new SqlCommand("SELECT 1 FROM Trip WHERE IdTrip = @tripId", conn))
        {
            cmd.Parameters.AddWithValue("@tripId", tripId);
            var reader = await cmd.ExecuteScalarAsync();
            if (reader == null)
            {
                throw new NotFoundException("Trip not found.");
            }
        }

        await using (var cmd = new SqlCommand("SELECT 1 FROM Client_Trip WHERE IdClient = @id AND IdTrip = @tripId",
                         conn))
        {
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@tripId", tripId);
            var reader = await cmd.ExecuteScalarAsync();
            if (reader != null)
            {
                throw new InvalidOperationException($"Client {id} is already registered on trip {tripId}");
            }
        }

        int maxPeople = 0;
        await using (var cmd = new SqlCommand("SELECT MaxPeople FROM Trip WHERE IdTrip = @tripId", conn))
        {
            cmd.Parameters.AddWithValue("@tripId", tripId);
            var result = await cmd.ExecuteScalarAsync();
            if (result == null || result == DBNull.Value)
            {
                throw new NotFoundException($"Trip {tripId} does not exist.");
            }
            maxPeople = Convert.ToInt32(result);
        }

        await using (var cmd = new SqlCommand("SELECT Count(*) FROM Client_Trip WHERE IdTrip = @tripId", conn))
        {
            cmd.Parameters.AddWithValue("@tripId", tripId);
            int peopleCount = (int) await cmd.ExecuteScalarAsync();
            if (peopleCount >= maxPeople)
            {
                throw new InvalidOperationException($"Trip {tripId} has reached its maximum people limit.");
            }
        }
        int currentDate = int.Parse(DateTime.Now.ToString("yyyyMMdd"));
        int paymentDate = int.Parse(DateTime.Now.AddDays(10).ToString("yyyyMMdd"));
        string command = @"INSERT INTO Client_Trip VALUES (@idClient, @idTrip, @date, @paymentDate)";
        await using (var cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@idClient", id);
            cmd.Parameters.AddWithValue("@idTrip", tripId);
            cmd.Parameters.AddWithValue("@date", currentDate);
            cmd.Parameters.AddWithValue("@paymentDate", paymentDate);
            await cmd.ExecuteNonQueryAsync();
        }
        
    }

    public async Task DeleteClientFromTrip(int id, int tripId)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        await using (var cmd = new SqlCommand(
                         "SELECT 1 FROM Client_Trip WHERE IdClient = @id AND IdTrip = @idTrip", conn))
        {
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@idTrip", tripId);
            var reader = await cmd.ExecuteScalarAsync();
            if (reader == null)
            {
                throw new NotFoundException($"Client {id} is not registered on trip {tripId}.");
            }
        }

        await using (var cmd = new SqlCommand(
                         "DELETE FROM Client_Trip WHERE IdClient = @id AND IdTrip = @tripId", conn))
        {
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@tripId", tripId);
            await cmd.ExecuteNonQueryAsync();
        }
    }
    
}