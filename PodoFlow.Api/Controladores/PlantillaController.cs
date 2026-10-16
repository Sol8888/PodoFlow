using Microsoft.AspNetCore.Mvc;
using PodoFlow.Api.DTOs.Plantilla;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlantillaController : ControllerBase
    {
        private readonly IPlantillaServicio _servicio;

        public PlantillaController(IPlantillaServicio servicio)
        {
            _servicio = servicio;
        }

        [HttpGet]
        public async Task<ActionResult<List<PlantillaRespuestaDto>>> ObtenerTodos(
            [FromQuery] string? canal = null,
            [FromQuery] string? tipo = null,
            [FromQuery] bool incluirInactivos = false,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamano = 10)
        {
            if (pagina < 1 || tamano < 1 || tamano > 100)
                return BadRequest(new { mensaje = "Paginación inválida." });

            try
            {
                return Ok(await _servicio.ObtenerTodosAsync(canal, tipo, incluirInactivos, pagina, tamano));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<PlantillaRespuestaDto>> ObtenerPorId(int id)
        {
            var plantilla = await _servicio.ObtenerPorIdAsync(id);

            if (plantilla == null)
                return NotFound();

            return Ok(plantilla);
        }

        [HttpPost]
        public async Task<ActionResult<PlantillaRespuestaDto>> Crear(PlantillaCrearDto dto)
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

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, PlantillaActualizarDto dto)
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

        // Baja lógica: pl_estatus = 0.
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var eliminado = await _servicio.EliminarAsync(id);

                if (!eliminado)
                    return NotFound();

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
        }

        // pl_estatus = 1.
        [HttpPatch("{id:int}/reactivar")]
        public async Task<IActionResult> Reactivar(int id)
        {
            try
            {
                var reactivado = await _servicio.ReactivarAsync(id);

                if (!reactivado)
                    return NotFound();

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
        }
    }
}
