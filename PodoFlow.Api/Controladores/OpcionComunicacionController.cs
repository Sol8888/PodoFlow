using Microsoft.AspNetCore.Mvc;
using PodoFlow.Api.DTOs.OpcionComunicacion;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores
{
    [ApiController]
    [Route("api/[controller]")]
    public class OpcionComunicacionController : ControllerBase
    {
        private readonly IOpcionComunicacionServicio _servicio;

        public OpcionComunicacionController(IOpcionComunicacionServicio servicio)
        {
            _servicio = servicio;
        }

        [HttpGet]
        public async Task<ActionResult<List<OpcionComunicacionRespuestaDto>>> ObtenerTodos(
            [FromQuery] long? pacienteId = null,
            [FromQuery] int? finalidadId = null,
            [FromQuery] string? canal = null,
            [FromQuery] bool incluirInactivos = false,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamano = 10)
        {
            if (pagina < 1 || tamano < 1 || tamano > 100)
                return BadRequest(new { mensaje = "Paginación inválida." });

            try
            {
                return Ok(await _servicio.ObtenerTodosAsync(
                    pacienteId, finalidadId, canal, incluirInactivos, pagina, tamano));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // Devuelve el registro esté activo o no.
        [HttpGet("{id:long}")]
        public async Task<ActionResult<OpcionComunicacionRespuestaDto>> ObtenerPorId(long id)
        {
            var opcion = await _servicio.ObtenerPorIdAsync(id);

            if (opcion == null)
                return NotFound();

            return Ok(opcion);
        }

        // Si la opción existía dada de baja, se reactiva y responde 201 con el mismo id.
        [HttpPost]
        public async Task<ActionResult<OpcionComunicacionRespuestaDto>> Crear(OpcionComunicacionCrearDto dto)
        {
            try
            {
                var creada = await _servicio.CrearAsync(dto);

                return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.Id }, creada);
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

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Actualizar(long id, OpcionComunicacionActualizarDto dto)
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
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
        }

        // Baja lógica: op_estatus = 0.
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Eliminar(long id)
        {
            var eliminado = await _servicio.EliminarAsync(id);

            if (!eliminado)
                return NotFound();

            return NoContent();
        }

        // op_estatus = 1.
        [HttpPatch("{id:long}/reactivar")]
        public async Task<IActionResult> Reactivar(long id)
        {
            try
            {
                var reactivado = await _servicio.ReactivarAsync(id);

                if (!reactivado)
                    return NotFound();

                return NoContent();
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
    }
}
