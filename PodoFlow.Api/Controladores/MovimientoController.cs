
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.DTOs.Movimiento;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores;

[ApiController]
[Authorize(Policy = "InventarioGestionar")]
[Route("api/movimientos")]
public class MovimientoController : ControllerBase
{
    private readonly IMovimientoServicio _servicio;

    public MovimientoController(IMovimientoServicio servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos(
        [FromQuery] int? insumoId = null,
        [FromQuery] long? atencionId = null,
        [FromQuery] string? tipo = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 10)
    {
        try
        {
            var datos = await _servicio.ObtenerTodosAsync(
                insumoId,
                atencionId,
                tipo,
                pagina,
                tamano);

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
            var movimiento =
                await _servicio.ObtenerPorIdAsync(id);

            return movimiento == null
                ? NotFound("No se encontró el movimiento.")
                : Ok(movimiento);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Crear(
        [FromBody] MovimientoCrearDto dto)
    {
        try
        {
            var movimiento = await _servicio.CrearAsync(dto);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = movimiento.Id },
                movimiento);
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
                "El inventario cambió durante la operación.");
        }
        catch (DbUpdateException)
        {
            return Conflict(
                "No se pudo registrar el movimiento.");
        }
    }
}
