using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones
{
    public class DocumentoRepositorio : IDocumentoRepositorio
    {
        private readonly PodoFlowContext _context;

        public DocumentoRepositorio(PodoFlowContext context)
        {
            _context = context;
        }

        // Del más reciente al más antiguo.
        public async Task<List<Documento>> ObtenerTodosAsync(string? tipo, bool incluirInactivos, int pagina, int tamano)
        {
            var consulta = _context.Documentos.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(tipo))
                consulta = consulta.Where(x => x.DoTipo == tipo);

            if (!incluirInactivos)
                consulta = consulta.Where(x => x.DoEstatus);

            return await consulta
                .OrderByDescending(x => x.DoFechaCarg)
                .ThenByDescending(x => x.DoId)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync();
        }

        // Sin filtro de estatus: el detalle se muestra aunque esté dado de baja.
        public async Task<Documento?> ObtenerPorIdAsync(long id)
        {
            return await _context.Documentos
                .FirstOrDefaultAsync(x => x.DoId == id);
        }

        // La comparación la hace SQL Server, que por defecto no distingue mayúsculas.
        public async Task<Documento?> ObtenerDuplicadoAsync(string tipo, string nombre, string? version, long? excluirId)
        {
            return await _context.Documentos
                .AsNoTracking()
                .Where(x => x.DoTipo == tipo && x.DoNombre == nombre && x.DoVersi == version)
                .Where(x => excluirId == null || x.DoId != excluirId)
                .OrderByDescending(x => x.DoEstatus)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> ExisteUsuarioActivoAsync(long usuarioId)
        {
            return await _context.Usuarios
                .AnyAsync(x => x.UsId == usuarioId && x.UsEstatus);
        }

        public async Task<Documento> CrearAsync(Documento documento)
        {
            _context.Documentos.Add(documento);

            await _context.SaveChangesAsync();

            return documento;
        }

        public async Task GuardarCambiosAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
