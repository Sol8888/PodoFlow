using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.DTOs.Notificacion;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores
{
    [ApiController]
    [Authorize(Policy = "ComunicacionGestionar")]
    [Route("api/[controller]")]
    public class NotificacionController : ControllerBase
    {
        private const string MensajeConcurrencia =
            "La notificación fue modificada por otro usuario o proceso. Vuelva a consultarla e intente de nuevo.";

        private readonly INotificacionServicio _servicio;

        public NotificacionController(INotificacionServicio servicio)
        {
            _servicio = servicio;
        }

        [HttpGet]
        public async Task<ActionResult<List<NotificacionRespuestaDto>>> ObtenerTodos(
            [FromQuery] long? pacienteId = null,
            [FromQuery] string? estado = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamano = 10)
        {
            if (pagina < 1 || tamano < 1 || tamano > 100)
                return BadRequest(new { mensaje = "Paginación inválida." });

            try
            {
                return Ok(await _servicio.ObtenerTodosAsync(pacienteId, estado, pagina, tamano));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpGet("por-enviar")]
        public async Task<ActionResult<List<NotificacionRespuestaDto>>> ObtenerPorEnviar(
            [FromQuery] int tamano = 20)
        {
            if (tamano < 1 || tamano > 100)
                return BadRequest(new { mensaje = "Paginación inválida." });

            return Ok(await _servicio.ObtenerPorEnviarAsync(tamano));
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<NotificacionRespuestaDto>> ObtenerPorId(long id)
        {
            var notificacion = await _servicio.ObtenerPorIdAsync(id);

            if (notificacion == null)
                return NotFound();

            return Ok(notificacion);
        }

        [HttpGet("{id:long}/envios")]
        public async Task<ActionResult<List<EnvioRespuestaDto>>> ObtenerEnvios(long id)
        {
            var envios = await _servicio.ObtenerEnviosAsync(id);

            if (envios == null)
                return NotFound();

            return Ok(envios);
        }

        [HttpPost]
        public async Task<ActionResult<NotificacionRespuestaDto>> Crear(NotificacionCrearDto dto)
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

        [HttpPost("recordatorios")]
        public async Task<IActionResult> ProgramarRecordatorios([FromQuery] int horasAntes = 24)
        {
            try
            {
                var (programadas, omitidas) = await _servicio.ProgramarRecordatoriosAsync(horasAntes);

                return Ok(new { programadas, omitidas });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPost("{id:long}/envios")]
        public async Task<ActionResult<EnvioRespuestaDto>> RegistrarEnvio(long id, EnvioCrearDto dto)
        {
            try
            {
                var envio = await _servicio.RegistrarEnvioAsync(id, dto);

                if (envio == null)
                    return NotFound();

                return CreatedAtAction(nameof(ObtenerEnvios), new { id }, envio);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new { mensaje = MensajeConcurrencia });
            }
            catch (DbUpdateException)
            {
                return Conflict(new { mensaje = "Otro proceso registró un intento al mismo tiempo. Vuelva a consultar los envíos." });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Actualizar(long id, NotificacionActualizarDto dto)
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
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new { mensaje = MensajeConcurrencia });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
        }

        [HttpPatch("{id:long}/estado")]
        public async Task<ActionResult<NotificacionRespuestaDto>> CambiarEstado(long id, NotificacionCambiarEstadoDto dto)
        {
            try
            {
                var notificacion = await _servicio.CambiarEstadoAsync(id, dto);

                if (notificacion == null)
                    return NotFound();

                return Ok(notificacion);
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
