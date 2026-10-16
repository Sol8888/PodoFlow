using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones
{
    // Caso C: solo insertar y listar. No hay actualizar ni eliminar.
    public class EvaluacionRiesgoRepositorio : IEvaluacionRiesgoRepositorio
    {
        private readonly PodoFlowContext _context;

        public EvaluacionRiesgoRepositorio(PodoFlowContext context)
        {
            _context = context;
        }

        // Filtros opcionales: los riesgos de una evaluación, o las evaluaciones que analizaron un riesgo.
        public async Task<List<EvaluacionRiesgo>> ObtenerTodosAsync(long? evaluacionId, long? riesgoId, int pagina, int tamano)
        {
            var consulta = _context.EvaluacionesRiesgos.AsNoTracking();

            if (evaluacionId.HasValue)
                consulta = consulta.Where(x => x.EvId == evaluacionId.Value);

            if (riesgoId.HasValue)
                consulta = consulta.Where(x => x.RiId == riesgoId.Value);

            return await consulta
                .OrderBy(x => x.EvId)
                .ThenBy(x => x.RiId)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync();
        }

        public async Task<List<EvaluacionRiesgo>> ObtenerPorEvaluacionAsync(long evaluacionId)
        {
            return await _context.EvaluacionesRiesgos
                .AsNoTracking()
                .Where(x => x.EvId == evaluacionId)
                .ToListAsync();
        }

        public async Task<EvaluacionRiesgo?> ObtenerAsync(long evaluacionId, long riesgoId)
        {
            return await _context.EvaluacionesRiesgos
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.EvId == evaluacionId && x.RiId == riesgoId);
        }

        public async Task<EvaluacionRiesgo> CrearAsync(EvaluacionRiesgo asignacion)
        {
            _context.EvaluacionesRiesgos.Add(asignacion);

            await _context.SaveChangesAsync();

            return asignacion;
        }

        // Todas en un solo SaveChanges: o se copian todas o ninguna.
        public async Task CrearVariosAsync(List<EvaluacionRiesgo> asignaciones)
        {
            _context.EvaluacionesRiesgos.AddRange(asignaciones);

            await _context.SaveChangesAsync();
        }
    }
}
