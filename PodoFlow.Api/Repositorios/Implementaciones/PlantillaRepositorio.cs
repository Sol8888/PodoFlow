using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones
{
    public class PlantillaRepositorio : IPlantillaRepositorio
    {
        private readonly PodoFlowContext _context;

        public PlantillaRepositorio(PodoFlowContext context)
        {
            _context = context;
        }

        // Por nombre; dentro del mismo nombre, la versión más reciente primero.
        public async Task<List<Plantilla>> ObtenerTodosAsync(
            string? canal, string? tipo, bool incluirInactivos, int pagina, int tamano)
        {
            var consulta = _context.Plantillas.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(canal))
                consulta = consulta.Where(x => x.PlCanal == canal);

            if (!string.IsNullOrWhiteSpace(tipo))
                consulta = consulta.Where(x => x.PlTipo == tipo);

            if (!incluirInactivos)
                consulta = consulta.Where(x => x.PlEstatus);

            return await consulta
                .OrderBy(x => x.PlNombre)
                .ThenBy(x => x.PlCanal)
                .ThenByDescending(x => x.PlFechaCrea)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync();
        }

        // Sin filtro de estatus: el detalle se muestra aunque esté dada de baja.
        public async Task<Plantilla?> ObtenerPorIdAsync(int id)
        {
            return await _context.Plantillas
                .FirstOrDefaultAsync(x => x.PlId == id);
        }

        // La comparación la hace SQL Server, que por defecto no distingue mayúsculas.
        public async Task<Plantilla?> ObtenerDuplicadoAsync(string nombre, string canal, string version, int? excluirId)
        {
            return await _context.Plantillas
                .AsNoTracking()
                .Where(x => x.PlNombre == nombre && x.PlCanal == canal && x.PlVersi == version)
                .Where(x => excluirId == null || x.PlId != excluirId)
                .OrderByDescending(x => x.PlEstatus)
                .FirstOrDefaultAsync();
        }

        // com.NOTIFICACION es de mi bloque, pero todavía no tiene modelo: se lee con SQL.
        // Cuando exista el DbSet de Notificacion se puede cambiar por LINQ.
        public async Task<int> ContarNotificacionesAsync(int plantillaId, bool soloPendientes)
        {
            var consulta = _context.Notificaciones.Where(x => x.PlId == plantillaId);

            if (soloPendientes)
                consulta = consulta.Where(x => x.NoEstad == "PENDIENTE" || x.NoEstad == "PROCESANDO");

            return await consulta.CountAsync();
        }

        public async Task<Plantilla> CrearAsync(Plantilla plantilla)
        {
            _context.Plantillas.Add(plantilla);

            await _context.SaveChangesAsync();

            return plantilla;
        }

        public async Task GuardarCambiosAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}