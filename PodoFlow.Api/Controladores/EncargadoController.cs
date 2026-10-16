using Microsoft.AspNetCore.Mvc;
using PodoFlow.Api.DTOs.Encargado;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores
{
    [ApiController]
    [Route("api/[controller]")]
    public class EncargadoController : ControllerBase
    {
        private readonly IEncargadoServicio _servicio;

        public EncargadoController(IEncargadoServicio servicio)
        {
            _servicio = servicio;
        }

        [HttpGet]
        public async Task<ActionResult<List<EncargadoRespuestaDto>>> ObtenerTodos(
            [FromQuery] bool incluirInactivos = false,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamano = 10)
        {
            if (pagina < 1 || tamano < 1 || tamano > 100)
                return BadRequest(new { mensaje = "Paginación inválida." });

            return Ok(await _servicio.ObtenerTodosAsync(incluirInactivos, pagina, tamano));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<EncargadoRespuestaDto>> ObtenerPorId(int id)
        {
            var encargado = await _servicio.ObtenerPorIdAsync(id);

            if (encargado == null)
                return NotFound();

            return Ok(encargado);
        }

        [HttpPost]
        public async Task<ActionResult<EncargadoRespuestaDto>> Crear(EncargadoCrearDto dto)
        {
            try
            {
                var creado = await _servicio.CrearAsync(dto);

                return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, EncargadoActualizarDto dto)
        {
            try
            {
                var actualizado = await _servicio.ActualizarAsync(id, dto);

                if (!actualizado)
                    return NotFound();

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensaje = ex.Message });
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