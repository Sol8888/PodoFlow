using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Repositorios.Interfaces;

public interface IFinalidadRepositorio
{
    Task<List<Finalidad>> ObtenerTodosAsync();

    Task<Finalidad?> ObtenerPorIdAsync(int id);

    Task<Finalidad?> ObtenerPorCodigoAsync(string codigo);

    Task<Finalidad> CrearAsync(Finalidad finalidad);

    Task ActualizarAsync(Finalidad finalidad);

    Task EliminarAsync(Finalidad finalidad);
    }

