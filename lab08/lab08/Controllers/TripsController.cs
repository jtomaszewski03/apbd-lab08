using Microsoft.AspNetCore.Mvc;
using lab08.Services;

namespace lab08.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly ITripsService _tripsService;

        public TripsController(ITripsService tripsService)
        {
            _tripsService = tripsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTrips()
        {
            var trips = await _tripsService.GetTrips();
            Console.WriteLine(trips);
            return Ok(trips);
        }

        [HttpGet("clients/{id}/trips")]
        public async Task<IActionResult> GetTrip(int id)
        {
            var trips = await _tripsService.GetTripsByClientId(id);
            return Ok(trips);
        }
    }
}
