
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.DTOs.HorarioAtencion;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores;

[ApiController]
[Route("api/horarios-atencion")]
public class HorarioAtencionController : ControllerBase
{
    private readonly IHorarioAtencionServicio _servicio;

    public HorarioAtencionController(
        IHorarioAtencionServicio servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos(
        bool incluirInactivos = false,
        int pagina = 1,
        int tamano = 10)
    {
        if (pagina < 1 || tamano < 1 || tamano > 100)
            return BadRequest("Paginación inválida.");

        return Ok(await _servicio.ObtenerTodosAsync(
            incluirInactivos, pagina, tamano));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var horario = await _servicio.ObtenerPorIdAsync(id);

        return horario == null
            ? NotFound()
            : Ok(horario);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(
        HorarioAtencionCrearDto dto)
    {
        try
        {
            var horario = await _servicio.CrearAsync(dto);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = horario.Id },
                horario);
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
            return Conflict("No se pudo crear el horario.");
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(
        int id,
        HorarioAtencionActualizarDto dto)
    {
        try
        {
            return await _servicio.ActualizarAsync(id, dto)
                ? NoContent()
                : NotFound();
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
            return Conflict("No se pudo actualizar el horario.");
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        return await _servicio.EliminarAsync(id)
            ? NoContent()
            : NotFound();
    }

    [HttpPatch("{id:int}/reactivar")]
    public async Task<IActionResult> Reactivar(int id)
    {
        try
        {
            return await _servicio.ReactivarAsync(id)
                ? NoContent()
                : NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }
}
