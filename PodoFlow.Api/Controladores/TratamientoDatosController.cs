using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodoFlow.Api.DTOs.TratamientoDatos;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores
{
    [ApiController]
    [Authorize(Policy = "PrivacidadGestionar")]
    [Route("api/[controller]")]
    public class TratamientoDatosController : ControllerBase
    {
        private readonly ITratamientoDatosServicio _servicio;

        public TratamientoDatosController(ITratamientoDatosServicio servicio)
        {
            _servicio = servicio;
        }

        [HttpGet]
        public async Task<ActionResult<List<TratamientoDatosRespuestaDto>>> ObtenerTodos(
            [FromQuery] bool incluirInactivos = false,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamano = 10)
        {
            if (pagina < 1 || tamano < 1 || tamano > 100)
                return BadRequest(new { mensaje = "Paginación inválida." });

            return Ok(await _servicio.ObtenerTodosAsync(incluirInactivos, pagina, tamano));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TratamientoDatosRespuestaDto>> ObtenerPorId(int id)
        {
            var tratamiento = await _servicio.ObtenerPorIdAsync(id);

            if (tratamiento == null)
                return NotFound();

            return Ok(tratamiento);
        }

        [HttpPost]
        public async Task<ActionResult<TratamientoDatosRespuestaDto>> Crear(TratamientoDatosCrearDto dto)
        {
            try
            {
                var creado = await _servicio.CrearAsync(dto);

                return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, TratamientoDatosActualizarDto dto)
        {
            try
            {
                var actualizado = await _servicio.ActualizarAsync(id, dto);

                if (!actualizado)
                    return NotFound();

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var eliminado = await _servicio.EliminarAsync(id);

            if (!eliminado)
                return NotFound();

            return NoContent();
        }

        [HttpPatch("{id:int}/reactivar")]
        public async Task<IActionResult> Reactivar(int id)
        {
            var reactivado = await _servicio.ReactivarAsync(id);

            if (!reactivado)
                return NotFound();

            return NoContent();
        }
    }
}
