using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodoFlow.Api.DTOs.Exportacion;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores
{
    [ApiController]
    [Authorize(Policy = "PrivacidadGestionar")]
    [Route("api/[controller]")]
    public class ExportacionController : ControllerBase
    {
        private readonly IExportacionServicio _servicio;

        public ExportacionController(IExportacionServicio servicio)
        {
            _servicio = servicio;
        }

        [HttpGet]
        public async Task<ActionResult<List<ExportacionRespuestaDto>>> ObtenerTodos(
            [FromQuery] int pagina = 1,
            [FromQuery] int tamano = 10)
        {
            if (pagina < 1 || tamano < 1 || tamano > 100)
                return BadRequest(new { mensaje = "Paginación inválida." });

            return Ok(await _servicio.ObtenerTodosAsync(pagina, tamano));
        }

        [HttpGet("solicitud/{solicitudId:long}")]
        public async Task<ActionResult<List<ExportacionRespuestaDto>>> ObtenerPorSolicitud(long solicitudId)
        {
            var exportaciones = await _servicio.ObtenerPorSolicitudAsync(solicitudId);

            if (exportaciones == null)
                return NotFound();

            return Ok(exportaciones);
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<ExportacionRespuestaDto>> ObtenerPorId(long id)
        {
            var exportacion = await _servicio.ObtenerPorIdAsync(id);

            if (exportacion == null)
                return NotFound();

            return Ok(exportacion);
        }

        [HttpGet("{id:long}/descargar")]
        public async Task<IActionResult> Descargar(long id, [FromQuery] long usuarioId)
        {
            try
            {
                var archivo = await _servicio.DescargarAsync(id, usuarioId);

                if (archivo == null)
                    return NotFound();

                return File(archivo.Value.Contenido, archivo.Value.TipoContenido, archivo.Value.NombreArchivo);
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

        [HttpPost]
        public async Task<ActionResult<ExportacionRespuestaDto>> Crear(ExportacionCrearDto dto)
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

        [HttpPatch("{id:long}/estado")]
        public async Task<ActionResult<ExportacionRespuestaDto>> CambiarEstado(long id, ExportacionCambiarEstadoDto dto)
        {
            try
            {
                var exportacion = await _servicio.CambiarEstadoAsync(id, dto);

                if (exportacion == null)
                    return NotFound();

                return Ok(exportacion);
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
