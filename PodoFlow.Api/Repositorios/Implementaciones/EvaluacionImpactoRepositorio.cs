using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones
{
    public class EvaluacionImpactoRepositorio : IEvaluacionImpactoRepositorio
    {
        private readonly PodoFlowContext _context;

        public EvaluacionImpactoRepositorio(PodoFlowContext context)
        {
            _context = context;
        }

        // De la más reciente a la más antigua.
        public async Task<List<EvaluacionImpacto>> ObtenerTodosAsync(string? estado, int pagina, int tamano)
        {
            var consulta = _context.EvaluacionesImpacto.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(estado))
                consulta = consulta.Where(x => x.EvEstad == estado);

            return await consulta
                .OrderByDescending(x => x.EvFecha)
                .ThenByDescending(x => x.EvId)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync();
        }

        public async Task<EvaluacionImpacto?> ObtenerPorIdAsync(long id)
        {
            return await _context.EvaluacionesImpacto
                .FirstOrDefaultAsync(x => x.EvId == id);
        }

        public async Task<bool> ExisteUsuarioActivoAsync(long usuarioId)
        {
            return await _context.Usuarios
                .AnyAsync(x => x.UsId == usuarioId && x.UsEstatus);
        }

        public async Task<EvaluacionImpacto> CrearAsync(EvaluacionImpacto evaluacion)
        {
            _context.EvaluacionesImpacto.Add(evaluacion);

            await _context.SaveChangesAsync();

            return evaluacion;
        }

        public async Task GuardarCambiosAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
