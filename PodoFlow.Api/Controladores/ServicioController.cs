using Microsoft.AspNetCore.Mvc;
using PodoFlow.Api.DTOs.Servicio;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores;


[ApiController]
[Route("api/servicios")]
public class ServicioController : ControllerBase
{
    private readonly IServicioServicio _servicio;

    public ServicioController(IServicioServicio servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<ActionResult> ObtenerTodos()
    {
        var servicios =
            await _servicio.ObtenerTodosAsync();

        return Ok(servicios);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> ObtenerPorId(int id)
    {
        var servicio =
            await _servicio.ObtenerPorIdAsync(id);

        if (servicio == null)
            return NotFound();

        return Ok(servicio);
    }

    [HttpPost]
    public async Task<ActionResult> Crear(
        ServicioCrearDto dto)
    {
        var servicio =
            await _servicio.CrearAsync(dto);

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { id = servicio.Id },
            servicio
        );
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Actualizar(
        int id,
        ServicioActualizarDto dto)
    {
        var actualizado =
            await _servicio.ActualizarAsync(id, dto);

        if (!actualizado)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Eliminar(int id)
    {
        var eliminado =
            await _servicio.EliminarAsync(id);

        if (!eliminado)
            return NotFound();

        return NoContent();
    }
}
