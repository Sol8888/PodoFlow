using Microsoft.AspNetCore.Mvc;
using PodoFlow.Api.DTOs.Seudonimo;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeudonimoController : ControllerBase
    {
        private readonly ISeudonimoServicio _servicio;

        public SeudonimoController(ISeudonimoServicio servicio)
        {
            _servicio = servicio;
        }

        // GET api/Seudonimo?incluirInactivos=false&pagina=1&tamano=10
        [HttpGet]
        public async Task<ActionResult<List<SeudonimoRespuestaDto>>> ObtenerTodos(
            [FromQuery] bool incluirInactivos = false,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamano = 10)
        {
            if (pagina < 1 || tamano < 1 || tamano > 100)
                return BadRequest(new { mensaje = "Paginación inválida." });

            return Ok(await _servicio.ObtenerTodosAsync(incluirInactivos, pagina, tamano));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<SeudonimoRespuestaDto>> ObtenerPorId(Guid id)
        {
            var seudonimo = await _servicio.ObtenerPorIdAsync(id);

            if (seudonimo == null)
                return NotFound();

            return Ok(seudonimo);
        }

        [HttpGet("paciente/{pacienteId:long}")]
        public async Task<ActionResult<SeudonimoRespuestaDto>> ObtenerPorPaciente(long pacienteId)
        {
            var seudonimo = await _servicio.ObtenerPorPacienteAsync(pacienteId);

            if (seudonimo == null)
                return NotFound();

            return Ok(seudonimo);
        }

        [HttpPost]
        public async Task<ActionResult<SeudonimoRespuestaDto>> Crear(SeudonimoCrearDto dto)
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

        [HttpPost("generar")]
        public async Task<ActionResult<SeudonimoGeneracionRespuestaDto>> Generar()
        {
            try
            {
                return Ok(await _servicio.GenerarAsync());
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Eliminar(Guid id)
        {
            var eliminado = await _servicio.EliminarAsync(id);

            if (!eliminado)
                return NotFound();

            return NoContent();
        }

        [HttpPatch("{id:guid}/reactivar")]
        public async Task<IActionResult> Reactivar(Guid id)
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
