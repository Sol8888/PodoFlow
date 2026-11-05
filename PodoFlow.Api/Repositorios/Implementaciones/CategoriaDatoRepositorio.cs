using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones
{
    public class CategoriaDatoRepositorio : ICategoriaDatoRepositorio
    {
        private readonly PodoFlowContext _context;

        public CategoriaDatoRepositorio(PodoFlowContext context)
        {
            _context = context;
        }

        public async Task<List<CategoriaDato>> ObtenerTodosAsync(bool incluirInactivos, int pagina, int tamano)
        {
            var consulta = _context.CategoriasDato.AsNoTracking();

            if (!incluirInactivos)
                consulta = consulta.Where(x => x.CaEstatus);

            return await consulta
                .OrderBy(x => x.CaNombre)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync();
        }

        public async Task<CategoriaDato?> ObtenerPorIdAsync(int id)
        {
            return await _context.CategoriasDato
                .FirstOrDefaultAsync(x => x.CaId == id);
        }

        public async Task<CategoriaDato?> ObtenerPorNombreAsync(string nombre)
        {
            return await _context.CategoriasDato
                .FirstOrDefaultAsync(x => x.CaNombre == nombre);
        }

        public async Task<CategoriaDato> CrearAsync(CategoriaDato categoria)
        {
            _context.CategoriasDato.Add(categoria);

            await _context.SaveChangesAsync();

            return categoria;
        }

        public async Task GuardarCambiosAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
