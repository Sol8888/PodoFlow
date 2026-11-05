using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones
{
    public class TratamientoCategoriaRepositorio : ITratamientoCategoriaRepositorio
    {
        private readonly PodoFlowContext _context;

        public TratamientoCategoriaRepositorio(PodoFlowContext context)
        {
            _context = context;
        }

        public async Task<List<TratamientoCategoria>> ObtenerTodosAsync(
            int? tratamientoId, int? categoriaId, bool incluirInactivos, int pagina, int tamano)
        {
            var consulta = _context.TratamientosCategorias.AsNoTracking();

            if (tratamientoId.HasValue)
                consulta = consulta.Where(x => x.TrId == tratamientoId.Value);

            if (categoriaId.HasValue)
                consulta = consulta.Where(x => x.CaId == categoriaId.Value);

            if (!incluirInactivos)
                consulta = consulta.Where(x => x.TcEstatus);

            return await consulta
                .OrderBy(x => x.TrId)
                .ThenBy(x => x.CaId)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync();
        }

        public async Task<TratamientoCategoria?> ObtenerAsync(int tratamientoId, int categoriaId)
        {
            return await _context.TratamientosCategorias
                .FirstOrDefaultAsync(x => x.TrId == tratamientoId && x.CaId == categoriaId);
        }

        public async Task<TratamientoCategoria> CrearAsync(TratamientoCategoria asignacion)
        {
            _context.TratamientosCategorias.Add(asignacion);

            await _context.SaveChangesAsync();

            return asignacion;
        }

        public async Task GuardarCambiosAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
