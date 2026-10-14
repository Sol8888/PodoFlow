
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.DTOs.Insumo;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores;

[ApiController]
[Authorize(Policy = "HistoriaClinicaGestionar")]
[Route("api/insumos")]
public class InsumoController : ControllerBase
{
    private readonly IInsumoServicio _servicio;

    public InsumoController(IInsumoServicio servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos(
        [FromQuery] bool incluirInactivos = false,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 10)
    {
        try
        {
            var datos = await _servicio.ObtenerTodosAsync(
                incluirInactivos, pagina, tamano);

            return Ok(datos);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        try
        {
            var insumo = await _servicio.ObtenerPorIdAsync(id);

            return insumo == null
                ? NotFound("No se encontró el insumo.")
                : Ok(insumo);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Crear(
        [FromBody] InsumoCrearDto dto)
    {
        try
        {
            var insumo = await _servicio.CrearAsync(dto);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = insumo.Id },
                insumo);
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
                "El insumo cambió durante la operación. " +
                "Vuelve a consultarlo.");
        }
        catch (DbUpdateException)
        {
            return Conflict(
                "No se pudo registrar el insumo.");
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(
        int id,
        [FromBody] InsumoActualizarDto dto)
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
                "El insumo fue modificado por otra operación. " +
                "Obtén la nueva versión mediante GET.");
        }
        catch (DbUpdateException)
        {
            return Conflict(
                "No se pudo actualizar el insumo.");
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(
        int id,
        [FromQuery] string version)
    {
        try
        {
            var eliminado =
                await _servicio.EliminarAsync(id, version);

            return eliminado ? NoContent() : NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(
                "El insumo cambió. Consulta su versión actual.");
        }
        catch (DbUpdateException)
        {
            return Conflict(
                "No se pudo desactivar el insumo.");
        }
    }

    [HttpPatch("{id:int}/reactivar")]
    public async Task<IActionResult> Reactivar(
        int id,
        [FromQuery] string version)
    {
        try
        {
            var reactivado =
                await _servicio.ReactivarAsync(id, version);

            return reactivado ? NoContent() : NotFound();
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
                "El insumo cambió. Consulta su versión actual.");
        }
        catch (DbUpdateException)
        {
            return Conflict(
                "No se pudo reactivar el insumo.");
        }
    }
}
