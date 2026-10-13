
using PodoFlow.Api.DTOs.DatoContacto;

namespace PodoFlow.Api.Servicios.Interfaces;

public interface IDatoContactoServicio
{
    Task<List<DatoContactoRespuestaDto>> ListarAsync(
        long? pacienteId, bool incluirInactivos,
        int pagina, int tamano);

    Task<DatoContactoRespuestaDto?> ObtenerAsync(long id);

    Task<DatoContactoRespuestaDto> CrearAsync(
        DatoContactoCrearDto dto);

    Task<bool> ActualizarAsync(
        long id, DatoContactoActualizarDto dto);

    Task<bool> EliminarAsync(long id);

    Task<bool> ReactivarAsync(long id);
}
