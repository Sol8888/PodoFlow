
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.DTOs.DatoContacto;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores;

[ApiController]
[Route("api/datos-contacto")]
public class DatoContactoController : ControllerBase
{
    private readonly IDatoContactoServicio _servicio;

    public DatoContactoController(IDatoContactoServicio servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(
        long? pacienteId = null,
        bool incluirInactivos = false,
        int pagina = 1,
        int tamano = 10)
    {
        if (pagina < 1 || tamano < 1 || tamano > 100 ||
            (pacienteId.HasValue && pacienteId <= 0))
            return BadRequest("Parámetros inválidos.");

        return Ok(await _servicio.ListarAsync(
            pacienteId, incluirInactivos, pagina, tamano));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Obtener(long id)
    {
        var contacto = await _servicio.ObtenerAsync(id);
        return contacto == null ? NotFound() : Ok(contacto);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(DatoContactoCrearDto dto)
    {
        try
        {
            var contacto = await _servicio.CrearAsync(dto);
            return CreatedAtAction(nameof(Obtener),
                new { id = contacto.Id }, contacto);
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
            return Conflict("No se pudo crear el contacto.");
        }
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Actualizar(
        long id, DatoContactoActualizarDto dto)
    {
        try
        {
            return await _servicio.ActualizarAsync(id, dto)
                ? NoContent() : NotFound();
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
            return Conflict("No se pudo actualizar el contacto.");
        }
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Eliminar(long id)
    {
        return await _servicio.EliminarAsync(id)
            ? NoContent() : NotFound();
    }

    [HttpPatch("{id:long}/reactivar")]
    public async Task<IActionResult> Reactivar(long id)
    {
        try
        {
            return await _servicio.ReactivarAsync(id)
                ? NoContent() : NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
