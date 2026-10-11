
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.DTOs.RolPermiso;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores;

[ApiController]
[Route("api/roles-permisos")]
public class RolPermisoController : ControllerBase
{
    private readonly IRolPermisoServicio _servicio;

    public RolPermisoController(IRolPermisoServicio servicio)
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
            return BadRequest("Paginación inválida.");

        var relaciones = await _servicio.ObtenerTodosAsync(
            incluirInactivos, pagina, tamano);

        return Ok(relaciones);
    }

    [HttpGet("{rolId:int}/{permisoId:int}")]
    public async Task<ActionResult> ObtenerPorId(
        int rolId, int permisoId)
    {
        var relacion = await _servicio.ObtenerPorIdAsync(
            rolId, permisoId);

        if (relacion == null)
            return NotFound("No se encontró la asignación.");

        return Ok(relacion);
    }

    [HttpPost]
    public async Task<ActionResult> Crear(
        [FromBody] RolPermisoCrearDto dto)
    {
        try
        {
            var relacion = await _servicio.CrearAsync(dto);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new
                {
                    rolId = relacion.RolId,
                    permisoId = relacion.PermisoId
                },
                relacion);
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
            return Conflict("No se pudo guardar la asignación.");
        }
    }

    [HttpPut("{rolId:int}/{permisoId:int}")]
    public async Task<ActionResult> Actualizar(
        int rolId,
        int permisoId,
        [FromBody] RolPermisoActualizarDto dto)
    {
        try
        {
            var actualizado = await _servicio.ActualizarAsync(
                rolId, permisoId, dto);

            if (!actualizado)
                return NotFound("No se encontró la asignación.");

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
            return Conflict("No se pudo modificar la asignación.");
        }
    }

    [HttpDelete("{rolId:int}/{permisoId:int}")]
    public async Task<ActionResult> Eliminar(
        int rolId, int permisoId)
    {
        var eliminado = await _servicio.EliminarAsync(
            rolId, permisoId);

        if (!eliminado)
            return NotFound("No se encontró la asignación.");

        return NoContent();
    }

    [HttpPatch("{rolId:int}/{permisoId:int}/reactivar")]
    public async Task<ActionResult> Reactivar(
        int rolId, int permisoId)
    {
        try
        {
            var reactivado = await _servicio.ReactivarAsync(
                rolId, permisoId);

            if (!reactivado)
                return NotFound("No se encontró la asignación.");

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (DbUpdateException)
        {
            return Conflict("No se pudo reactivar la asignación.");
        }
    }
}
