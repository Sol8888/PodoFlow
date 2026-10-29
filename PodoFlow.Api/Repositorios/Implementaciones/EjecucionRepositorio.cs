using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones
{
    public class EjecucionRepositorio : IEjecucionRepositorio
    {
        private readonly PodoFlowContext _context;

        public EjecucionRepositorio(PodoFlowContext context)
        {
            _context = context;
        }

        public async Task<List<Ejecucion>> ObtenerTodosAsync(string? estado, int pagina, int tamano)
        {
            var consulta = _context.Ejecuciones.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(estado))
                consulta = consulta.Where(x => x.EjEstad == estado);

            return await consulta
                .OrderByDescending(x => x.EjFechaHora)
                .ThenByDescending(x => x.EjId)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync();
        }

        public async Task<Ejecucion?> ObtenerPorIdAsync(long id)
        {
            return await _context.Ejecuciones
                .FirstOrDefaultAsync(x => x.EjId == id);
        }

        public async Task<List<DatoAnalitico>> ObtenerDatosAsync(long ejecucionId, int pagina, int tamano)
        {
            return await _context.DatosAnaliticos
                .AsNoTracking()
                .Where(x => x.EjId == ejecucionId)
                .OrderBy(x => x.DaId)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync();
        }

        public async Task<int> ContarDatosAsync(long ejecucionId)
        {
            return await _context.DatosAnaliticos
                .CountAsync(x => x.EjId == ejecucionId);
        }

        public async Task<List<Guid>> ObtenerSeudonimosCargadosAsync(long ejecucionId, List<Guid> seudonimos)
        {
            return await _context.DatosAnaliticos
                .AsNoTracking()
                .Where(x => x.EjId == ejecucionId && seudonimos.Contains(x.SeId))
                .Select(x => x.SeId)
                .ToListAsync();
        }

        public async Task<List<Guid>> ObtenerSeudonimosAutorizadosAsync(List<Guid> seudonimos, int finalidadId)
        {
            return await _context.Seudonimos
                .AsNoTracking()
                .Where(s => seudonimos.Contains(s.SeId) && s.SeEstatus)
                .Join(
                    _context.Consentimientos.Where(c => c.FiId == finalidadId && c.CoEstad == "ACEPTADO"),
                    s => s.PaId,
                    c => c.PaId,
                    (s, c) => s.SeId)
                .ToListAsync();
        }

        public async Task<List<Recomendacion>> ObtenerRecomendacionesAsync(long ejecucionId)
        {
            return await _context.Recomendaciones
                .AsNoTracking()
                .Where(x => x.EjId == ejecucionId)
                .OrderBy(x => x.ReId)
                .ToListAsync();
        }

        public async Task<bool> ExisteUsuarioActivoAsync(long usuarioId)
        {
            return await _context.Usuarios
                .AnyAsync(x => x.UsId == usuarioId && x.UsEstatus);
        }

        public async Task<Ejecucion> CrearAsync(Ejecucion ejecucion)
        {
            _context.Ejecuciones.Add(ejecucion);

            await _context.SaveChangesAsync();

            return ejecucion;
        }

        public async Task CrearDatosAsync(List<DatoAnalitico> datos)
        {
            _context.DatosAnaliticos.AddRange(datos);

            await _context.SaveChangesAsync();
        }

        public async Task<Recomendacion> CrearRecomendacionAsync(Recomendacion recomendacion)
        {
            _context.Recomendaciones.Add(recomendacion);

            await _context.SaveChangesAsync();

            return recomendacion;
        }

        public async Task GuardarCambiosAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
