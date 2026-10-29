using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones
{
    public class SolicitudRepositorio : ISolicitudRepositorio

    {
        private readonly PodoFlowContext _context;

        public SolicitudRepositorio(PodoFlowContext context)
        {
            _context = context;
        }

        public async Task<List<Solicitud>> ObtenerTodosAsync(int pagina, int tamano)
        {
            return await _context.Solicitudes
                .AsNoTracking()
                .OrderByDescending(x => x.SoFechaRece)
                .ThenByDescending(x => x.SoId)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync();
        }
        public async Task<List<Solicitud>> ObtenerPorPacienteAsync(long pacienteId)
        {
            return await _context.Solicitudes
                .AsNoTracking()
                .Where(x => x.PaId == pacienteId)
                .OrderByDescending(x => x.SoFechaRece)
                .ToListAsync();
        }

        public async Task<Solicitud?> ObtenerPorIdAsync(long id)
        {
            return await _context.Solicitudes
                .FirstOrDefaultAsync(x => x.SoId == id);
        }

        public async Task<List<AccionSolicitud>> ObtenerAccionesAsync(long solicitudId)
        {
            return await _context.AccionesSolicitud
                .AsNoTracking()
                .Where(x => x.SoId == solicitudId)
                .OrderBy(x => x.AcId)
                .ToListAsync();
        }

        public async Task<bool> ExistePacienteActivoAsync(long pacienteId)
        {
            return await _context.Pacientes
                .AnyAsync(x => x.PaId == pacienteId && x.PaEstatus);
        }

        public async Task<bool> ExisteUsuarioActivoAsync(long usuarioId)
        {
            return await _context.Usuarios
                .AnyAsync(x => x.UsId == usuarioId && x.UsEstatus);
        }

        public async Task<bool> ExisteExportacionEntregadaAsync(long solicitudId)
        {
            return await _context.Exportaciones
                .AnyAsync(x => x.SoId == solicitudId && x.ExEstad == "ENTREGADA");
        }
        public async Task<(string TipoIdentificacion, string Identificacion, string Nombre)?> ObtenerIdentidadPacienteAsync(long pacienteId)
        {
            var fila = await _context.Pacientes
                .AsNoTracking()
                .Where(x => x.PaId == pacienteId)
                .Select(x => new
                {
                    x.PaTipoIdent,
                    x.PaIdent,
                    Nombre = x.PaNombres + " " + x.PaApelli
                })
                .FirstOrDefaultAsync();

            if (fila == null)
                return null;

            return (fila.PaTipoIdent, fila.PaIdent, fila.Nombre);
        }
        public async Task<Solicitud> CrearAsync(Solicitud solicitud, AccionSolicitud? accion)
        {
            _context.Solicitudes.Add(solicitud);

            if (accion != null)
            {
                accion.Solicitud = solicitud;
                _context.AccionesSolicitud.Add(accion);
            }

            await _context.SaveChangesAsync();

            return solicitud;
        }

        public async Task ActualizarAsync(Solicitud solicitud, byte[] versionLeida)
        {
            AplicarVersionLeida(solicitud, versionLeida);

            await _context.SaveChangesAsync();
        }

        public async Task CambiarEstadoAsync(Solicitud solicitud, AccionSolicitud accion, byte[] versionLeida)
        {
            AplicarVersionLeida(solicitud, versionLeida);

            _context.AccionesSolicitud.Add(accion);

            await _context.SaveChangesAsync();
        }
        private void AplicarVersionLeida(Solicitud solicitud, byte[] versionLeida)
        {
            _context.Entry(solicitud)
                .Property(x => x.SoVersion)
                .OriginalValue = versionLeida;
        }
    }

}