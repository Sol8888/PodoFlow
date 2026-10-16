using Microsoft.AspNetCore.Mvc;
using PodoFlow.Api.DTOs.EvaluacionRiesgo;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores
{
    [ApiController]
    [Route("api/[controller]")]
    public class EvaluacionRiesgoController : ControllerBase
    {
        private readonly IEvaluacionRiesgoServicio _servicio;

        public EvaluacionRiesgoController(IEvaluacionRiesgoServicio servicio)
        {
            _servicio = servicio;
        }

        [HttpGet]
        public async Task<ActionResult<List<EvaluacionRiesgoRespuestaDto>>> ObtenerTodos(
            [FromQuery] long? evaluacionId = null,
            [FromQuery] long? riesgoId = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamano = 10)
        {
            if (pagina < 1 || tamano < 1 || tamano > 100)
                return BadRequest(new { mensaje = "Paginación inválida." });

            return Ok(await _servicio.ObtenerTodosAsync(evaluacionId, riesgoId, pagina, tamano));
        }

        [HttpGet("{evaluacionId:long}/{riesgoId:long}")]
        public async Task<ActionResult<EvaluacionRiesgoRespuestaDto>> Obtener(long evaluacionId, long riesgoId)
        {
            var asignacion = await _servicio.ObtenerAsync(evaluacionId, riesgoId);

            if (asignacion == null)
                return NotFound();

            return Ok(asignacion);
        }

        [HttpPost]
        public async Task<ActionResult<EvaluacionRiesgoRespuestaDto>> Crear(EvaluacionRiesgoCrearDto dto)
        {
            try
            {
                var creada = await _servicio.CrearAsync(dto);

                return CreatedAtAction(nameof(Obtener),
                    new { evaluacionId = creada.EvaluacionId, riesgoId = creada.RiesgoId }, creada);
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

        // POST api/EvaluacionRiesgo/copiar → pasa los riesgos de una versión a la siguiente
        [HttpPost("copiar")]
        public async Task<ActionResult<List<EvaluacionRiesgoRespuestaDto>>> Copiar(EvaluacionRiesgoCopiarDto dto)
        {
            try
            {
                return Ok(await _servicio.CopiarAsync(dto));
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
