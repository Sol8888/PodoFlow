using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.DTOs.Solicitud;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores
{
    [ApiController]
    [Route("api/[controller]")]
    public class SolicitudController : ControllerBase
    {
        private const string MensajeConcurrencia =
            "La solicitud fue modificada por otro usuario. Vuelva a consultarla e intente de nuevo.";

        private readonly ISolicitudServicio _servicio;

        public SolicitudController(ISolicitudServicio servicio)
        {
            _servicio = servicio;
        }

        [HttpGet]
        public async Task<ActionResult<List<SolicitudRespuestaDto>>> ObtenerTodos(
            [FromQuery] int pagina = 1,
            [FromQuery] int tamano = 10)
        {
            if (pagina < 1 || tamano < 1 || tamano > 100)
                return BadRequest(new { mensaje = "Paginación inválida." });

            return Ok(await _servicio.ObtenerTodosAsync(pagina, tamano));
        }

        [HttpGet("paciente/{pacienteId:long}")]
        public async Task<ActionResult<List<SolicitudRespuestaDto>>> ObtenerPorPaciente(long pacienteId)
        {
            return Ok(await _servicio.ObtenerPorPacienteAsync(pacienteId));
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<SolicitudRespuestaDto>> ObtenerPorId(long id)
        {
            var solicitud = await _servicio.ObtenerPorIdAsync(id);

            if (solicitud == null)
                return NotFound();

            return Ok(solicitud);
        }

        [HttpGet("{id:long}/acciones")]
        public async Task<ActionResult<List<AccionSolicitudRespuestaDto>>> ObtenerAcciones(long id)
        {
            var acciones = await _servicio.ObtenerAccionesAsync(id);

            if (acciones == null)
                return NotFound();

            return Ok(acciones);
        }

        [HttpPost]
        public async Task<ActionResult<SolicitudRespuestaDto>> Crear(SolicitudCrearDto dto)
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
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Actualizar(long id, SolicitudActualizarDto dto)
        {
            try
            {
                var actualizado = await _servicio.ActualizarAsync(id, dto);

                if (!actualizado)
                    return NotFound();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {

                return Conflict(new { mensaje = MensajeConcurrencia });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPatch("{id:long}/estado")]
        public async Task<ActionResult<SolicitudRespuestaDto>> CambiarEstado(long id, SolicitudCambiarEstadoDto dto)
        {
            try
            {
                var solicitud = await _servicio.CambiarEstadoAsync(id, dto);

                if (solicitud == null)
                    return NotFound();

                return Ok(solicitud);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new { mensaje = MensajeConcurrencia });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
        }
    }
}