using Microsoft.EntityFrameworkCore; 
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces; 

namespace PodoFlow.Api.Repositorios.Implementaciones
{
    public class AvisoRepositorio : IAvisoRepositorio 
    {
        private readonly PodoFlowContext _context;

        public AvisoRepositorio(PodoFlowContext context)
        {
            _context = context;
        }

        public async Task<List<Aviso>> ObtenerTodosAsync(bool incluirInactivos, int pagina, int tamano)
        {
            var consulta = _context.Avisos.AsNoTracking();

            if (!incluirInactivos)
                consulta = consulta.Where(x => x.AvEstatus);

            return await consulta
                .OrderByDescending(x => x.AvFechaDesd)
                .ThenByDescending(x => x.AvId)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync();
        }

        public async Task<Aviso?> ObtenerPorIdAsync(int id)
        {
            return await _context.Avisos
                .FirstOrDefaultAsync(x => x.AvId == id);
        }

        public async Task<Aviso?> ObtenerPorVersionAsync(string version)
        {
            return await _context.Avisos
                .FirstOrDefaultAsync(x => x.AvVersi == version);
        }

        public async Task<Aviso> CrearAsync(Aviso aviso)
        {
            _context.Avisos.Add(aviso);

            await _context.SaveChangesAsync();

            return aviso;
        }

        public async Task GuardarCambiosAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
