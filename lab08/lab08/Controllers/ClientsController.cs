using lab08.Exceptions;
using lab08.Models.DTOs;
using lab08.Services;
using Microsoft.AspNetCore.Mvc;

namespace lab08.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClientsController : ControllerBase
{
    private readonly IClientsService _clientsService;

    public ClientsController(IClientsService clientsService)
    {
        _clientsService = clientsService;
    }
    
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateClientDto createClientDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            int newClientId = await _clientsService.CreateClientAsync(createClientDto);
            return CreatedAtAction(nameof(Post), new { id = newClientId }, new { IdClient = newClientId });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPut("{id}/trips/{tripId}")]
    public async Task<IActionResult> Put(int id, int tripId)
    {
        try
        {
            await _clientsService.RegisterClientForTrip(id, tripId);
            return CreatedAtAction(nameof(Put), new { id, tripId }, new { IdClient = id, TripId = tripId });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpDelete("{id}/trips/{tripId}")]
    public async Task<IActionResult> Delete(int id, int tripId)
    {
        try
        {
            await _clientsService.DeleteClientFromTrip(id, tripId);
            return Ok();
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    
    
}