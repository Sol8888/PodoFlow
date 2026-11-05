using Microsoft.EntityFrameworkCore; 
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones
{
    public class EncargadoRepositorio : IEncargadoRepositorio
    {
        private readonly PodoFlowContext _context;

        public EncargadoRepositorio(PodoFlowContext context)
        {
            _context = context;
        }

        public async Task<List<Encargado>> ObtenerTodosAsync(bool incluirInactivos, int pagina, int tamano)
        {
            var consulta = _context.Encargados.AsNoTracking();

            if (!incluirInactivos)
                consulta = consulta.Where(x => x.EnEstatus);

            return await consulta
                .OrderBy(x => x.EnNombre)
                .ThenBy(x => x.EnServi)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync();
        }

        public async Task<Encargado?> ObtenerPorIdAsync(int id)
        {
            return await _context.Encargados
                .FirstOrDefaultAsync(x => x.EnId == id);
        }

        public async Task<Encargado?> ObtenerDuplicadoAsync(string nombre, string servicio, int? excluirId)
        {
            return await _context.Encargados
                .AsNoTracking()
                .Where(x => x.EnNombre == nombre && x.EnServi == servicio)
                .Where(x => excluirId == null || x.EnId != excluirId)
                .OrderByDescending(x => x.EnEstatus)
                .FirstOrDefaultAsync();
        }

        public async Task<Encargado> CrearAsync(Encargado encargado)
        {
            _context.Encargados.Add(encargado);

            await _context.SaveChangesAsync();

            return encargado;
        }

        public async Task GuardarCambiosAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}