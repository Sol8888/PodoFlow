using Microsoft.EntityFrameworkCore; 
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces; 

namespace PodoFlow.Api.Repositorios.Implementaciones
{
    public class ConsentimientoRepositorio : IConsentimientoRepositorio
    {
        private readonly PodoFlowContext _context;

        public ConsentimientoRepositorio(PodoFlowContext context)
        {
            _context = context;
        }

        public async Task<List<Consentimiento>> ObtenerTodosAsync(int pagina, int tamano)
        {
            return await _context.Consentimientos
                .AsNoTracking()
                .OrderByDescending(x => x.CoFechaObte)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync();
        }

        public async Task<List<Consentimiento>> ObtenerPorPacienteAsync(long pacienteId)
        {
            return await _context.Consentimientos
                .AsNoTracking()
                .Where(x => x.PaId == pacienteId)
                .OrderBy(x => x.FiId)
                .ToListAsync();
        }

        public async Task<Consentimiento?> ObtenerPorIdAsync(long id)
        {
            return await _context.Consentimientos
                .FirstOrDefaultAsync(x => x.CoId == id);
        }
        public async Task<bool> ExistePorPacienteYFinalidadAsync(long pacienteId, int finalidadId)
        {
            return await _context.Consentimientos
                .AnyAsync(x => x.PaId == pacienteId && x.FiId == finalidadId);
        }
        public async Task<bool> ExistePacienteActivoAsync(long pacienteId)
        {
            return await _context.Database
                .SqlQuery<long>($"SELECT pa_id AS Value FROM pac.PACIENTE WHERE pa_id = {pacienteId} AND pa_estatus = 1")
                .AnyAsync();
        }

    
        public async Task<Consentimiento> CrearAsync(Consentimiento consentimiento, EventoConsentimiento evento)
        {
            evento.Consentimiento = consentimiento;

            _context.Consentimientos.Add(consentimiento);
            _context.EventosConsentimiento.Add(evento);

            await _context.SaveChangesAsync();

            return consentimiento;
        }

        public async Task ActualizarAsync(Consentimiento consentimiento, byte[] versionLeida)
        {
            AplicarVersionLeida(consentimiento, versionLeida);

            await _context.SaveChangesAsync();
        }

        public async Task CambiarEstadoAsync(Consentimiento consentimiento, EventoConsentimiento evento, byte[] versionLeida)
        {
            AplicarVersionLeida(consentimiento, versionLeida);

            _context.EventosConsentimiento.Add(evento);

            await _context.SaveChangesAsync();
        }

       
        private void AplicarVersionLeida(Consentimiento consentimiento, byte[] versionLeida)
        {
            _context.Entry(consentimiento)
                .Property(x => x.CoVersion)
                .OriginalValue = versionLeida;
        }

        public async Task<List<EventoConsentimiento>> ObtenerEventosAsync(long consentimientoId)
        {
            return await _context.EventosConsentimiento
                .AsNoTracking()
                .Where(x => x.CoId == consentimientoId)
                .OrderBy(x => x.EvId)
                .ToListAsync();
        }
    }
}
