using Microsoft.AspNetCore.Mvc;
using PodoFlow.Api.DTOs.TratamientoCategoria;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores
{
    [ApiController]
    [Route("api/[controller]")]
    public class TratamientoCategoriaController : ControllerBase
    {
        private readonly ITratamientoCategoriaServicio _servicio;

        public TratamientoCategoriaController(ITratamientoCategoriaServicio servicio)
        {
            _servicio = servicio;
        }

        // GET api/TratamientoCategoria?tratamientoId=1&incluirInactivos=false&pagina=1&tamano=10
        // GET api/TratamientoCategoria?categoriaId=4  → tratamientos que usan esa categoría
        [HttpGet]
        public async Task<ActionResult<List<TratamientoCategoriaRespuestaDto>>> ObtenerTodos(
            [FromQuery] int? tratamientoId = null,
            [FromQuery] int? categoriaId = null,
            [FromQuery] bool incluirInactivos = false,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamano = 10)
        {
            if (pagina < 1 || tamano < 1 || tamano > 100)
                return BadRequest(new { mensaje = "Paginación inválida." });

            return Ok(await _servicio.ObtenerTodosAsync(
                tratamientoId, categoriaId, incluirInactivos, pagina, tamano));
        }

        // Devuelve la asignación esté activa o no.
        [HttpGet("{tratamientoId:int}/{categoriaId:int}")]
        public async Task<ActionResult<TratamientoCategoriaRespuestaDto>> Obtener(int tratamientoId, int categoriaId)
        {
            var asignacion = await _servicio.ObtenerAsync(tratamientoId, categoriaId);

            if (asignacion == null)
                return NotFound();

            return Ok(asignacion);
        }

        [HttpPost]
        public async Task<ActionResult<TratamientoCategoriaRespuestaDto>> Crear(TratamientoCategoriaCrearDto dto)
        {
            try
            {
                var creada = await _servicio.CrearAsync(dto);

                return CreatedAtAction(nameof(Obtener),
                    new { tratamientoId = creada.TratamientoId, categoriaId = creada.CategoriaId }, creada);
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

        // Baja lógica: tc_estatus = 0.
        [HttpDelete("{tratamientoId:int}/{categoriaId:int}")]
        public async Task<IActionResult> Eliminar(int tratamientoId, int categoriaId)
        {
            var eliminado = await _servicio.EliminarAsync(tratamientoId, categoriaId);

            if (!eliminado)
                return NotFound();

            return NoContent();
        }

        // tc_estatus = 1.
        [HttpPatch("{tratamientoId:int}/{categoriaId:int}/reactivar")]
        public async Task<IActionResult> Reactivar(int tratamientoId, int categoriaId)
        {
            try
            {
                var reactivado = await _servicio.ReactivarAsync(tratamientoId, categoriaId);

                if (!reactivado)
                    return NotFound();

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}
