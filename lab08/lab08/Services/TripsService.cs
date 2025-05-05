using Microsoft.Data.SqlClient;
using lab08.Models.DTOs;

namespace lab08.Services;

public class TripsService : ITripsService
{
    private readonly string _connectionString = "Server=localhost\\SQLEXPRESS;Database=apbd_lab08;Integrated Security=True;TrustServerCertificate=True;";
    
    public async Task<List<TripDto>> GetTrips()
    {
        var trips = new Dictionary<int, TripDto>();

        string command = @"
        SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople,
               c.Name AS CountryName
        FROM Trip t
        LEFT JOIN Country_Trip ct ON t.IdTrip = ct.IdTrip
        LEFT JOIN Country c ON ct.IdCountry = c.IdCountry
        ORDER BY t.IdTrip";
        
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            await conn.OpenAsync();

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    int tripId = Convert.ToInt32(reader["IdTrip"]);
                    if (!trips.ContainsKey(tripId))
                    {
                        trips[tripId] = new TripDto
                        {
                            Id = tripId,
                            Name = reader["Name"].ToString(),
                            Description = reader["Description"].ToString(),
                            DateFrom = Convert.ToDateTime(reader["DateFrom"]),
                            DateTo = Convert.ToDateTime(reader["DateTo"]),
                            MaxPeople = Convert.ToInt32(reader["MaxPeople"]),
                            Countries = new List<CountryDto>()
                        };
                    }

                    if (!reader.IsDBNull(reader.GetOrdinal("CountryName")))
                    {
                        trips[tripId].Countries.Add(new CountryDto
                        {
                            Name = reader.GetString(reader.GetOrdinal("CountryName"))
                        });
                    }
                }
            }
        }
        return trips.Values.ToList();
    }
    
}