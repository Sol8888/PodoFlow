using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces
{
    public interface IEvidenciaSolicitudRepositorio
    {
        Task<List<EvidenciaSolicitud>> ObtenerPorSolicitudAsync(long solicitudId);

        Task<EvidenciaSolicitud?> ObtenerPorIdAsync(long id);

        Task<EvidenciaSolicitud> CrearAsync(EvidenciaSolicitud evidencia, AccionSolicitud accion);
    }
}