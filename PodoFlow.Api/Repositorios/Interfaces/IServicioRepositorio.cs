using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces;

public interface IServicioRepositorio
{
    Task<List<Servicio>> ObtenerTodosAsync();

    Task<Servicio?> ObtenerPorIdAsync(int id);

    Task<Servicio> CrearAsync(Servicio servicio);

    Task ActualizarAsync(Servicio servicio);

    Task EliminarAsync(Servicio servicio);
}

