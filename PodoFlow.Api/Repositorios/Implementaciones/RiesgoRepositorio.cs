using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones
{
    public class RiesgoRepositorio : IRiesgoRepositorio
    {
        private readonly PodoFlowContext _context;

        public RiesgoRepositorio(PodoFlowContext context)
        {
            _context = context;
        }

        // Del nivel más alto al más bajo: lo primero que hay que atender.
        public async Task<List<Riesgo>> ObtenerTodosAsync(int? tratamientoId, string? estado, int pagina, int tamano)
        {
            var consulta = _context.Riesgos.AsNoTracking();

            if (tratamientoId.HasValue)
                consulta = consulta.Where(x => x.TrId == tratamientoId.Value);

            if (!string.IsNullOrWhiteSpace(estado))
                consulta = consulta.Where(x => x.RiEstad == estado);

            return await consulta
                .OrderByDescending(x => x.RiNivel)
                .ThenBy(x => x.RiId)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync();
        }

        public async Task<Riesgo?> ObtenerPorIdAsync(long id)
        {
            return await _context.Riesgos
                .FirstOrDefaultAsync(x => x.RiId == id);
        }

        public async Task<Riesgo> CrearAsync(Riesgo riesgo)
        {
            _context.Riesgos.Add(riesgo);

            await _context.SaveChangesAsync();

            return riesgo;
        }

        public async Task GuardarCambiosAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
