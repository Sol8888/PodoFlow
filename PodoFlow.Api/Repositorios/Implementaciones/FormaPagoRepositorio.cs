
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones;

public class FormaPagoRepositorio : IFormaPagoRepositorio
{
    private readonly PodoFlowContext _context;

    public FormaPagoRepositorio(PodoFlowContext context)
    {
        _context = context;
    }

    public Task<List<FormaPago>> ObtenerTodosAsync(
        bool incluirInactivos,
        int pagina,
        int tamano)
    {
        IQueryable<FormaPago> consulta =
            _context.FormasPago.AsNoTracking();

        if (!incluirInactivos)
        {
            consulta = consulta.Where(x => x.FoEstatus);
        }

        return consulta
            .OrderBy(x => x.FoNombre)
            .ThenBy(x => x.FoId)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();
    }

    public Task<FormaPago?> ObtenerPorIdAsync(int id)
    {
        return _context.FormasPago
            .FirstOrDefaultAsync(x => x.FoId == id);
    }

    public Task<FormaPago?> ObtenerPorCodigoAsync(string codigo)
    {
        return _context.FormasPago
            .FirstOrDefaultAsync(x => x.FoCodigo == codigo);
    }

    public async Task AgregarAsync(FormaPago formaPago)
    {
        await _context.FormasPago.AddAsync(formaPago);
    }

    public async Task GuardarCambiosAsync()
    {
        await _context.SaveChangesAsync();
    }
}
