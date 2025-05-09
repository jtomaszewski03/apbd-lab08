﻿using lab08.Models.DTOs;

namespace lab08.Services;

public interface ITripsService
{
    Task<List<TripDto>> GetTrips();
    Task<List<ClientTripDto>> GetTripsByClientId(int id);
}