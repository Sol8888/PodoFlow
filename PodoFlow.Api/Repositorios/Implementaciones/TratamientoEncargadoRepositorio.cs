using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones
{
    public class TratamientoEncargadoRepositorio : ITratamientoEncargadoRepositorio
    {
        private readonly PodoFlowContext _context;

        public TratamientoEncargadoRepositorio(PodoFlowContext context)
        {
            _context = context;
        }

        // Filtros opcionales: los encargados de un tratamiento, o los tratamientos en los que participa un encargado.
        public async Task<List<TratamientoEncargado>> ObtenerTodosAsync(
            int? tratamientoId, int? encargadoId, bool incluirInactivos, int pagina, int tamano)
        {
            var consulta = _context.TratamientosEncargados.AsNoTracking();

            if (tratamientoId.HasValue)
                consulta = consulta.Where(x => x.TrId == tratamientoId.Value);

            if (encargadoId.HasValue)
                consulta = consulta.Where(x => x.EnId == encargadoId.Value);

            if (!incluirInactivos)
                consulta = consulta.Where(x => x.TeEstatus);

            return await consulta
                .OrderBy(x => x.TrId)
                .ThenBy(x => x.EnId)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync();
        }

        // Por la clave compuesta. Sin filtro de estatus (para poder reactivar).
        public async Task<TratamientoEncargado?> ObtenerAsync(int tratamientoId, int encargadoId)
        {
            return await _context.TratamientosEncargados
                .FirstOrDefaultAsync(x => x.TrId == tratamientoId && x.EnId == encargadoId);
        }

        public async Task<TratamientoEncargado> CrearAsync(TratamientoEncargado asignacion)
        {
            _context.TratamientosEncargados.Add(asignacion);

            await _context.SaveChangesAsync();

            return asignacion;
        }

        public async Task GuardarCambiosAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
