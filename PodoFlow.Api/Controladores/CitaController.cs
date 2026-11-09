
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.DTOs.Cita;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores;

[ApiController]
[Authorize(Policy = "CitasVer")]
[Route("api/citas")]
public class CitaController : ControllerBase
{
    private readonly ICitaServicio _servicio;

    public CitaController(ICitaServicio servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos(
        [FromQuery] long? pacienteId = null,
        [FromQuery] long? usuarioId = null,
        [FromQuery] string? estado = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 10)
    {
        try
        {
            var citas = await _servicio.ObtenerTodosAsync(
                pacienteId, usuarioId, estado,
                pagina, tamano);

            return Ok(citas);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("disponibilidad")]
    public async Task<IActionResult> ObtenerDisponibilidad(
        [FromQuery] int servicioId,
        [FromQuery] DateOnly? fecha,
        [FromQuery] long? usuarioId = null)
    {
        if (fecha == null)
            return BadRequest(new { mensaje = "La fecha es obligatoria." });

        try
        {
            var huecos = await _servicio.ObtenerDisponibilidadAsync(
                servicioId, fecha.Value, usuarioId);

            return Ok(huecos);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> ObtenerPorId(long id)
    {
        var cita = await _servicio.ObtenerPorIdAsync(id);

        return cita == null
            ? NotFound("No se encontró la cita.")
            : Ok(cita);
    }

    [HttpPost]
    [Authorize(Policy = "CitasCrear")]
    public async Task<IActionResult> Crear(
        [FromBody] CitaCrearDto dto)
    {
        try
        {
            var cita = await _servicio.CrearAsync(dto);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = cita.Id },
                cita);
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
                "No se pudo crear la cita. Comprueba la disponibilidad.");
        }
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "CitasEditar")]
    public async Task<IActionResult> Actualizar(
        long id,
        [FromBody] CitaActualizarDto dto)
    {
        try
        {
            var actualizado = await _servicio.ActualizarAsync(id, dto);

            return actualizado
                ? NoContent()
                : NotFound("No se encontró la cita.");
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
                "La cita fue modificada. Consulta su versión actual.");
        }
        catch (DbUpdateException)
        {
            return Conflict("No se pudo actualizar la cita.");
        }
    }

    [HttpPatch("{id:long}/estado")]
    [Authorize(Policy = "CitasEditar")]
    public async Task<IActionResult> CambiarEstado(
        long id,
        [FromBody] CitaCambiarEstadoDto dto)
    {
        if (string.Equals(dto.NuevoEstado?.Trim(), "CANCELADA",
                StringComparison.OrdinalIgnoreCase) &&
            !User.HasClaim("permiso", "CITA_CANCELAR"))
            return Forbid();

        try
        {
            var actualizado = await _servicio.CambiarEstadoAsync(
                id, dto);

            return actualizado
                ? NoContent()
                : NotFound("No se encontró la cita.");
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
                "El estado fue modificado por otro proceso.");
        }
        catch (DbUpdateException)
        {
            return Conflict("No se pudo modificar el estado.");
        }
    }

    [HttpPatch("{id:long}/cancelar")]
    [Authorize(Policy = "CitasCancelar")]
    public async Task<IActionResult> Cancelar(
        long id,
        [FromBody] CitaCambiarEstadoDto dto)
    {
        try
        {
            dto.NuevoEstado = "CANCELADA";

            var cancelada = await _servicio.CambiarEstadoAsync(
                id, dto);

            return cancelada
                ? NoContent()
                : NotFound("No se encontró la cita.");
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
                "La cita fue modificada por otro proceso.");
        }
        catch (DbUpdateException)
        {
            return Conflict("No se pudo cancelar la cita.");
        }
    }
}
