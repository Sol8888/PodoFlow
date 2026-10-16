using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.DTOs.Consentimiento;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConsentimientoController : ControllerBase
    {
        private const string MensajeConcurrencia =
            "El consentimiento fue modificado. Vuelva a consultarlo e intente de nuevo.";

        private readonly IConsentimientoServicio _servicio;

        public ConsentimientoController(IConsentimientoServicio servicio)
        {
            _servicio = servicio;
        }

        [HttpGet]
        public async Task<ActionResult<List<ConsentimientoRespuestaDto>>> ObtenerTodos(
            [FromQuery] int pagina = 1,
            [FromQuery] int tamano = 10)
        {
            if (pagina < 1 || tamano < 1 || tamano > 100)
                return BadRequest(new { mensaje = "Paginación inválida." });

            return Ok(await _servicio.ObtenerTodosAsync(pagina, tamano));
        }

        [HttpGet("paciente/{pacienteId:long}")]
        public async Task<ActionResult<List<ConsentimientoRespuestaDto>>> ObtenerPorPaciente(long pacienteId)
        {
            return Ok(await _servicio.ObtenerPorPacienteAsync(pacienteId));
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<ConsentimientoRespuestaDto>> ObtenerPorId(long id)
        {
            var consentimiento = await _servicio.ObtenerPorIdAsync(id);

            if (consentimiento == null)
                return NotFound();

            return Ok(consentimiento);
        }

        [HttpPost]
        public async Task<ActionResult<ConsentimientoRespuestaDto>> Crear(ConsentimientoCrearDto dto)
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

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Actualizar(long id, ConsentimientoActualizarDto dto)
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
        }

        // PATCH api/Consentimiento/5/estado  → aceptar, rechazar o revocar
        [HttpPatch("{id:long}/estado")]
        public async Task<ActionResult<ConsentimientoRespuestaDto>> CambiarEstado(long id, ConsentimientoCambiarEstadoDto dto)
        {
            try
            {
                var consentimiento = await _servicio.CambiarEstadoAsync(id, dto);

                if (consentimiento == null)
                    return NotFound();

                return Ok(consentimiento);
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
        [HttpGet("{id:long}/eventos")]
        public async Task<ActionResult<List<EventoConsentimientoRespuestaDto>>> ObtenerEventos(long id)
        {
            var eventos = await _servicio.ObtenerEventosAsync(id);

            if (eventos == null)
                return NotFound();

            return Ok(eventos);
        }
    }
}
