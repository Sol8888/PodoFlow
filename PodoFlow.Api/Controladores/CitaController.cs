
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.DTOs.Cita;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores;

[ApiController]
[Authorize]
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

    [HttpGet("{id:long}")]
    public async Task<IActionResult> ObtenerPorId(long id)
    {
        var cita = await _servicio.ObtenerPorIdAsync(id);

        return cita == null
            ? NotFound("No se encontró la cita.")
            : Ok(cita);
    }

    [HttpPost]
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
    public async Task<IActionResult> CambiarEstado(
        long id,
        [FromBody] CitaCambiarEstadoDto dto)
    {
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
