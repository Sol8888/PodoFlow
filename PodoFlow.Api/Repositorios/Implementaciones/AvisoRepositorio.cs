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

        public async Task<List<Aviso>> ObtenerTodosAsync()
        {
            return await _context.Avisos
                .AsNoTracking()
                .OrderByDescending(x => x.AvFechaDesd)
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

        public async Task ActualizarAsync(Aviso aviso)
        {
            _context.Avisos.Update(aviso);

            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(Aviso aviso)
        {
            aviso.AvEstatus = false;

            await _context.SaveChangesAsync();
        }
    }
}
