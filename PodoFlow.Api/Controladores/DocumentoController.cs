using Microsoft.AspNetCore.Mvc;
using PodoFlow.Api.DTOs.Documento;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentoController : ControllerBase
    {
        private readonly IDocumentoServicio _servicio;

        public DocumentoController(IDocumentoServicio servicio)
        {
            _servicio = servicio;
        }

        // GET api/Documento?tipo=RAT&incluirInactivos=false&pagina=1&tamano=10
        [HttpGet]
        public async Task<ActionResult<List<DocumentoRespuestaDto>>> ObtenerTodos(
            [FromQuery] string? tipo = null,
            [FromQuery] bool incluirInactivos = false,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamano = 10)
        {
            if (pagina < 1 || tamano < 1 || tamano > 100)
                return BadRequest(new { mensaje = "Paginación inválida." });

            try
            {
                return Ok(await _servicio.ObtenerTodosAsync(tipo, incluirInactivos, pagina, tamano));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // Devuelve el registro esté activo o no.
        [HttpGet("{id:long}")]
        public async Task<ActionResult<DocumentoRespuestaDto>> ObtenerPorId(long id)
        {
            var documento = await _servicio.ObtenerPorIdAsync(id);

            if (documento == null)
                return NotFound();

            return Ok(documento);
        }

        [HttpPost]
        public async Task<ActionResult<DocumentoRespuestaDto>> Crear(DocumentoCrearDto dto)
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

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Actualizar(long id, DocumentoActualizarDto dto)
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

        // Baja lógica: do_estatus = 0.
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Eliminar(long id)
        {
            var eliminado = await _servicio.EliminarAsync(id);

            if (!eliminado)
                return NotFound();

            return NoContent();
        }

        // do_estatus = 1.
        [HttpPatch("{id:long}/reactivar")]
        public async Task<IActionResult> Reactivar(long id)
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
