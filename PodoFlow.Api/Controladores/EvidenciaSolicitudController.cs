using Microsoft.AspNetCore.Mvc;
using PodoFlow.Api.DTOs.EvidenciaSolicitud;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores
{
    [ApiController]
    [Route("api/[controller]")]
    public class EvidenciaSolicitudController : ControllerBase
    {
        private readonly IEvidenciaSolicitudServicio _servicio;

        public EvidenciaSolicitudController(IEvidenciaSolicitudServicio servicio)
        {
            _servicio = servicio;
        }

        [HttpGet("solicitud/{solicitudId:long}")]
        public async Task<ActionResult<List<EvidenciaSolicitudRespuestaDto>>> ObtenerPorSolicitud(long solicitudId)
        {
            var evidencias = await _servicio.ObtenerPorSolicitudAsync(solicitudId);

            if (evidencias == null)
                return NotFound();

            return Ok(evidencias);
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<EvidenciaSolicitudRespuestaDto>> ObtenerPorId(long id)
        {
            var evidencia = await _servicio.ObtenerPorIdAsync(id);

            if (evidencia == null)
                return NotFound();

            return Ok(evidencia);
        }

        [HttpPost]
        public async Task<ActionResult<EvidenciaSolicitudRespuestaDto>> Crear(EvidenciaSolicitudCrearDto dto)
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
    }
}