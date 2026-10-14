
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.DTOs.Antecedente;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores;

[ApiController]
[Authorize(Policy = "HistoriaClinicaGestionar")]
[Route("api/antecedentes")]
public class AntecedenteController : ControllerBase
{
    private readonly IAntecedenteServicio _servicio;

    public AntecedenteController(IAntecedenteServicio servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos(
        [FromQuery] long? historiaId = null,
        [FromQuery] bool incluirInactivos = false,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 10)
    {
        try
        {
            var antecedentes =
                await _servicio.ObtenerTodosAsync(
                    historiaId,
                    incluirInactivos,
                    pagina,
                    tamano);

            return Ok(antecedentes);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> ObtenerPorId(long id)
    {
        var antecedente =
            await _servicio.ObtenerPorIdAsync(id);

        if (antecedente == null)
            return NotFound("No se encontró el antecedente.");

        return Ok(antecedente);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(
        [FromBody] AntecedenteCrearDto dto)
    {
        try
        {
            var antecedente = await _servicio.CrearAsync(dto);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = antecedente.Id },
                antecedente);
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
                "No se pudo registrar el antecedente.");
        }
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Actualizar(
        long id,
        [FromBody] AntecedenteActualizarDto dto)
    {
        try
        {
            var actualizado =
                await _servicio.ActualizarAsync(id, dto);

            return actualizado
                ? NoContent()
                : NotFound("No se encontró el antecedente.");
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
                "No se pudo actualizar el antecedente.");
        }
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Eliminar(long id)
    {
        try
        {
            var eliminado = await _servicio.EliminarAsync(id);

            return eliminado
                ? NoContent()
                : NotFound("No se encontró el antecedente.");
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
                "No se pudo desactivar el antecedente.");
        }
    }

    [HttpPatch("{id:long}/reactivar")]
    public async Task<IActionResult> Reactivar(long id)
    {
        try
        {
            var reactivado =
                await _servicio.ReactivarAsync(id);

            return reactivado
                ? NoContent()
                : NotFound("No se encontró el antecedente.");
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
                "No se pudo reactivar el antecedente.");
        }
    }
}
