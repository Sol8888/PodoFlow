
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.DTOs.Atencion;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores;

[ApiController]
[Authorize(Policy = "HistoriaClinicaGestionar")]
[Route("api/atenciones")]
public class AtencionController : ControllerBase
{
    private readonly IAtencionServicio _servicio;

    public AtencionController(IAtencionServicio servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos(
        [FromQuery] long? historiaId = null,
        [FromQuery] string? estado = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 10)
    {
        try
        {
            return Ok(await _servicio.ObtenerTodosAsync(
                historiaId, estado, pagina, tamano));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> ObtenerPorId(long id)
    {
        var atencion = await _servicio.ObtenerPorIdAsync(id);

        return atencion == null
            ? NotFound("No se encontró la atención.")
            : Ok(atencion);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(
        [FromBody] AtencionCrearDto dto)
    {
        try
        {
            var atencion = await _servicio.CrearAsync(dto);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = atencion.Id },
                atencion);
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
                "No se pudo crear la atención. Revisa las relaciones.");
        }
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Actualizar(
        long id, [FromBody] AtencionActualizarDto dto)
    {
        try
        {
            return await _servicio.ActualizarAsync(id, dto)
                ? NoContent() : NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(
                "La atención fue modificada. Consulta su versión actual.");
        }
        catch (DbUpdateException)
        {
            return Conflict("No se pudo actualizar la atención.");
        }
    }

    [HttpPatch("{id:long}/estado")]
    public async Task<IActionResult> CambiarEstado(
        long id, [FromBody] AtencionCambiarEstadoDto dto)
    {
        try
        {
            return await _servicio.CambiarEstadoAsync(id, dto)
                ? NoContent() : NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(
                "La atención fue modificada. Consulta la versión actual.");
        }
        catch (DbUpdateException)
        {
            return Conflict("No se pudo cambiar el estado.");
        }
    }
}

