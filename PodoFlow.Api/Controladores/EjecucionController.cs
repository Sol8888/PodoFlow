using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodoFlow.Api.DTOs.Ejecucion;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores
{
    [ApiController]
    [Authorize(Policy = "AnaliticaVer")]
    [Route("api/[controller]")]
    public class EjecucionController : ControllerBase
    {
        private readonly IEjecucionServicio _servicio;

        public EjecucionController(IEjecucionServicio servicio)
        {
            _servicio = servicio;
        }

        [HttpGet]
        public async Task<ActionResult<List<EjecucionRespuestaDto>>> ObtenerTodos(
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
        public async Task<ActionResult<EjecucionRespuestaDto>> ObtenerPorId(long id)
        {
            var ejecucion = await _servicio.ObtenerPorIdAsync(id);

            if (ejecucion == null)
                return NotFound();

            return Ok(ejecucion);
        }

        [HttpGet("{id:long}/datos")]
        public async Task<ActionResult<List<DatoAnaliticoRespuestaDto>>> ObtenerDatos(
            long id,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamano = 50)
        {
            if (pagina < 1 || tamano < 1 || tamano > 500)
                return BadRequest(new { mensaje = "Paginación inválida." });

            var datos = await _servicio.ObtenerDatosAsync(id, pagina, tamano);

            if (datos == null)
                return NotFound();

            return Ok(datos);
        }

        [HttpGet("{id:long}/recomendaciones")]
        public async Task<ActionResult<List<RecomendacionRespuestaDto>>> ObtenerRecomendaciones(long id)
        {
            var recomendaciones = await _servicio.ObtenerRecomendacionesAsync(id);

            if (recomendaciones == null)
                return NotFound();

            return Ok(recomendaciones);
        }

        [HttpPost]
        public async Task<ActionResult<EjecucionRespuestaDto>> Crear(EjecucionCrearDto dto)
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

        [HttpPost("{id:long}/datos")]
        public async Task<IActionResult> CargarDatos(long id, DatoAnaliticoLoteDto dto)
        {
            try
            {
                var cargados = await _servicio.CargarDatosAsync(id, dto);

                if (cargados == null)
                    return NotFound();

                return CreatedAtAction(nameof(ObtenerDatos), new { id }, new { cargados });
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

        [HttpPost("{id:long}/recomendaciones")]
        public async Task<ActionResult<RecomendacionRespuestaDto>> CrearRecomendacion(long id, RecomendacionCrearDto dto)
        {
            try
            {
                var recomendacion = await _servicio.CrearRecomendacionAsync(id, dto);

                if (recomendacion == null)
                    return NotFound();

                return CreatedAtAction(nameof(ObtenerRecomendaciones), new { id }, recomendacion);
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
        public async Task<IActionResult> Actualizar(long id, EjecucionActualizarDto dto)
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

        [HttpPatch("{id:long}/estado")]
        public async Task<ActionResult<EjecucionRespuestaDto>> CambiarEstado(long id, EjecucionCambiarEstadoDto dto)
        {
            try
            {
                var ejecucion = await _servicio.CambiarEstadoAsync(id, dto);

                if (ejecucion == null)
                    return NotFound();

                return Ok(ejecucion);
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
