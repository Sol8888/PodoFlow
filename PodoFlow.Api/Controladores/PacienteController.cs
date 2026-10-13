
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.DTOs.Paciente;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores;

[ApiController]
[Route("api/pacientes")]
public class PacienteController : ControllerBase
{
    private readonly IPacienteServicio _servicio;

    public PacienteController(IPacienteServicio servicio)
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

        return Ok(await _servicio.ObtenerTodosAsync(
            incluirInactivos, pagina, tamano));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Obtener(long id)
    {
        var paciente = await _servicio.ObtenerPorIdAsync(id);
        return paciente == null ? NotFound() : Ok(paciente);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(PacienteCrearDto dto)
    {
        try
        {
            var paciente = await _servicio.CrearAsync(dto);
            return CreatedAtAction(
                nameof(Obtener),
                new { id = paciente.Id },
                paciente);
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
            return Conflict("No se pudo registrar el paciente.");
        }
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Actualizar(
        long id, PacienteActualizarDto dto)
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
        catch (DbUpdateConcurrencyException)
        {
            return Conflict("El paciente fue modificado por otro proceso.");
        }
        catch (DbUpdateException)
        {
            return Conflict("No se pudo actualizar el paciente.");
        }
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Eliminar(
        long id, [FromQuery] string motivo)
    {
        try
        {
            return await _servicio.EliminarAsync(id, motivo)
                ? NoContent() : NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict("El paciente cambió. Vuelve a consultar.");
        }
    }

    [HttpPatch("{id:long}/reactivar")]
    public async Task<IActionResult> Reactivar(long id)
    {
        try
        {
            return await _servicio.ReactivarAsync(id)
                ? NoContent() : NotFound();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict("El paciente cambió. Vuelve a consultar.");
        }
    }
}
