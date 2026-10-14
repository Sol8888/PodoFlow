using Microsoft.AspNetCore.Mvc;
using PodoFlow.Api.DTOs.TratamientoEncargado;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores
{
    // Tabla intermedia: el registro se identifica por las dos llaves en la ruta.
    // No hay PUT porque no hay nada que editar.
    [ApiController]
    [Route("api/[controller]")]
    public class TratamientoEncargadoController : ControllerBase
    {
        private readonly ITratamientoEncargadoServicio _servicio;

        public TratamientoEncargadoController(ITratamientoEncargadoServicio servicio)
        {
            _servicio = servicio;
        }

        // GET api/TratamientoEncargado?tratamientoId=1&incluirInactivos=false&pagina=1&tamano=10
        // GET api/TratamientoEncargado?encargadoId=2  → tratamientos en los que participa ese encargado
        [HttpGet]
        public async Task<ActionResult<List<TratamientoEncargadoRespuestaDto>>> ObtenerTodos(
            [FromQuery] int? tratamientoId = null,
            [FromQuery] int? encargadoId = null,
            [FromQuery] bool incluirInactivos = false,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamano = 10)
        {
            if (pagina < 1 || tamano < 1 || tamano > 100)
                return BadRequest(new { mensaje = "Paginación inválida." });

            return Ok(await _servicio.ObtenerTodosAsync(
                tratamientoId, encargadoId, incluirInactivos, pagina, tamano));
        }

        // Devuelve la asignación esté activa o no.
        [HttpGet("{tratamientoId:int}/{encargadoId:int}")]
        public async Task<ActionResult<TratamientoEncargadoRespuestaDto>> Obtener(int tratamientoId, int encargadoId)
        {
            var asignacion = await _servicio.ObtenerAsync(tratamientoId, encargadoId);

            if (asignacion == null)
                return NotFound();

            return Ok(asignacion);
        }

        [HttpPost]
        public async Task<ActionResult<TratamientoEncargadoRespuestaDto>> Crear(TratamientoEncargadoCrearDto dto)
        {
            try
            {
                var creada = await _servicio.CrearAsync(dto);

                return CreatedAtAction(nameof(Obtener),
                    new { tratamientoId = creada.TratamientoId, encargadoId = creada.EncargadoId }, creada);
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

        // Baja lógica: te_estatus = 0.
        [HttpDelete("{tratamientoId:int}/{encargadoId:int}")]
        public async Task<IActionResult> Eliminar(int tratamientoId, int encargadoId)
        {
            var eliminado = await _servicio.EliminarAsync(tratamientoId, encargadoId);

            if (!eliminado)
                return NotFound();

            return NoContent();
        }

        // te_estatus = 1.
        [HttpPatch("{tratamientoId:int}/{encargadoId:int}/reactivar")]
        public async Task<IActionResult> Reactivar(int tratamientoId, int encargadoId)
        {
            try
            {
                var reactivado = await _servicio.ReactivarAsync(tratamientoId, encargadoId);

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
