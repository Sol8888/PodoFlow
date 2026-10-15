
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.DTOs.Cobro;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores;

[ApiController]
[Authorize(Policy = "FinanzasGestionar")]
[Route("api/cobros")]
public class CobroController : ControllerBase
{
    private readonly ICobroServicio _servicio;

    public CobroController(ICobroServicio servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos(
        [FromQuery] long? prestacionId = null,
        [FromQuery] string? estado = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 10)
    {
        try
        {
            var datos = await _servicio.ObtenerTodosAsync(
                prestacionId,
                estado,
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
            var cobro = await _servicio.ObtenerPorIdAsync(id);

            return cobro == null
                ? NotFound("No se encontró el cobro.")
                : Ok(cobro);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Crear(
        [FromBody] CobroCrearDto dto)
    {
        try
        {
            var cobro = await _servicio.CrearAsync(dto);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = cobro.Id },
                cobro);
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
                "No se pudo registrar el cobro.");
        }
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Actualizar(
        long id,
        [FromBody] CobroActualizarDto dto)
    {
        try
        {
            var actualizado =
                await _servicio.ActualizarAsync(id, dto);

            return actualizado
                ? NoContent()
                : NotFound("No se encontró el cobro.");
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
                "No se pudo actualizar el cobro.");
        }
    }

    [HttpPatch("{id:long}/estado")]
    public async Task<IActionResult> CambiarEstado(
        long id,
        [FromBody] CobroCambiarEstadoDto dto)
    {
        try
        {
            var actualizado =
                await _servicio.CambiarEstadoAsync(id, dto);

            return actualizado
                ? NoContent()
                : NotFound("No se encontró el cobro.");
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
                "No se pudo anular el cobro.");
        }
    }
}
