
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.DTOs.PacienteUsuario;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores;

[ApiController]
[Route("api/pacientes-usuarios")]
public class PacienteUsuarioController : ControllerBase
{
    private readonly IPacienteUsuarioServicio _servicio;

    public PacienteUsuarioController(
        IPacienteUsuarioServicio servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(
        bool incluirInactivos = false,
        int pagina = 1,
        int tamano = 10)
    {
        if (pagina < 1 || tamano < 1 || tamano > 100)
            return BadRequest("Paginación inválida.");

        return Ok(await _servicio.ListarAsync(
            incluirInactivos, pagina, tamano));
    }

    [HttpGet("{pacienteId:long}/{usuarioId:long}")]
    public async Task<IActionResult> Obtener(
        long pacienteId, long usuarioId)
    {
        var relacion = await _servicio.ObtenerAsync(
            pacienteId, usuarioId);
        return relacion == null ? NotFound() : Ok(relacion);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(
        PacienteUsuarioCrearDto dto)
    {
        try
        {
            var relacion = await _servicio.CrearAsync(dto);

            return CreatedAtAction(nameof(Obtener),
                new
                {
                    pacienteId = relacion.PacienteId,
                    usuarioId = relacion.UsuarioId
                }, relacion);
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
            return Conflict("La relación viola una restricción UNIQUE.");
        }
    }

    [HttpPut("{pacienteId:long}/{usuarioId:long}")]
    public async Task<IActionResult> Actualizar(
        long pacienteId, long usuarioId,
        PacienteUsuarioActualizarDto dto)
    {
        try
        {
            return await _servicio.ActualizarAsync(
                pacienteId, usuarioId, dto)
                ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpDelete("{pacienteId:long}/{usuarioId:long}")]
    public async Task<IActionResult> Eliminar(
        long pacienteId, long usuarioId)
    {
        return await _servicio.EliminarAsync(pacienteId, usuarioId)
            ? NoContent() : NotFound();
    }

    [HttpPatch("{pacienteId:long}/{usuarioId:long}/reactivar")]
    public async Task<IActionResult> Reactivar(
        long pacienteId, long usuarioId)
    {
        try
        {
            return await _servicio.ReactivarAsync(
                pacienteId, usuarioId)
                ? NoContent() : NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
