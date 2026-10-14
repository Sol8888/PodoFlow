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

        public async Task<List<Finalidad>> ObtenerTodosAsync(bool incluirInactivos, int pagina, int tamano)
        {
            var consulta = _context.Finalidades.AsNoTracking();

            if (!incluirInactivos)
                consulta = consulta.Where(x => x.FiEstatus);

            return await consulta
                .OrderBy(x => x.FiNombre)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
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

        public async Task GuardarCambiosAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}