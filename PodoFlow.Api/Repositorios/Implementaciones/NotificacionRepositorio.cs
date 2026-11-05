using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones
{
    public class NotificacionRepositorio : INotificacionRepositorio
    {
        private readonly PodoFlowContext _context;

        public NotificacionRepositorio(PodoFlowContext context)
        {
            _context = context;
        }

        public async Task<List<Notificacion>> ObtenerTodosAsync(long? pacienteId, string? estado, int pagina, int tamano)
        {
            var consulta = _context.Notificaciones.AsNoTracking();

            if (pacienteId.HasValue)
                consulta = consulta.Where(x => x.PaId == pacienteId.Value);

            if (!string.IsNullOrWhiteSpace(estado))
                consulta = consulta.Where(x => x.NoEstad == estado);

            return await consulta
                .OrderByDescending(x => x.NoFechaProg)
                .ThenByDescending(x => x.NoId)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync();
        }

        public async Task<List<Notificacion>> ObtenerPorEnviarAsync(DateTime hasta, int tamano)
        {
            return await _context.Notificaciones
                .AsNoTracking()
                .Where(x => x.NoEstad == "PENDIENTE" && x.NoFechaProg <= hasta)
                .OrderBy(x => x.NoFechaProg)
                .ThenBy(x => x.NoId)
                .Take(tamano)
                .ToListAsync();
        }

        public async Task<Notificacion?> ObtenerPorIdAsync(long id)
        {
            return await _context.Notificaciones
                .FirstOrDefaultAsync(x => x.NoId == id);
        }

        public async Task<List<Envio>> ObtenerEnviosAsync(long notificacionId)
        {
            return await _context.Envios
                .AsNoTracking()
                .Where(x => x.NoId == notificacionId)
                .OrderBy(x => x.EnNumInten)
                .ToListAsync();
        }
        public async Task<DateTime?> ObtenerFechaCitaVigenteAsync(long citaId, long pacienteId)
        {
            var estadosVigentes = new[] { "SOLICITADA", "CONFIRMADA", "REPROGRAMADA" };

            return await _context.Citas
                .AsNoTracking()
                .Where(x => x.CiId == citaId && x.PaId == pacienteId && estadosVigentes.Contains(x.CiEstad))
                .Select(x => (DateTime?)x.CiFechaIni)
                .FirstOrDefaultAsync();
        }
        public async Task<bool> ExisteUsuarioActivoAsync(long usuarioId)
        {
            return await _context.Usuarios
                .AnyAsync(x => x.UsId == usuarioId && x.UsEstatus);
        }

        public async Task<Notificacion> CrearAsync(Notificacion notificacion)
        {
            _context.Notificaciones.Add(notificacion);

            await _context.SaveChangesAsync();

            return notificacion;
        }

        public async Task ActualizarAsync(Notificacion notificacion, byte[] versionLeida)
        {
            AplicarVersionLeida(notificacion, versionLeida);

            await _context.SaveChangesAsync();
        }

      
        public async Task<Envio> RegistrarEnvioAsync(Notificacion notificacion, Envio envio)
        {
            var ultimo = await _context.Envios
                .Where(x => x.NoId == notificacion.NoId)
                .Select(x => (short?)x.EnNumInten)
                .MaxAsync();

            envio.EnNumInten = (short)((ultimo ?? 0) + 1);

            _context.Envios.Add(envio);

            await _context.SaveChangesAsync();

            return envio;
        }

        // Si otro usuario modificó la notificación, el UPDATE no afecta filas
        private void AplicarVersionLeida(Notificacion notificacion, byte[] versionLeida)
        {
            _context.Entry(notificacion)
                .Property(x => x.NoVersion)
                .OriginalValue = versionLeida;
        }
    }
}
