using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodoFlow.Api.DTOs.CategoriaDato;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores
{
    [ApiController]
    [Authorize(Policy = "PrivacidadGestionar")]
    [Route("api/[controller]")]
    public class CategoriaDatoController : ControllerBase
    {
        private readonly ICategoriaDatoServicio _servicio;

        public CategoriaDatoController(ICategoriaDatoServicio servicio)
        {
            _servicio = servicio;
        }

        [HttpGet]
        public async Task<ActionResult<List<CategoriaDatoRespuestaDto>>> ObtenerTodos(
            [FromQuery] bool incluirInactivos = false,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamano = 10)
        {
            if (pagina < 1 || tamano < 1 || tamano > 100)
                return BadRequest(new { mensaje = "Paginación inválida." });

            return Ok(await _servicio.ObtenerTodosAsync(incluirInactivos, pagina, tamano));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CategoriaDatoRespuestaDto>> ObtenerPorId(int id)
        {
            var categoria = await _servicio.ObtenerPorIdAsync(id);

            if (categoria == null)
                return NotFound();

            return Ok(categoria);
        }

        [HttpPost]
        public async Task<ActionResult<CategoriaDatoRespuestaDto>> Crear(CategoriaDatoCrearDto dto)
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
        public async Task<IActionResult> Actualizar(int id, CategoriaDatoActualizarDto dto)
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
            var reactivado = await _servicio.ReactivarAsync(id);

            if (!reactivado)
                return NotFound();

            return NoContent();
        }
    }
}