
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.DTOs.Historia;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores;

[ApiController]
[Authorize(Policy = "HistoriaClinicaGestionar")]
[Route("api/historias")]
public class HistoriaController : ControllerBase
{
    private readonly IHistoriaServicio _servicio;

    public HistoriaController(IHistoriaServicio servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos(
        [FromQuery] string? estado = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 10)
    {
        try
        {
            return Ok(await _servicio.ObtenerTodosAsync(
                estado, pagina, tamano));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> ObtenerPorId(long id)
    {
        var historia = await _servicio.ObtenerPorIdAsync(id);

        return historia == null
            ? NotFound("No se encontró la historia clínica.")
            : Ok(historia);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(
        [FromBody] HistoriaCrearDto dto)
    {
        try
        {
            var historia = await _servicio.CrearAsync(dto);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = historia.Id },
                historia);
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
                "No se pudo registrar la historia. " +
                "Comprueba que el paciente no tenga otra historia.");
        }
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Actualizar(
        long id,
        [FromBody] HistoriaActualizarDto dto)
    {
        try
        {
            var actualizado =
                await _servicio.ActualizarAsync(id, dto);

            return actualizado ? NoContent() : NotFound();
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
                "La historia fue modificada por otra persona. " +
                "Consulta la versión actual.");
        }
        catch (DbUpdateException)
        {
            return Conflict(
                "No se pudo actualizar la historia clínica.");
        }
    }

    [HttpPatch("{id:long}/estado")]
    public async Task<IActionResult> CambiarEstado(
        long id,
        [FromBody] HistoriaCambiarEstadoDto dto)
    {
        try
        {
            var actualizado =
                await _servicio.CambiarEstadoAsync(id, dto);

            return actualizado ? NoContent() : NotFound();
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
                "La historia fue modificada por otra persona.");
        }
        catch (DbUpdateException)
        {
            return Conflict(
                "No se pudo cambiar el estado de la historia.");
        }
    }
}
