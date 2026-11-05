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

        public async Task<bool> ExisteEvidenciaAsync(long solicitudId)
        {
            return await _context.EvidenciasSolicitud
                .AnyAsync(x => x.SoId == solicitudId);
        }

        public async Task<List<Dictionary<string, object?>>> ObtenerDatosTitularAsync(string conjunto, long pacienteId)
        {
            var consulta = conjunto switch
            {
                "paciente" => ConsultarPaciente(pacienteId),
                "contactos" => ConsultarContactos(pacienteId),
                "citas" => ConsultarCitas(pacienteId),
                "consentimientos" => ConsultarConsentimientos(pacienteId),
                "eventosConsentimiento" => ConsultarEventosConsentimiento(pacienteId),
                "opcionesComunicacion" => ConsultarOpcionesComunicacion(pacienteId),
                "notificaciones" => ConsultarNotificaciones(pacienteId),
                "envios" => ConsultarEnvios(pacienteId),
                "solicitudes" => ConsultarSolicitudes(pacienteId),
                "historia" => ConsultarHistoria(pacienteId),
                "antecedentes" => ConsultarAntecedentes(pacienteId),
                "atenciones" => ConsultarAtenciones(pacienteId),
                "evoluciones" => ConsultarEvoluciones(pacienteId),
                "adjuntos" => ConsultarAdjuntos(pacienteId),
                _ => throw new ArgumentException($"No existe el conjunto de datos {conjunto}.")
            };

            return await consulta.ToListAsync();
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

        public async Task RegistrarAccionAsync(AccionSolicitud accion)
        {
            _context.AccionesSolicitud.Add(accion);

            await _context.SaveChangesAsync();
        }

        private IQueryable<Dictionary<string, object?>> ConsultarPaciente(long pacienteId)
        {
            return _context.Pacientes
                .AsNoTracking()
                .Where(x => x.PaId == pacienteId)
                .Select(x => new Dictionary<string, object?>
                {
                    { "id", x.PaId },
                    { "tipoIdentificacion", x.PaTipoIdent },
                    { "identificacion", x.PaIdent },
                    { "nombres", x.PaNombres },
                    { "apellidos", x.PaApelli },
                    { "fechaNacimiento", Dia(x.PaFechaNaci) },
                    { "genero", x.PaGenero },
                    { "direccion", x.PaDirec },
                    { "fechaRegistro", Utc(x.PaFechaCrea) },
                    { "fechaModificacion", Utc(x.PaFechaModi) },
                    { "fechaBaja", Utc(x.PaFechaBaja) },
                    { "motivoBaja", x.PaMotivBaja },
                    { "activo", x.PaEstatus }
                });
        }

        private IQueryable<Dictionary<string, object?>> ConsultarContactos(long pacienteId)
        {
            return _context.DatosContacto
                .AsNoTracking()
                .Where(x => x.PaId == pacienteId)
                .OrderBy(x => x.DaId)
                .Select(x => new Dictionary<string, object?>
                {
                    { "id", x.DaId },
                    { "tipo", x.DaTipo },
                    { "valor", x.DaValor },
                    { "principal", x.DaPrinc },
                    { "verificado", x.DaVerif },
                    { "fechaRegistro", Utc(x.DaFechaCrea) },
                    { "activo", x.DaEstatus }
                });
        }

        private IQueryable<Dictionary<string, object?>> ConsultarCitas(long pacienteId)
        {
            return _context.Citas
                .AsNoTracking()
                .Where(c => c.PaId == pacienteId)
                .Join(_context.Servicios, c => c.SeId, s => s.SeId, (c, s) => new { c, s })
                .OrderBy(x => x.c.CiFechaIni)
                .ThenBy(x => x.c.CiId)
                .Select(x => new Dictionary<string, object?>
                {
                    { "id", x.c.CiId },
                    { "servicio", x.s.SeNombre },
                    { "fechaInicio", Utc(x.c.CiFechaIni) },
                    { "fechaFin", Utc(x.c.CiFechaFin) },
                    { "estado", x.c.CiEstad },
                    { "canal", x.c.CiCanal },
                    { "observacion", x.c.CiObser },
                    { "fechaRegistro", Utc(x.c.CiFechaCrea) }
                });
        }

        private IQueryable<Dictionary<string, object?>> ConsultarConsentimientos(long pacienteId)
        {
            return _context.Consentimientos
                .AsNoTracking()
                .Where(c => c.PaId == pacienteId)
                .Join(_context.Finalidades, c => c.FiId, f => f.FiId, (c, f) => new { c, f })
                .Join(_context.Avisos, x => x.c.AvId, a => a.AvId, (x, a) => new { x.c, x.f, a })
                .OrderBy(x => x.c.CoId)
                .Select(x => new Dictionary<string, object?>
                {
                    { "id", x.c.CoId },
                    { "finalidad", x.f.FiCodigo },
                    { "finalidadNombre", x.f.FiNombre },
                    { "versionAviso", x.a.AvVersi },
                    { "estado", x.c.CoEstad },
                    { "medio", x.c.CoMedio },
                    { "fechaObtencion", Utc(x.c.CoFechaObte) },
                    { "fechaRevocatoria", Utc(x.c.CoFechaRevo) },
                    { "observacion", x.c.CoObser }
                });
        }

        private IQueryable<Dictionary<string, object?>> ConsultarEventosConsentimiento(long pacienteId)
        {
            return _context.EventosConsentimiento
                .AsNoTracking()
                .Join(_context.Consentimientos, e => e.CoId, c => c.CoId, (e, c) => new { e, c })
                .Where(x => x.c.PaId == pacienteId)
                .OrderBy(x => x.e.EvId)
                .Select(x => new Dictionary<string, object?>
                {
                    { "id", x.e.EvId },
                    { "consentimientoId", x.e.CoId },
                    { "estado", x.e.EvEstad },
                    { "fecha", Utc(x.e.EvFechaHora) },
                    { "medio", x.e.EvMedio },
                    { "observacion", x.e.EvObser }
                });
        }

        private IQueryable<Dictionary<string, object?>> ConsultarOpcionesComunicacion(long pacienteId)
        {
            return _context.OpcionesComunicacion
                .AsNoTracking()
                .Where(o => o.PaId == pacienteId)
                .Join(_context.Finalidades, o => o.FiId, f => f.FiId, (o, f) => new { o, f })
                .OrderBy(x => x.o.OpId)
                .Select(x => new Dictionary<string, object?>
                {
                    { "id", x.o.OpId },
                    { "canal", x.o.OpCanal },
                    { "finalidad", x.f.FiCodigo },
                    { "permitido", x.o.OpPermi },
                    { "fechaActualizacion", Utc(x.o.OpFechaActu) },
                    { "activo", x.o.OpEstatus }
                });
        }

        private IQueryable<Dictionary<string, object?>> ConsultarNotificaciones(long pacienteId)
        {
            return _context.Notificaciones
                .AsNoTracking()
                .Where(n => n.PaId == pacienteId)
                .Join(_context.Plantillas, n => n.PlId, p => p.PlId, (n, p) => new { n, p })
                .OrderBy(x => x.n.NoId)
                .Select(x => new Dictionary<string, object?>
                {
                    { "id", x.n.NoId },
                    { "tipo", x.n.NoTipo },
                    { "canal", x.n.NoCanal },
                    { "plantilla", x.p.PlNombre },
                    { "citaId", x.n.CiId },
                    { "fechaProgramada", Utc(x.n.NoFechaProg) },
                    { "estado", x.n.NoEstad },
                    { "fechaRegistro", Utc(x.n.NoFechaCrea) }
                });
        }

        private IQueryable<Dictionary<string, object?>> ConsultarEnvios(long pacienteId)
        {
            return _context.Envios
                .AsNoTracking()
                .Join(_context.Notificaciones, e => e.NoId, n => n.NoId, (e, n) => new { e, n })
                .Where(x => x.n.PaId == pacienteId)
                .OrderBy(x => x.e.NoId)
                .ThenBy(x => x.e.EnNumInten)
                .Select(x => new Dictionary<string, object?>
                {
                    { "id", x.e.EnId },
                    { "notificacionId", x.e.NoId },
                    { "intento", x.e.EnNumInten },
                    { "fecha", Utc(x.e.EnFechaHora) },
                    { "resultado", x.e.EnResul }
                });
        }

        private IQueryable<Dictionary<string, object?>> ConsultarSolicitudes(long pacienteId)
        {
            return _context.Solicitudes
                .AsNoTracking()
                .Where(x => x.PaId == pacienteId)
                .OrderBy(x => x.SoId)
                .Select(x => new Dictionary<string, object?>
                {
                    { "id", x.SoId },
                    { "derecho", x.SoTipoDere },
                    { "tipoSolicitante", x.SoTipoSoli },
                    { "nombreSolicitante", x.SoTipoSoli == "TITULAR" ? x.SoNombrSoli : null },
                    { "identificacionSolicitante", x.SoTipoSoli == "TITULAR" ? x.SoIdentSoli : null },
                    { "fechaRecepcion", Utc(x.SoFechaRece) },
                    { "estado", x.SoEstad },
                    { "medioRespuesta", x.SoMedioResp },
                    { "detalle", x.SoDetal },
                    { "respuesta", x.SoRespue },
                    { "fechaCierre", Utc(x.SoFechaCier) }
                });
        }

        private IQueryable<Dictionary<string, object?>> ConsultarHistoria(long pacienteId)
        {
            return _context.Historias
                .AsNoTracking()
                .Where(x => x.PaId == pacienteId)
                .Select(x => new Dictionary<string, object?>
                {
                    { "id", x.HiId },
                    { "fechaApertura", Utc(x.HiFechaAper) },
                    { "estado", x.HiEstad },
                    { "fechaBloqueo", Utc(x.HiFechaBloq) },
                    { "motivoBloqueo", x.HiMotivBloq }
                });
        }

        private IQueryable<Dictionary<string, object?>> ConsultarAntecedentes(long pacienteId)
        {
            return _context.Antecedentes
                .AsNoTracking()
                .Join(_context.Historias, a => a.HiId, h => h.HiId, (a, h) => new { a, h })
                .Where(x => x.h.PaId == pacienteId)
                .OrderBy(x => x.a.AnId)
                .Select(x => new Dictionary<string, object?>
                {
                    { "id", x.a.AnId },
                    { "tipo", x.a.AnTipo },
                    { "descripcion", x.a.AnDescri },
                    { "fechaRegistro", Utc(x.a.AnFechaReg) },
                    { "activo", x.a.AnEstatus }
                });
        }

        private IQueryable<Dictionary<string, object?>> ConsultarAtenciones(long pacienteId)
        {
            return _context.Atenciones
                .AsNoTracking()
                .Join(_context.Historias, t => t.HiId, h => h.HiId, (t, h) => new { t, h })
                .Where(x => x.h.PaId == pacienteId)
                .OrderBy(x => x.t.AtFecha)
                .ThenBy(x => x.t.AtId)
                .Select(x => new Dictionary<string, object?>
                {
                    { "id", x.t.AtId },
                    { "citaId", x.t.CiId },
                    { "fecha", Utc(x.t.AtFecha) },
                    { "motivoConsulta", x.t.AtMotivCons },
                    { "valoracionClinica", x.t.AtValorClin },
                    { "diagnostico", x.t.AtDiagn },
                    { "tratamiento", x.t.AtTrata },
                    { "observacion", x.t.AtObser },
                    { "estado", x.t.AtEstad },
                    { "fechaModificacion", Utc(x.t.AtFechaModi) }
                });
        }

        private IQueryable<Dictionary<string, object?>> ConsultarEvoluciones(long pacienteId)
        {
            return _context.Evoluciones
                .AsNoTracking()
                .Join(_context.Atenciones, e => e.AtId, t => t.AtId, (e, t) => new { e, t })
                .Join(_context.Historias, x => x.t.HiId, h => h.HiId, (x, h) => new { x.e, h })
                .Where(x => x.h.PaId == pacienteId)
                .OrderBy(x => x.e.EvFecha)
                .ThenBy(x => x.e.EvId)
                .Select(x => new Dictionary<string, object?>
                {
                    { "id", x.e.EvId },
                    { "atencionId", x.e.AtId },
                    { "fecha", Utc(x.e.EvFecha) },
                    { "descripcion", x.e.EvDescri },
                    { "activo", x.e.EvEstatus }
                });
        }

        private IQueryable<Dictionary<string, object?>> ConsultarAdjuntos(long pacienteId)
        {
            return _context.Adjuntos
                .AsNoTracking()
                .Join(_context.Atenciones, d => d.AtId, t => t.AtId, (d, t) => new { d, t })
                .Join(_context.Historias, x => x.t.HiId, h => h.HiId, (x, h) => new { x.d, h })
                .Where(x => x.h.PaId == pacienteId)
                .OrderBy(x => x.d.AdId)
                .Select(x => new Dictionary<string, object?>
                {
                    { "id", x.d.AdId },
                    { "atencionId", x.d.AtId },
                    { "nombre", x.d.AdNombre },
                    { "tipoMime", x.d.AdTipoMime },
                    { "tamanoBytes", x.d.AdTamanByte },
                    { "huella", x.d.AdHuella },
                    { "fechaCarga", Utc(x.d.AdFechaCarg) },
                    { "activo", x.d.AdEstatus }
                });
        }

        private static DateTime Utc(DateTime fecha)
        {
            return DateTime.SpecifyKind(fecha, DateTimeKind.Utc);
        }

        private static DateTime? Utc(DateTime? fecha)
        {
            return fecha.HasValue ? DateTime.SpecifyKind(fecha.Value, DateTimeKind.Utc) : null;
        }

        private static DateOnly? Dia(DateTime? fecha)
        {
            return fecha.HasValue ? DateOnly.FromDateTime(fecha.Value) : null;
        }
    }
}
