
using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces;

public interface IAtencionRepositorio
{
    Task<List<Atencion>> ObtenerTodosAsync(
        long? historiaId, string? estado,
        int pagina, int tamano);

    Task<Atencion?> ObtenerPorIdAsync(long id);

    Task<Historia?> ObtenerHistoriaAsync(long id);

    Task<Cita?> ObtenerCitaAsync(long id);

    Task<bool> PacienteActivoAsync(long id);

    Task<bool> UsuarioActivoAsync(long id);

    Task<bool> CitaAsociadaAsync(long citaId);

    Task<Atencion> CrearAsync(Atencion atencion);

    Task GuardarCambiosAsync();

    void EstablecerVersionOriginal(
        Atencion atencion, byte[] version);
}
