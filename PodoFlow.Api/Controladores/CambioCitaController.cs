
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores;

[ApiController]
[Authorize(Policy = "AuditoriaLeer")]
[Route("api/cambios-cita")]
public class CambioCitaController : ControllerBase
{
    private readonly ICambioCitaServicio _servicio;

    public CambioCitaController(
        ICambioCitaServicio servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos(
        [FromQuery] long? citaId = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 10)
    {
        try
        {
            var cambios = await _servicio.ObtenerTodosAsync(
                citaId, pagina, tamano);

            return Ok(cambios);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> ObtenerPorId(long id)
    {
        var cambio = await _servicio.ObtenerPorIdAsync(id);

        if (cambio == null)
            return NotFound("No se encontró el cambio de cita.");

        return Ok(cambio);
    }
}
