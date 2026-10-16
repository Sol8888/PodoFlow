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
            return await _context.Database
                .SqlQuery<long>($"SELECT pa_id AS Value FROM pac.PACIENTE WHERE pa_id = {pacienteId} AND pa_estatus = 1")
                .AnyAsync();
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
            return await _context.Database
                .SqlQuery<long>($@"SELECT c.pa_id AS Value
                    FROM pri.CONSENTIMIENTO c
                    JOIN pac.PACIENTE p ON p.pa_id = c.pa_id
                    WHERE c.fi_id = {finalidadId} AND c.co_estad = 'ACEPTADO' AND p.pa_estatus = 1
                      AND NOT EXISTS (SELECT 1 FROM ana.SEUDONIMO s WHERE s.pa_id = c.pa_id)")
                .ToListAsync();
        }

        public async Task<int> ContarPacientesConSeudonimoActivoAsync(int finalidadId)
        {
            return await _context.Database
                .SqlQuery<long>($@"SELECT c.pa_id AS Value
                    FROM pri.CONSENTIMIENTO c
                    JOIN pac.PACIENTE p ON p.pa_id = c.pa_id
                    JOIN ana.SEUDONIMO s ON s.pa_id = c.pa_id AND s.se_estatus = 1
                    WHERE c.fi_id = {finalidadId} AND c.co_estad = 'ACEPTADO' AND p.pa_estatus = 1")
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
