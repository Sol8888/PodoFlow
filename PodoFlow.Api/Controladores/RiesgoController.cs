using Microsoft.AspNetCore.Mvc;
using PodoFlow.Api.DTOs.Riesgo;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores
{
    // Caso B: no hay DELETE; la "baja" es pasar a CERRADO con PATCH {id}/estado.
    [ApiController]
    [Route("api/[controller]")]
    public class RiesgoController : ControllerBase
    {
        private readonly IRiesgoServicio _servicio;

        public RiesgoController(IRiesgoServicio servicio)
        {
            _servicio = servicio;
        }

        // GET api/Riesgo?tratamientoId=1&estado=ABIERTO&pagina=1&tamano=10
        [HttpGet]
        public async Task<ActionResult<List<RiesgoRespuestaDto>>> ObtenerTodos(
            [FromQuery] int? tratamientoId = null,
            [FromQuery] string? estado = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamano = 10)
        {
            if (pagina < 1 || tamano < 1 || tamano > 100)
                return BadRequest(new { mensaje = "Paginación inválida." });

            try
            {
                return Ok(await _servicio.ObtenerTodosAsync(tratamientoId, estado, pagina, tamano));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<RiesgoRespuestaDto>> ObtenerPorId(long id)
        {
            var riesgo = await _servicio.ObtenerPorIdAsync(id);

            if (riesgo == null)
                return NotFound();

            return Ok(riesgo);
        }

        [HttpPost]
        public async Task<ActionResult<RiesgoRespuestaDto>> Crear(RiesgoCrearDto dto)
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

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Actualizar(long id, RiesgoActualizarDto dto)
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

        // PATCH api/Riesgo/5/estado → mitigar, aceptar, reabrir o cerrar
        [HttpPatch("{id:long}/estado")]
        public async Task<ActionResult<RiesgoRespuestaDto>> CambiarEstado(long id, RiesgoCambiarEstadoDto dto)
        {
            try
            {
                var riesgo = await _servicio.CambiarEstadoAsync(id, dto);

                if (riesgo == null)
                    return NotFound();

                return Ok(riesgo);
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
