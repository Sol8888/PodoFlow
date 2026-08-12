using PodoFlow.Api.DTOs.Servicio;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;


namespace PodoFlow.Api.Servicios.Implementaciones;

public class ServicioServicio : IServicioServicio
{
    private readonly IServicioRepositorio _repositorio;

    public ServicioServicio(IServicioRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<List<ServicioRespuestaDto>> ObtenerTodosAsync()
    {
        var servicios = await _repositorio.ObtenerTodosAsync();

        return servicios.Select(MapearRespuesta).ToList();
    }

    public async Task<ServicioRespuestaDto?> ObtenerPorIdAsync(int id)
    {
        var servicio = await _repositorio.ObtenerPorIdAsync(id);

        if (servicio == null)
            return null;

        return MapearRespuesta(servicio);
    }

    public async Task<ServicioRespuestaDto> CrearAsync(
        ServicioCrearDto dto)
    {
        var servicio = new Servicio
        {
            SeNombre = dto.Nombre.Trim(),
            SeDescri = dto.Descripcion?.Trim(),
            SePrecio = dto.Precio,
            SeDuracMin = dto.DuracionMinutos,
            SeFechaCrea = DateTime.UtcNow,
            SeEstatus = true
        };

        var creado = await _repositorio.CrearAsync(servicio);

        return MapearRespuesta(creado);
    }

    public async Task<bool> ActualizarAsync(
        int id,
        ServicioActualizarDto dto)
    {
        var servicio = await _repositorio.ObtenerPorIdAsync(id);

        if (servicio == null)
            return false;

        servicio.SeNombre = dto.Nombre.Trim();
        servicio.SeDescri = dto.Descripcion?.Trim();
        servicio.SePrecio = dto.Precio;
        servicio.SeDuracMin = dto.DuracionMinutos;
        servicio.SeEstatus = dto.Estatus;
        servicio.SeFechaModi = DateTime.UtcNow;

        await _repositorio.ActualizarAsync(servicio);

        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var servicio = await _repositorio.ObtenerPorIdAsync(id);

        if (servicio == null)
            return false;

        await _repositorio.EliminarAsync(servicio);

        return true;
    }

    private static ServicioRespuestaDto MapearRespuesta(
        Servicio servicio)
    {
        return new ServicioRespuestaDto
        {
            Id = servicio.SeId,
            Nombre = servicio.SeNombre,
            Descripcion = servicio.SeDescri,
            Precio = servicio.SePrecio,
            DuracionMinutos = servicio.SeDuracMin,
            Estatus = servicio.SeEstatus
        };
    }
}
