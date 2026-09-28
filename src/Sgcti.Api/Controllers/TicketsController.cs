using Microsoft.AspNetCore.Mvc;
using Sgcti.Core.Abstractions;
using Sgcti.Core.Entities;

namespace Sgcti.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly ITicketRepositorio _ticketRepositorio;

    public TicketsController(ITicketRepositorio ticketRepositorio)
    {
        _ticketRepositorio = ticketRepositorio;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Ticket>>> GetAll(CancellationToken ct)
        => Ok(await _ticketRepositorio.ObtenerTodosAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Ticket>> GetById(int id, CancellationToken ct)
    {
        var ticket = await _ticketRepositorio.ObtenerPorIdAsync(id, ct);
        if (ticket is null)
        {
            return NotFound();
        }

        return Ok(ticket);
    }

    [HttpPost]
    public async Task<ActionResult<Ticket>> Create(Ticket ticket, CancellationToken ct)
    {
        var creado = await _ticketRepositorio.AgregarAsync(ticket, ct);
        return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Ticket ticket, CancellationToken ct)
    {
        if (id != ticket.Id)
        {
            return BadRequest("El identificador del cuerpo no coincide con el de la ruta.");
        }

        var existente = await _ticketRepositorio.ObtenerPorIdAsync(id, ct);
        if (existente is null)
        {
            return NotFound();
        }

        await _ticketRepositorio.ActualizarAsync(ticket, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var existente = await _ticketRepositorio.ObtenerPorIdAsync(id, ct);
        if (existente is null)
        {
            return NotFound();
        }

        await _ticketRepositorio.EliminarAsync(id, ct);
        return NoContent();
    }
}