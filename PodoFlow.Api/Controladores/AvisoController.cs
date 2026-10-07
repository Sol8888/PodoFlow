using Microsoft.AspNetCore.Mvc;
using PodoFlow.Api.DTOs.Aviso;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores
{
    [ApiController]
    [Route("api/[controller]")]
    public class AvisoController : ControllerBase
    {
        private readonly IAvisoServicio _servicio;

        public AvisoController(IAvisoServicio servicio)
        {
            _servicio = servicio;
        }

        [HttpGet]
        public async Task<ActionResult<List<AvisoRespuestaDto>>> ObtenerTodos()
        {
            return Ok(await _servicio.ObtenerTodosAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<AvisoRespuestaDto>> ObtenerPorId(int id)
        {
            var aviso = await _servicio.ObtenerPorIdAsync(id);

            if (aviso == null)
                return NotFound();

            return Ok(aviso);
        }

        [HttpPost]
        public async Task<ActionResult<AvisoRespuestaDto>> Crear(AvisoCrearDto dto)
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

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, AvisoActualizarDto dto)
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
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var eliminado = await _servicio.EliminarAsync(id);

            if (!eliminado)
                return NotFound();

            return NoContent();
        }
    }
}