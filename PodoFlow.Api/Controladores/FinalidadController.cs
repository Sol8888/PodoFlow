using Microsoft.AspNetCore.Mvc;
using PodoFlow.Api.DTOs.Finalidad;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores
{
    [ApiController]
    [Route("api/[controller]")]
    public class FinalidadController : ControllerBase
    {
        private readonly IFinalidadServicio _servicio;

        public FinalidadController(IFinalidadServicio servicio)
        {
            _servicio = servicio;
        }

        [HttpGet]
        public async Task<ActionResult<List<FinalidadRespuestaDto>>> ObtenerTodos()
        {
            return Ok(await _servicio.ObtenerTodosAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<FinalidadRespuestaDto>> ObtenerPorId(int id)
        {
            var finalidad = await _servicio.ObtenerPorIdAsync(id);

            if (finalidad == null)
                return NotFound();

            return Ok(finalidad);
        }

        [HttpPost]
        public async Task<ActionResult<FinalidadRespuestaDto>> Crear(FinalidadCrearDto dto)
        {
            try
            {
                var creada = await _servicio.CrearAsync(dto);

                return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.Id }, creada);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, FinalidadActualizarDto dto)
        {
            var actualizado = await _servicio.ActualizarAsync(id, dto);

            if (!actualizado)
                return NotFound();

            return NoContent();
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
