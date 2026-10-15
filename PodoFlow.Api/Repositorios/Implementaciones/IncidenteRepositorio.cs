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

        // Caso B: no hay estatus, así que no aplica incluirInactivos. Filtro opcional por estado y paginación.
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

        // seg.USUARIO es del bloque de la Persona 1: solo se lee.
        public async Task<bool> ExisteUsuarioActivoAsync(long usuarioId)
        {
            return await _context.Database
                .SqlQuery<long>($"SELECT us_id AS Value FROM seg.USUARIO WHERE us_id = {usuarioId} AND us_estatus = 1")
                .AnyAsync();
        }

        // Incidente y su primera acción se guardan juntos: o se graban los dos o ninguno.
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

        // Acción manual: no toca la fila del incidente, por eso no necesita versión.
        public async Task<AccionIncidente> CrearAccionAsync(AccionIncidente accion)
        {
            _context.AccionesIncidente.Add(accion);

            await _context.SaveChangesAsync();

            return accion;
        }

        // Si otro usuario modificó el incidente, el UPDATE no afecta filas y EF lanza DbUpdateConcurrencyException.
        private void AplicarVersionLeida(Incidente incidente, byte[] versionLeida)
        {
            _context.Entry(incidente)
                .Property(x => x.InVersion)
                .OriginalValue = versionLeida;
        }
    }
}
