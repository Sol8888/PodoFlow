
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.DTOs.Prestacion;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores;

[ApiController]
[Authorize(Policy = "FinanzasGestionar")]
[Route("api/prestaciones")]
public class PrestacionController : ControllerBase
{
    private readonly IPrestacionServicio _servicio;

    public PrestacionController(IPrestacionServicio servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos(
        [FromQuery] long? pacienteId = null,
        [FromQuery] string? estado = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 10)
    {
        try
        {
            var datos = await _servicio.ObtenerTodosAsync(
                pacienteId, estado, pagina, tamano);

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
        try
        {
            var prestacion =
                await _servicio.ObtenerPorIdAsync(id);

            return prestacion == null
                ? NotFound("No se encontró la prestación.")
                : Ok(prestacion);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Crear(
        [FromBody] PrestacionCrearDto dto)
    {
        try
        {
            var prestacion = await _servicio.CrearAsync(dto);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = prestacion.Id },
                prestacion);
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
                "No se pudo crear la prestación. " +
                "Revisa las relaciones y los importes.");
        }
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Actualizar(
        long id,
        [FromBody] PrestacionActualizarDto dto)
    {
        try
        {
            var actualizado =
                await _servicio.ActualizarAsync(id, dto);

            return actualizado
                ? NoContent()
                : NotFound("No se encontró la prestación.");
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
                "No se pudo actualizar la prestación.");
        }
    }

    [HttpPatch("{id:long}/estado")]
    public async Task<IActionResult> CambiarEstado(
        long id,
        [FromBody] PrestacionCambiarEstadoDto dto)
    {
        try
        {
            var actualizado =
                await _servicio.CambiarEstadoAsync(id, dto);

            return actualizado
                ? NoContent()
                : NotFound("No se encontró la prestación.");
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
                "No se pudo cambiar el estado de la prestación.");
        }
    }
}
