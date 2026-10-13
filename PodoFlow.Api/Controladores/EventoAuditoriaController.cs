
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores;

[ApiController]
[Authorize(Policy = "AuditoriaLeer")]
[Route("api/auditoria")]
public class EventoAuditoriaController : ControllerBase
{
    private readonly IEventoAuditoriaServicio _servicio;

    public EventoAuditoriaController(
        IEventoAuditoriaServicio servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos(
        [FromQuery] long? usuarioId = null,
        [FromQuery] long? pacienteId = null,
        [FromQuery] string? modulo = null,
        [FromQuery] string? resultado = null,
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? hasta = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 10)
    {
        try
        {
            var eventos = await _servicio.ObtenerTodosAsync(
                usuarioId,
                pacienteId,
                modulo,
                resultado,
                desde,
                hasta,
                pagina,
                tamano);

            return Ok(eventos);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> ObtenerPorId(long id)
    {
        var evento = await _servicio.ObtenerPorIdAsync(id);

        if (evento == null)
            return NotFound(
                "No se encontró el evento de auditoría.");

        return Ok(evento);
    }
}
