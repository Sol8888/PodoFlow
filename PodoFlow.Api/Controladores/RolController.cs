
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.DTOs.Rol;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores;

[ApiController]
[Route("api/roles")]
public class RolController : ControllerBase
{
    private readonly IRolServicio _servicio;

    public RolController(IRolServicio servicio)
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
            return BadRequest("Los parámetros de paginación no son válidos.");
        }

        var roles = await _servicio.ObtenerTodosAsync(
            incluirInactivos, pagina, tamano);

        return Ok(roles);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> ObtenerPorId(int id)
    {
        var rol = await _servicio.ObtenerPorIdAsync(id);

        if (rol == null)
            return NotFound("No se encontró el rol.");

        return Ok(rol);
    }

    [HttpPost]
    public async Task<ActionResult> Crear(
        [FromBody] RolCrearDto dto)
    {
        try
        {
            var rol = await _servicio.CrearAsync(dto);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = rol.Id },
                rol);
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
                "No se pudo guardar el rol. Verifica que el código no esté duplicado.");
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Actualizar(
        int id, [FromBody] RolActualizarDto dto)
    {
        try
        {
            var actualizado = await _servicio.ActualizarAsync(id, dto);

            if (!actualizado)
                return NotFound("No se encontró el rol.");

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
            return Conflict("No se pudo actualizar el rol.");
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Eliminar(int id)
    {
        var eliminado = await _servicio.EliminarAsync(id);

        if (!eliminado)
            return NotFound("No se encontró el rol.");

        return NoContent();
    }

    [HttpPatch("{id:int}/reactivar")]
    public async Task<ActionResult> Reactivar(int id)
    {
        var reactivado = await _servicio.ReactivarAsync(id);

        if (!reactivado)
            return NotFound("No se encontró el rol.");

        return NoContent();
    }
}
