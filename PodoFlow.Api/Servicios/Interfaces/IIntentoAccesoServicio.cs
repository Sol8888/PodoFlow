
using PodoFlow.Api.DTOs.IntentoAcceso;

namespace PodoFlow.Api.Servicios.Interfaces;

public interface IIntentoAccesoServicio
{
    Task<IntentoAccesoRespuestaDto> CrearAsync(
        IntentoAccesoCrearDto dto,
        string? direccionIp,
        string? agenteUsuario);

    Task<IntentoAccesoRespuestaDto?> ObtenerPorIdAsync(
        long id);

    Task<List<IntentoAccesoRespuestaDto>> ObtenerTodosAsync(
        long? usuarioId,
        bool? exitoso,
        int pagina,
        int tamano);
}
