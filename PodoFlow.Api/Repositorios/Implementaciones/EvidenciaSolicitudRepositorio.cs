using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones
{
    public class EvidenciaSolicitudRepositorio : IEvidenciaSolicitudRepositorio
    {
        private readonly PodoFlowContext _context;

        public EvidenciaSolicitudRepositorio(PodoFlowContext context)
        {
            _context = context;
        }

        public async Task<List<EvidenciaSolicitud>> ObtenerPorSolicitudAsync(long solicitudId)
        {
            return await _context.EvidenciasSolicitud
                .AsNoTracking()
                .Where(x => x.SoId == solicitudId)
                .OrderBy(x => x.EvId)
                .ToListAsync();
        }

        public async Task<EvidenciaSolicitud?> ObtenerPorIdAsync(long id)
        {
            return await _context.EvidenciasSolicitud
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.EvId == id);
        }
        public async Task<EvidenciaSolicitud> CrearAsync(EvidenciaSolicitud evidencia, AccionSolicitud accion)
        {
            _context.EvidenciasSolicitud.Add(evidencia);
            _context.AccionesSolicitud.Add(accion);

            await _context.SaveChangesAsync();

            return evidencia;
        }
    }
}