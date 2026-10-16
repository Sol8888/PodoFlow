using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones
{
    public class OpcionComunicacionRepositorio : IOpcionComunicacionRepositorio
    {
        private readonly PodoFlowContext _context;

        public OpcionComunicacionRepositorio(PodoFlowContext context)
        {
            _context = context;
        }

        public async Task<List<OpcionComunicacion>> ObtenerTodosAsync(
            long? pacienteId, int? finalidadId, string? canal, bool incluirInactivos, int pagina, int tamano)
        {
            var consulta = _context.OpcionesComunicacion.AsNoTracking();

            if (pacienteId.HasValue)
                consulta = consulta.Where(x => x.PaId == pacienteId.Value);

            if (finalidadId.HasValue)
                consulta = consulta.Where(x => x.FiId == finalidadId.Value);

            if (!string.IsNullOrWhiteSpace(canal))
                consulta = consulta.Where(x => x.OpCanal == canal);

            if (!incluirInactivos)
                consulta = consulta.Where(x => x.OpEstatus);

            return await consulta
                .OrderBy(x => x.PaId)
                .ThenBy(x => x.FiId)
                .ThenBy(x => x.OpCanal)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync();
        }

        // Sin filtro de estatus: el detalle se muestra aunque esté dada de baja.
        public async Task<OpcionComunicacion?> ObtenerPorIdAsync(long id)
        {
            return await _context.OpcionesComunicacion
                .FirstOrDefaultAsync(x => x.OpId == id);
        }

        // Con tracking: si está dada de baja, el servicio la reactiva sobre esta misma entidad.
        public async Task<OpcionComunicacion?> ObtenerPorClaveAsync(long pacienteId, string canal, int finalidadId)
        {
            return await _context.OpcionesComunicacion
                .FirstOrDefaultAsync(x => x.PaId == pacienteId && x.OpCanal == canal && x.FiId == finalidadId);
        }

        public async Task<Consentimiento?> ObtenerConsentimientoAsync(long pacienteId, int finalidadId)
        {
            return await _context.Consentimientos
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.PaId == pacienteId && x.FiId == finalidadId);
        }

        // pac.PACIENTE es del bloque de la Persona 1: solo se lee.
        public async Task<bool> ExistePacienteActivoAsync(long pacienteId)
        {
            return await _context.Database
                .SqlQuery<long>($"SELECT pa_id AS Value FROM pac.PACIENTE WHERE pa_id = {pacienteId} AND pa_estatus = 1")
                .AnyAsync();
        }

        public async Task<OpcionComunicacion> CrearAsync(OpcionComunicacion opcion)
        {
            _context.OpcionesComunicacion.Add(opcion);

            await _context.SaveChangesAsync();

            return opcion;
        }

        public async Task GuardarCambiosAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
