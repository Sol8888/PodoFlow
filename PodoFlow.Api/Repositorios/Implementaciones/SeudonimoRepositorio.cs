using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones
{
    public class SeudonimoRepositorio : ISeudonimoRepositorio
    {
        private readonly PodoFlowContext _context;

        public SeudonimoRepositorio(PodoFlowContext context)
        {
            _context = context;
        }

        public async Task<List<Seudonimo>> ObtenerTodosAsync(bool incluirInactivos, int pagina, int tamano)
        {
            var consulta = _context.Seudonimos.AsNoTracking();

            if (!incluirInactivos)
                consulta = consulta.Where(x => x.SeEstatus);

            return await consulta
                .OrderBy(x => x.PaId)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync();
        }

        public async Task<Seudonimo?> ObtenerPorIdAsync(Guid id)
        {
            return await _context.Seudonimos
                .FirstOrDefaultAsync(x => x.SeId == id);
        }

        public async Task<Seudonimo?> ObtenerPorPacienteAsync(long pacienteId)
        {
            return await _context.Seudonimos
                .FirstOrDefaultAsync(x => x.PaId == pacienteId);
        }

        public async Task<bool> ExistePacienteActivoAsync(long pacienteId)
        {
            return await _context.Pacientes
                .AnyAsync(x => x.PaId == pacienteId && x.PaEstatus);
        }

        public async Task<string?> ObtenerEstadoConsentimientoAsync(long pacienteId, int finalidadId)
        {
            return await _context.Consentimientos
                .AsNoTracking()
                .Where(x => x.PaId == pacienteId && x.FiId == finalidadId)
                .Select(x => x.CoEstad)
                .FirstOrDefaultAsync();
        }

        public async Task<List<long>> ObtenerPacientesSinSeudonimoAsync(int finalidadId)
        {
            return await _context.Consentimientos
                .AsNoTracking()
                .Where(c => c.FiId == finalidadId && c.CoEstad == "ACEPTADO")
                .Where(c => _context.Pacientes.Any(p => p.PaId == c.PaId && p.PaEstatus))
                .Where(c => !_context.Seudonimos.Any(s => s.PaId == c.PaId))
                .Select(c => c.PaId)
                .ToListAsync();
        }

        public async Task<int> ContarPacientesConSeudonimoActivoAsync(int finalidadId)
        {
            return await _context.Consentimientos
                .AsNoTracking()
                .Where(c => c.FiId == finalidadId && c.CoEstad == "ACEPTADO")
                .Where(c => _context.Pacientes.Any(p => p.PaId == c.PaId && p.PaEstatus))
                .Join(_context.Seudonimos.Where(s => s.SeEstatus), c => c.PaId, s => s.PaId, (c, s) => c.PaId)
                .CountAsync();
        }

        public async Task<Seudonimo> CrearAsync(Seudonimo seudonimo)
        {
            _context.Seudonimos.Add(seudonimo);

            await _context.SaveChangesAsync();

            return seudonimo;
        }
        public async Task CrearVariosAsync(List<Seudonimo> seudonimos)
        {
            _context.Seudonimos.AddRange(seudonimos);

            await _context.SaveChangesAsync();
        }

        public async Task GuardarCambiosAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
