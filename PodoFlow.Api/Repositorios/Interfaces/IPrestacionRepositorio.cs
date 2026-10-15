
using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces;

public interface IPrestacionRepositorio
{
    Task<List<Prestacion>> ObtenerTodosAsync(
        long? pacienteId,
        string? estado,
        int pagina,
        int tamano);

    Task<Prestacion?> ObtenerPorIdAsync(long id);

    Task<bool> PacienteActivoAsync(long id);

    Task<bool> UsuarioActivoAsync(long id);

    Task<bool> ServicioExisteAsync(int id);

    Task<Cita?> ObtenerCitaAsync(long id);

    Task<Atencion?> ObtenerAtencionAsync(long id);

    Task<Historia?> ObtenerHistoriaAsync(long id);

    Task AgregarAsync(Prestacion prestacion);

    Task GuardarCambiosAsync();
}
