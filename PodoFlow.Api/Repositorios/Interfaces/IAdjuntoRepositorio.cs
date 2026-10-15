
using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces;

public interface IAdjuntoRepositorio
{
    Task<List<Adjunto>> ObtenerTodosAsync(
        long? atencionId,
        bool incluirInactivos,
        int pagina,
        int tamano);

    Task<Adjunto?> ObtenerPorIdAsync(long id);

    Task<Adjunto?> ObtenerPorRutaAsync(string ruta);

    Task<Atencion?> ObtenerAtencionAsync(long id);

    Task<Historia?> ObtenerHistoriaAsync(long id);

    Task<bool> PacienteActivoAsync(long id);

    Task<bool> UsuarioActivoAsync(long id);

    Task<Adjunto> CrearAsync(Adjunto adjunto);

    Task GuardarCambiosAsync();
}
