
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.DTOs.UsuarioRol;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores;

[ApiController]
[Route("api/usuarios-roles")]
public class UsuarioRolController : ControllerBase
{
    private readonly IUsuarioRolServicio _servicio;

    public UsuarioRolController(IUsuarioRolServicio servicio)
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

        var relaciones = await _servicio.ObtenerTodosAsync(
            incluirInactivos, pagina, tamano);

        return Ok(relaciones);
    }

    [HttpGet("{usuarioId:long}/{rolId:int}")]
    public async Task<ActionResult> ObtenerPorId(
        long usuarioId, int rolId)
    {
        var relacion = await _servicio.ObtenerPorIdAsync(
            usuarioId, rolId);

        if (relacion == null)
            return NotFound("No se encontró la asignación.");

        return Ok(relacion);
    }

    [HttpPost]
    public async Task<ActionResult> Crear(
        [FromBody] UsuarioRolCrearDto dto)
    {
        try
        {
            var relacion = await _servicio.CrearAsync(dto);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new
                {
                    usuarioId = relacion.UsuarioId,
                    rolId = relacion.RolId
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
            return Conflict(
                "No se pudo guardar la asignación.");
        }
    }

    [HttpPut("{usuarioId:long}/{rolId:int}")]
    public async Task<ActionResult> Actualizar(
        long usuarioId,
        int rolId,
        [FromBody] UsuarioRolActualizarDto dto)
    {
        try
        {
            var actualizado = await _servicio.ActualizarAsync(
                usuarioId, rolId, dto);

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
            return Conflict(
                "No se pudo modificar la asignación.");
        }
    }

    [HttpDelete("{usuarioId:long}/{rolId:int}")]
    public async Task<ActionResult> Eliminar(
        long usuarioId, int rolId)
    {
        var eliminado = await _servicio.EliminarAsync(
            usuarioId, rolId);

        if (!eliminado)
            return NotFound("No se encontró la asignación.");

        return NoContent();
    }

    [HttpPatch("{usuarioId:long}/{rolId:int}/reactivar")]
    public async Task<ActionResult> Reactivar(
        long usuarioId, int rolId)
    {
        try
        {
            var reactivado = await _servicio.ReactivarAsync(
                usuarioId, rolId);

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
            return Conflict(
                "No se pudo reactivar la asignación.");
        }
    }
}
