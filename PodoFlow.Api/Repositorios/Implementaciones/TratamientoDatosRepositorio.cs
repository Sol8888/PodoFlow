using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones
{
    public class TratamientoDatosRepositorio : ITratamientoDatosRepositorio
    {
        private readonly PodoFlowContext _context;

        public TratamientoDatosRepositorio(PodoFlowContext context)
        {
            _context = context;
        }

        public async Task<List<TratamientoDatos>> ObtenerTodosAsync(bool incluirInactivos, int pagina, int tamano)
        {
            var consulta = _context.TratamientosDatos.AsNoTracking();

            if (!incluirInactivos)
                consulta = consulta.Where(x => x.TrEstatus);

            return await consulta
                .OrderBy(x => x.TrCodigo)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync();
        }

        public async Task<TratamientoDatos?> ObtenerPorIdAsync(int id)
        {
            return await _context.TratamientosDatos
                .FirstOrDefaultAsync(x => x.TrId == id);
        }

        public async Task<TratamientoDatos?> ObtenerPorCodigoAsync(string codigo)
        {
            return await _context.TratamientosDatos
                .FirstOrDefaultAsync(x => x.TrCodigo == codigo);
        }

        public async Task<bool> ExisteUsuarioActivoAsync(long usuarioId)
        {
            return await _context.Database
                .SqlQuery<long>($"SELECT us_id AS Value FROM seg.USUARIO WHERE us_id = {usuarioId} AND us_estatus = 1")
                .AnyAsync();
        }

        public async Task<TratamientoDatos> CrearAsync(TratamientoDatos tratamiento)
        {
            _context.TratamientosDatos.Add(tratamiento);

            await _context.SaveChangesAsync();

            return tratamiento;
        }

        public async Task GuardarCambiosAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
