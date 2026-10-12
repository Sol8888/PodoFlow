
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.DTOs.Usuario;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores;

[ApiController]
[Route("api/usuarios")]
public class UsuarioController : ControllerBase
{
    private readonly IUsuarioServicio _servicio;

    public UsuarioController(IUsuarioServicio servicio)
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

        var usuarios = await _servicio.ObtenerTodosAsync(
            incluirInactivos, pagina, tamano);

        return Ok(usuarios);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult> ObtenerPorId(long id)
    {
        var usuario = await _servicio.ObtenerPorIdAsync(id);

        if (usuario == null) return NotFound();
        return Ok(usuario);
    }

    [HttpPost]
    public async Task<ActionResult> Crear(
        [FromBody] UsuarioCrearDto dto)
    {
        try
        {
            var usuario = await _servicio.CrearAsync(dto);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = usuario.Id },
                usuario);
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
            return Conflict("No se pudo registrar el usuario.");
        }
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult> Actualizar(
        long id, [FromBody] UsuarioActualizarDto dto)
    {
        try
        {
            var actualizado = await _servicio.ActualizarAsync(id, dto);

            if (!actualizado) return NotFound();
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
        catch (DbUpdateConcurrencyException)
        {
            return Conflict("El registro cambió. Vuelve a consultarlo.");
        }
        catch (DbUpdateException)
        {
            return Conflict("No se pudo actualizar el usuario.");
        }
    }

    [HttpDelete("{id:long}")]
    public async Task<ActionResult> Eliminar(long id)
    {
        var eliminado = await _servicio.EliminarAsync(id);

        if (!eliminado) return NotFound();
        return NoContent();
    }

    [HttpPatch("{id:long}/reactivar")]
    public async Task<ActionResult> Reactivar(long id)
    {
        var reactivado = await _servicio.ReactivarAsync(id);

        if (!reactivado) return NotFound();
        return NoContent();
    }
}
