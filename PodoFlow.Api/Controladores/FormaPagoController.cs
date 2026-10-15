
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using PodoFlow.Api.DTOs.FormaPago;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores;

[ApiController]
[Authorize(Policy = "FinanzasGestionar")]
[Route("api/formas-pago")]
public class FormaPagoController : ControllerBase
{
    private readonly IFormaPagoServicio _servicio;

    public FormaPagoController(IFormaPagoServicio servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos(
        [FromQuery] bool incluirInactivos = false,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 10)
    {
        try
        {
            return Ok(await _servicio.ObtenerTodosAsync(
                incluirInactivos, pagina, tamano));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        try
        {
            var forma = await _servicio.ObtenerPorIdAsync(id);

            return forma == null
                ? NotFound("No se encontró la forma de pago.")
                : Ok(forma);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Crear(
        [FromBody] FormaPagoCrearDto dto)
    {
        try
        {
            var forma = await _servicio.CrearAsync(dto);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = forma.Id },
                forma);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (DbUpdateException ex) when (EsCodigoDuplicado(ex))
        {
            return Conflict(
                "El código ya existe. Vuelve a consultar las formas de pago.");
        }
        catch (DbUpdateException)
        {
            return Conflict("No se pudo registrar la forma de pago.");
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(
        int id,
        [FromBody] FormaPagoActualizarDto dto)
    {
        try
        {
            bool actualizado =
                await _servicio.ActualizarAsync(id, dto);

            return actualizado ? NoContent() : NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (DbUpdateException ex) when (EsCodigoDuplicado(ex))
        {
            return Conflict(
                "El código ya está registrado.");
        }
        catch (DbUpdateException)
        {
            return Conflict(
                "No se pudo actualizar la forma de pago.");
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            bool eliminado = await _servicio.EliminarAsync(id);

            return eliminado ? NoContent() : NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (DbUpdateException)
        {
            return Conflict(
                "No se pudo desactivar la forma de pago.");
        }
    }

    [HttpPatch("{id:int}/reactivar")]
    public async Task<IActionResult> Reactivar(int id)
    {
        try
        {
            bool reactivado = await _servicio.ReactivarAsync(id);

            return reactivado ? NoContent() : NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (DbUpdateException)
        {
            return Conflict(
                "No se pudo reactivar la forma de pago.");
        }
    }

    private static bool EsCodigoDuplicado(DbUpdateException ex)
    {
        return ex.InnerException is SqlException sql &&
               (sql.Number == 2601 || sql.Number == 2627);
    }
}
