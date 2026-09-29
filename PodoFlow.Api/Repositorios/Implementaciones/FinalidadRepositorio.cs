using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
namespace PodoFlow.Api.Repositorios.Implementaciones
{
    public class FinalidadRepositorio : IFinalidadRepositorio
    {
        private readonly PodoFlowContext _context;

        public FinalidadRepositorio(PodoFlowContext context)
        {
            _context = context;
        }

        public async Task<List<Finalidad>> ObtenerTodosAsync()
        {
            return await _context.Finalidades
                .AsNoTracking()
                .OrderBy(x => x.FiNombre)
                .ToListAsync();
        }

        public async Task<Finalidad?> ObtenerPorIdAsync(int id)
        {
            return await _context.Finalidades
                .FirstOrDefaultAsync(x => x.FiId == id);
        }

        public async Task<Finalidad?> ObtenerPorCodigoAsync(string codigo)
        {
            return await _context.Finalidades
                .FirstOrDefaultAsync(x => x.FiCodigo == codigo);
        }

        public async Task<Finalidad> CrearAsync(Finalidad finalidad)
        {
            _context.Finalidades.Add(finalidad);

            await _context.SaveChangesAsync();

            return finalidad;
        }

        public async Task ActualizarAsync(Finalidad finalidad)
        {
            _context.Finalidades.Update(finalidad);

            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(Finalidad finalidad)
        {
            finalidad.FiEstatus = false;

            await _context.SaveChangesAsync();
        }
    }
}