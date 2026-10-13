
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.DTOs.Permiso;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores;

[ApiController]
[Route("api/permisos")]
public class PermisoController : ControllerBase
{
    private readonly IPermisoServicio _servicio;

    public PermisoController(IPermisoServicio servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<ActionResult> ObtenerTodos(
        [FromQuery] bool incluirInactivos = false,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 10)
    {
        if (pagina < 1 || tamano < 1 || tamano > 100)
        {
            return BadRequest(
                "Los parámetros de paginación no son válidos.");
        }

        var permisos = await _servicio.ObtenerTodosAsync(
            incluirInactivos, pagina, tamano);

        return Ok(permisos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> ObtenerPorId(int id)
    {
        var permiso = await _servicio.ObtenerPorIdAsync(id);

        if (permiso == null)
            return NotFound("No se encontró el permiso.");

        return Ok(permiso);
    }

    [HttpPost]
    public async Task<ActionResult> Crear(
        [FromBody] PermisoCrearDto dto)
    {
        try
        {
            var permiso = await _servicio.CrearAsync(dto);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = permiso.Id },
                permiso);
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
                "No se pudo guardar el permiso. Verifica que el código no esté duplicado.");
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Actualizar(
        int id, [FromBody] PermisoActualizarDto dto)
    {
        try
        {
            var actualizado = await _servicio.ActualizarAsync(
                id, dto);

            if (!actualizado)
                return NotFound("No se encontró el permiso.");

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
            return Conflict("No se pudo actualizar el permiso.");
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Eliminar(int id)
    {
        var eliminado = await _servicio.EliminarAsync(id);

        if (!eliminado)
            return NotFound("No se encontró el permiso.");

        return NoContent();
    }

    [HttpPatch("{id:int}/reactivar")]
    public async Task<ActionResult> Reactivar(int id)
    {
        var reactivado = await _servicio.ReactivarAsync(id);

        if (!reactivado)
            return NotFound("No se encontró el permiso.");

        return NoContent();
    }
}
