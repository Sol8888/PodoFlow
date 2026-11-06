
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.DTOs.Adjunto;
using PodoFlow.Api.Servicios.Almacenamiento;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Controladores;

[ApiController]
[Authorize(Policy = "HistoriaClinicaGestionar")]
[Route("api/adjuntos")]
public class AdjuntoController : ControllerBase
{
    private readonly IAdjuntoServicio _servicio;

    public AdjuntoController(IAdjuntoServicio servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos(
        [FromQuery] long? atencionId = null,
        [FromQuery] bool incluirInactivos = false,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 10)
    {
        try
        {
            var datos = await _servicio.ObtenerTodosAsync(
                atencionId, incluirInactivos, pagina, tamano);

            return Ok(datos);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> ObtenerPorId(long id)
    {
        var adjunto = await _servicio.ObtenerPorIdAsync(id);

        if (adjunto == null)
            return NotFound("No se encontró el adjunto.");

        return Ok(adjunto);
    }


    [HttpPost]
    public IActionResult Crear()
    {
        return StatusCode(
            StatusCodes.Status410Gone,
            "Para registrar un adjunto utiliza " +
            "POST /api/adjuntos/archivos/atencion/{atencionId}.");
    }

    [HttpPost("archivos/atencion/{atencionId:long}")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(AlmacenamientoAdjuntos.MaximoBytes + 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = AlmacenamientoAdjuntos.MaximoBytes + 1024 * 1024)]
    public async Task<IActionResult> Subir(
        long atencionId,
        [FromForm] AdjuntoSubirDto dto,
        CancellationToken ct)
    {
        try
        {
            var adjunto = await _servicio.SubirAsync(atencionId, dto, ct);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = adjunto.Id },
                adjunto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
        catch (DbUpdateException)
        {
            return Conflict(new { mensaje = "No se pudo registrar el adjunto." });
        }
    }

    [HttpGet("{id:long}/archivo")]
    public async Task<IActionResult> Descargar(long id, CancellationToken ct)
    {
        try
        {
            var archivo = await _servicio.AbrirArchivoAsync(id, ct);

            if (archivo == null)
                return NotFound(new { mensaje = "No se encontró el archivo." });

            return File(archivo.Contenido, archivo.TipoMime, archivo.Nombre);
        }
        catch (InvalidDataException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{id:long}")]
    public IActionResult Actualizar(long id)
    {
        return StatusCode(
            StatusCodes.Status410Gone,
            "La actualización directa de metadatos está deshabilitada.");
    }


    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Eliminar(long id)
    {
        try
        {
            var eliminado = await _servicio.EliminarAsync(id);

            return eliminado ? NoContent() : NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (DbUpdateException)
        {
            return Conflict("No se pudo desactivar el adjunto.");
        }
    }

    [HttpPatch("{id:long}/reactivar")]
    public async Task<IActionResult> Reactivar(long id)
    {
        try
        {
            var reactivado = await _servicio.ReactivarAsync(id);

            return reactivado ? NoContent() : NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (DbUpdateException)
        {
            return Conflict("No se pudo reactivar el adjunto.");
        }
    }
}
