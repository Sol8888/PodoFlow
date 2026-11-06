using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.DTOs.Incidente;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores
{
    [ApiController]
    [Authorize(Policy = "PrivacidadGestionar")]
    [Route("api/[controller]")]
    public class IncidenteController : ControllerBase
    {
        private const string MensajeConcurrencia =
            "El incidente fue modificado por otro usuario. Vuelva a consultarlo e intente de nuevo.";

        private readonly IIncidenteServicio _servicio;

        public IncidenteController(IIncidenteServicio servicio)
        {
            _servicio = servicio;
        }

        [HttpGet]
        public async Task<ActionResult<List<IncidenteRespuestaDto>>> ObtenerTodos(
            [FromQuery] string? estado = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamano = 10)
        {
            if (pagina < 1 || tamano < 1 || tamano > 100)
                return BadRequest(new { mensaje = "Paginación inválida." });

            try
            {
                return Ok(await _servicio.ObtenerTodosAsync(estado, pagina, tamano));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<IncidenteRespuestaDto>> ObtenerPorId(long id)
        {
            var incidente = await _servicio.ObtenerPorIdAsync(id);

            if (incidente == null)
                return NotFound();

            return Ok(incidente);
        }

        [HttpGet("{id:long}/acciones")]
        public async Task<ActionResult<List<AccionIncidenteRespuestaDto>>> ObtenerAcciones(long id)
        {
            var acciones = await _servicio.ObtenerAccionesAsync(id);

            if (acciones == null)
                return NotFound();

            return Ok(acciones);
        }

        [HttpPost]
        public async Task<ActionResult<IncidenteRespuestaDto>> Crear(IncidenteCrearDto dto)
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
        }

        [HttpPost("{id:long}/acciones")]
        public async Task<ActionResult<AccionIncidenteRespuestaDto>> RegistrarAccion(long id, AccionIncidenteCrearDto dto)
        {
            try
            {
                var accion = await _servicio.RegistrarAccionAsync(id, dto);

                if (accion == null)
                    return NotFound();

                return CreatedAtAction(nameof(ObtenerAcciones), new { id }, accion);
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
        public async Task<IActionResult> Actualizar(long id, IncidenteActualizarDto dto)
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
        public async Task<ActionResult<IncidenteRespuestaDto>> CambiarEstado(long id, IncidenteCambiarEstadoDto dto)
        {
            try
            {
                var incidente = await _servicio.CambiarEstadoAsync(id, dto);

                if (incidente == null)
                    return NotFound();

                return Ok(incidente);
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
