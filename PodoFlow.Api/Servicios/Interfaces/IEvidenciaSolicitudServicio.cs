using PodoFlow.Api.DTOs.EvidenciaSolicitud;

namespace PodoFlow.Api.Servicios.Interfaces
{
    public interface IEvidenciaSolicitudServicio
    {
        Task<List<EvidenciaSolicitudRespuestaDto>?> ObtenerPorSolicitudAsync(long solicitudId);

        Task<EvidenciaSolicitudRespuestaDto?> ObtenerPorIdAsync(long id);

        Task<EvidenciaSolicitudRespuestaDto> CrearAsync(EvidenciaSolicitudCrearDto dto);
    }
}
