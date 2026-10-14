using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones
{
    public class ExportacionRepositorio : IExportacionRepositorio
    {
        private readonly PodoFlowContext _context;

        public ExportacionRepositorio(PodoFlowContext context)
        {
            _context = context;
        }

        public async Task<List<Exportacion>> ObtenerTodosAsync(int pagina, int tamano)
        {
            return await _context.Exportaciones
                .AsNoTracking()
                .OrderByDescending(x => x.ExFechaGene)
                .ThenByDescending(x => x.ExId)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync();
        }

        public async Task<List<Exportacion>> ObtenerPorSolicitudAsync(long solicitudId)
        {
            return await _context.Exportaciones
                .AsNoTracking()
                .Where(x => x.SoId == solicitudId)
                .OrderBy(x => x.ExId)
                .ToListAsync();
        }

        public async Task<Exportacion?> ObtenerPorIdAsync(long id)
        {
            return await _context.Exportaciones
                .FirstOrDefaultAsync(x => x.ExId == id);
        }

        public async Task<Exportacion> CrearAsync(Exportacion exportacion, AccionSolicitud accion)
        {
            _context.Exportaciones.Add(exportacion);
            _context.AccionesSolicitud.Add(accion);

            await _context.SaveChangesAsync();

            return exportacion;
        }

        public async Task CambiarEstadoAsync(AccionSolicitud accion)
        {
            _context.AccionesSolicitud.Add(accion);

            await _context.SaveChangesAsync();
        }
    }
}
