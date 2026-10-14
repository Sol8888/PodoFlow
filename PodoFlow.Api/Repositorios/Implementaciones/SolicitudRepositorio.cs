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

        public async Task<List<Solicitud>> ObtenerTodosAsync()
        {
            return await _context.Solicitudes
                .AsNoTracking()
                .OrderByDescending(x => x.SoFechaRece)
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
            return await _context.Database
                .SqlQuery<long>($"SELECT pa_id AS Value FROM pac.PACIENTE WHERE pa_id = {pacienteId} AND pa_estatus = 1")
                .AnyAsync();
        }

        public async Task<bool> ExisteUsuarioActivoAsync(long usuarioId)
        {
            return await _context.Database
                .SqlQuery<long>($"SELECT us_id AS Value FROM seg.USUARIO WHERE us_id = {usuarioId} AND us_estatus = 1")
                .AnyAsync();
        }
        // Esto tal vez sea mejor en un SP
        public async Task<(string TipoIdentificacion, string Identificacion, string Nombre)?> ObtenerIdentidadPacienteAsync(long pacienteId)
        {
            var fila = await _context.Database
                .SqlQuery<IdentidadPacienteFila>($"""
                    SELECT pa_tipo_ident                      AS TipoIdentificacion,
                           pa_ident                           AS Identificacion,
                           CONCAT(pa_nombres, ' ', pa_apelli) AS Nombre
                    FROM pac.PACIENTE
                    WHERE pa_id = {pacienteId}
                    """)
                .FirstOrDefaultAsync();

            if (fila == null)
                return null;

            return (fila.TipoIdentificacion, fila.Identificacion, fila.Nombre);
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
        private sealed class IdentidadPacienteFila
        {
            public string TipoIdentificacion { get; set; } = string.Empty;

            public string Identificacion { get; set; } = string.Empty;

            public string Nombre { get; set; } = string.Empty;
        }
    }

}