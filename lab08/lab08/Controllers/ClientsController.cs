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
    
    
}