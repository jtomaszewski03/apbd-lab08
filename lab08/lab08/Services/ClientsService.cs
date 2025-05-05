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
    
}