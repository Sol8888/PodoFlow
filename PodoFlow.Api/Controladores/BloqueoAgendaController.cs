
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.DTOs.BloqueoAgenda;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores;

[ApiController]
[Route("api/bloqueos-agenda")]
public class BloqueoAgendaController : ControllerBase
{
    private readonly IBloqueoAgendaServicio _servicio;

    public BloqueoAgendaController(
        IBloqueoAgendaServicio servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos(
        [FromQuery] bool incluirInactivos = false,
        [FromQuery] long? usuarioId = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 10)
    {
        if (pagina < 1 || tamano < 1 || tamano > 100 ||
            (usuarioId.HasValue && usuarioId <= 0))
            return BadRequest("Parámetros inválidos.");

        var bloqueos = await _servicio.ObtenerTodosAsync(
            incluirInactivos, usuarioId, pagina, tamano);

        return Ok(bloqueos);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> ObtenerPorId(long id)
    {
        var bloqueo = await _servicio.ObtenerPorIdAsync(id);

        if (bloqueo == null)
            return NotFound("No se encontró el bloqueo.");

        return Ok(bloqueo);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(
        [FromBody] BloqueoAgendaCrearDto dto)
    {
        try
        {
            var bloqueo = await _servicio.CrearAsync(dto);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = bloqueo.Id },
                bloqueo);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (DbUpdateException)
        {
            return Conflict("No se pudo crear el bloqueo.");
        }
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Actualizar(
        long id,
        [FromBody] BloqueoAgendaActualizarDto dto)
    {
        try
        {
            var actualizado = await _servicio.ActualizarAsync(
                id, dto);

            if (!actualizado)
                return NotFound("No se encontró el bloqueo.");

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (DbUpdateException)
        {
            return Conflict("No se pudo actualizar el bloqueo.");
        }
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Eliminar(long id)
    {
        try
        {
            var eliminado = await _servicio.EliminarAsync(id);

            if (!eliminado)
                return NotFound("No se encontró el bloqueo.");

            return NoContent();
        }
        catch (DbUpdateException)
        {
            return Conflict("No se pudo desactivar el bloqueo.");
        }
    }

    [HttpPatch("{id:long}/reactivar")]
    public async Task<IActionResult> Reactivar(long id)
    {
        try
        {
            var reactivado = await _servicio.ReactivarAsync(id);

            if (!reactivado)
                return NotFound("No se encontró el bloqueo.");

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (DbUpdateException)
        {
            return Conflict("No se pudo reactivar el bloqueo.");
        }
    }
}

