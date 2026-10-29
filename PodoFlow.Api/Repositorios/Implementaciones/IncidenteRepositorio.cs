using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones
{
    public class IncidenteRepositorio : IIncidenteRepositorio
    {
        private readonly PodoFlowContext _context;

        public IncidenteRepositorio(PodoFlowContext context)
        {
            _context = context;
        }

        public async Task<List<Incidente>> ObtenerTodosAsync(string? estado, int pagina, int tamano)
        {
            var consulta = _context.Incidentes.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(estado))
                consulta = consulta.Where(x => x.InEstad == estado);

            return await consulta
                .OrderByDescending(x => x.InFechaDete)
                .ThenByDescending(x => x.InId)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync();
        }

        public async Task<Incidente?> ObtenerPorIdAsync(long id)
        {
            return await _context.Incidentes
                .FirstOrDefaultAsync(x => x.InId == id);
        }

        public async Task<List<AccionIncidente>> ObtenerAccionesAsync(long incidenteId)
        {
            return await _context.AccionesIncidente
                .AsNoTracking()
                .Where(x => x.InId == incidenteId)
                .OrderBy(x => x.AcId)
                .ToListAsync();
        }
        public async Task<bool> ExisteUsuarioActivoAsync(long usuarioId)
        {
            return await _context.Usuarios
                .AnyAsync(x => x.UsId == usuarioId && x.UsEstatus);
        }
        public async Task<Incidente> CrearAsync(Incidente incidente, AccionIncidente accion)
        {
            _context.Incidentes.Add(incidente);

            accion.Incidente = incidente;
            _context.AccionesIncidente.Add(accion);

            await _context.SaveChangesAsync();

            return incidente;
        }

        public async Task ActualizarAsync(Incidente incidente, byte[] versionLeida)
        {
            AplicarVersionLeida(incidente, versionLeida);

            await _context.SaveChangesAsync();
        }

        public async Task CambiarEstadoAsync(Incidente incidente, AccionIncidente accion, byte[] versionLeida)
        {
            AplicarVersionLeida(incidente, versionLeida);

            _context.AccionesIncidente.Add(accion);

            await _context.SaveChangesAsync();
        }
        public async Task<AccionIncidente> CrearAccionAsync(AccionIncidente accion)
        {
            _context.AccionesIncidente.Add(accion);

            await _context.SaveChangesAsync();

            return accion;
        }
        private void AplicarVersionLeida(Incidente incidente, byte[] versionLeida)
        {
            _context.Entry(incidente)
                .Property(x => x.InVersion)
                .OriginalValue = versionLeida;
        }
    }
}
