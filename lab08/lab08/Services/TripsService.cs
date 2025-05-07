using lab08.Exceptions;
using Microsoft.Data.SqlClient;
using lab08.Models.DTOs;

namespace lab08.Services;

public class TripsService : ITripsService
{
    private readonly string? _connectionString;

    public TripsService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default");
    }

    public async Task<List<TripDto>> GetTrips()
    {
        var trips = new Dictionary<int, TripDto>();

        string command = @"
        SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople,
               c.Name AS CountryName
        FROM Trip t
        JOIN Country_Trip ct ON t.IdTrip = ct.IdTrip
        JOIN Country c ON ct.IdCountry = c.IdCountry";

        await using (SqlConnection conn = new SqlConnection(_connectionString))
        await using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            await conn.OpenAsync();

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    int tripId = Convert.ToInt32(reader["IdTrip"]);
                    if (!trips.ContainsKey(tripId))
                    {
                        trips[tripId] = new TripDto
                        {
                            Id = tripId,
                            Name = reader.GetString(1),
                            Description = reader.GetString(2),
                            DateFrom = reader.GetDateTime(3),
                            DateTo = reader.GetDateTime(4),
                            MaxPeople = reader.GetInt32(5),
                            Countries = new List<CountryDto>()
                        };
                    }
                    trips[tripId].Countries.Add(new CountryDto
                    {
                        Name = reader.GetString(6),
                    });
                }
            }
        }
        return trips.Values.ToList();
    }

    public async Task<List<ClientTripDto>> GetTripsByClientId(int id)
    {
        string checkCommand = @"SELECT 1 FROM Client WHERE IdClient = @id";
        string command = @"SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople,
               ct.RegisteredAt, ct.PaymentDate,
               c.Name AS CountryName
        FROM Trip t
        JOIN Client_Trip ct ON t.IdTrip = ct.IdTrip
        JOIN Country_Trip ctr ON t.IdTrip = ctr.IdTrip
        JOIN Country c ON ctr.IdCountry = c.IdCountry
        WHERE ct.IdClient = @id";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using (var cmd = new SqlCommand(checkCommand, connection))
        {
            cmd.Parameters.AddWithValue("@id", id);
            var clientExists = await cmd.ExecuteScalarAsync();
            if (clientExists == null)
            {
                throw new NotFoundException("Client not found");
            }
        }

        await using (var cmd = new SqlCommand(command, connection))
        {
            cmd.Parameters.AddWithValue("@id", id);
            var clientTrips = new Dictionary<int, ClientTripDto>();

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    int tripId = reader.GetInt32(0);
                    if (!clientTrips.ContainsKey(tripId))
                    {
                        clientTrips[tripId] = new ClientTripDto
                        {
                            Id = tripId,
                            Name = reader.GetString(1),
                            Description = reader.GetString(2),
                            DateFrom = reader.GetDateTime(3),
                            DateTo = reader.GetDateTime(4),
                            MaxPeople = reader.GetInt32(5),
                            RegisteredAt = reader.GetInt32(6),
                            PaymentDate = await reader.IsDBNullAsync(7) ? null : reader.GetInt32(7),
                            Countries = new List<CountryDto>()
                        };
                    }
                    clientTrips[tripId].Countries.Add(new CountryDto
                    {
                        Name = reader.GetString(8),
                    });
                }
            }

            if (clientTrips.Values.Count == 0)
            {
                throw new NotFoundException("No trips found");
            }
            return clientTrips.Values.ToList();
        }
    }
}