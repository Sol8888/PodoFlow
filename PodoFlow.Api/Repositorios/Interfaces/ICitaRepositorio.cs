
using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces;

public interface ICitaRepositorio
{
    Task<List<Cita>> ObtenerTodosAsync(
        long? pacienteId,
        long? usuarioId,
        string? estado,
        int pagina,
        int tamano);

    Task<Cita?> ObtenerPorIdAsync(long id);

    Task<bool> PacienteActivoAsync(long id);
    Task<bool> ServicioActivoAsync(int id);
    Task<bool> UsuarioActivoAsync(long id);

    Task<bool> DisponibleAsync(
        long? usuarioId,
        DateTime inicioUtc,
        DateTime finUtc,
        long? excluirCitaId = null);

    Task<Cita> CrearAsync(Cita cita);
    Task GuardarCambiosAsync();

    void EstablecerVersionOriginal(Cita cita, byte[] version);
}

