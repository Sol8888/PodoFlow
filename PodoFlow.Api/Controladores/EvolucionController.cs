
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.DTOs.Evolucion;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores;

[ApiController]
[Authorize(Policy = "HistoriaClinicaGestionar")]
[Route("api/evoluciones")]
public class EvolucionController : ControllerBase
{
    private readonly IEvolucionServicio _servicio;

    public EvolucionController(IEvolucionServicio servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos(
        [FromQuery] long? atencionId = null,
        [FromQuery] bool incluirInactivos = false,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 10)
    {
        try
        {
            var datos = await _servicio.ObtenerTodosAsync(
                atencionId, incluirInactivos, pagina, tamano);

            return Ok(datos);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> ObtenerPorId(long id)
    {
        var evolucion = await _servicio.ObtenerPorIdAsync(id);

        if (evolucion == null)
            return NotFound("No se encontró la evolución.");

        return Ok(evolucion);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(
        [FromBody] EvolucionCrearDto dto)
    {
        try
        {
            var evolucion = await _servicio.CrearAsync(dto);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = evolucion.Id },
                evolucion);
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
            return Conflict(
                "No se pudo registrar la evolución.");
        }
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Actualizar(
        long id,
        [FromBody] EvolucionActualizarDto dto)
    {
        try
        {
            var actualizado = await _servicio.ActualizarAsync(
                id, dto);

            return actualizado
                ? NoContent()
                : NotFound();
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
            return Conflict(
                "No se pudo actualizar la evolución.");
        }
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Eliminar(long id)
    {
        try
        {
            var eliminado = await _servicio.EliminarAsync(id);

            return eliminado
                ? NoContent()
                : NotFound();
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
            return Conflict(
                "No se pudo desactivar la evolución.");
        }
    }

    [HttpPatch("{id:long}/reactivar")]
    public async Task<IActionResult> Reactivar(long id)
    {
        try
        {
            var reactivado = await _servicio.ReactivarAsync(id);

            return reactivado
                ? NoContent()
                : NotFound();
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
            return Conflict(
                "No se pudo reactivar la evolución.");
        }
    }
}

