
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.DTOs.IntentoAcceso;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores;

[ApiController]
[Authorize]
[Route("api/intentos-acceso")]
public class IntentoAccesoController : ControllerBase
{
    private readonly IIntentoAccesoServicio _servicio;

    public IntentoAccesoController(
        IIntentoAccesoServicio servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<ActionResult> ObtenerTodos(
        [FromQuery] long? usuarioId = null,
        [FromQuery] bool? exitoso = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 10)
    {
        if (pagina < 1 || tamano < 1 || tamano > 100)
        {
            return BadRequest(
                "Los parámetros de paginación no son válidos.");
        }

        if (usuarioId.HasValue && usuarioId <= 0)
            return BadRequest("El ID de usuario no es válido.");

        var intentos = await _servicio.ObtenerTodosAsync(
            usuarioId, exitoso, pagina, tamano);

        return Ok(intentos);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult> ObtenerPorId(long id)
    {
        var intento = await _servicio.ObtenerPorIdAsync(id);

        if (intento == null)
            return NotFound("No se encontró el intento de acceso.");

        return Ok(intento);
    }

    [HttpPost]
    public async Task<ActionResult> Crear(
        [FromBody] IntentoAccesoCrearDto dto)
    {
        try
        {
            var ip = HttpContext.Connection.RemoteIpAddress?
                .ToString();

            var agenteUsuario = Request.Headers.UserAgent
                .ToString();

            var intento = await _servicio.CrearAsync(
                dto, ip, agenteUsuario);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = intento.Id },
                intento);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (DbUpdateException)
        {
            return Conflict(
                "No se pudo registrar el intento de acceso.");
        }
    }
}
