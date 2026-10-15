using Microsoft.AspNetCore.Mvc;
using PodoFlow.Api.DTOs.EvaluacionImpacto;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores
{
    // Caso B: no hay DELETE; la "baja" es pasar a CERRADA con PATCH {id}/estado.
    [ApiController]
    [Route("api/[controller]")]
    public class EvaluacionImpactoController : ControllerBase
    {
        private readonly IEvaluacionImpactoServicio _servicio;

        public EvaluacionImpactoController(IEvaluacionImpactoServicio servicio)
        {
            _servicio = servicio;
        }

        // GET api/EvaluacionImpacto?estado=APROBADA&pagina=1&tamano=10
        [HttpGet]
        public async Task<ActionResult<List<EvaluacionImpactoRespuestaDto>>> ObtenerTodos(
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
        public async Task<ActionResult<EvaluacionImpactoRespuestaDto>> ObtenerPorId(long id)
        {
            var evaluacion = await _servicio.ObtenerPorIdAsync(id);

            if (evaluacion == null)
                return NotFound();

            return Ok(evaluacion);
        }

        [HttpPost]
        public async Task<ActionResult<EvaluacionImpactoRespuestaDto>> Crear(EvaluacionImpactoCrearDto dto)
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
        public async Task<IActionResult> Actualizar(long id, EvaluacionImpactoActualizarDto dto)
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

        // PATCH api/EvaluacionImpacto/5/estado → aprobar, revisar o cerrar
        [HttpPatch("{id:long}/estado")]
        public async Task<ActionResult<EvaluacionImpactoRespuestaDto>> CambiarEstado(long id, EvaluacionImpactoCambiarEstadoDto dto)
        {
            try
            {
                var evaluacion = await _servicio.CambiarEstadoAsync(id, dto);

                if (evaluacion == null)
                    return NotFound();

                return Ok(evaluacion);
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
