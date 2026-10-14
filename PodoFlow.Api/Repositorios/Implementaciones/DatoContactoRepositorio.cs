
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;

namespace PodoFlow.Api.Repositorios.Implementaciones;

public class DatoContactoRepositorio : IDatoContactoRepositorio
{
    private readonly PodoFlowContext _context;

    public DatoContactoRepositorio(PodoFlowContext context)
    {
        _context = context;
    }

    public async Task<List<DatoContacto>> ListarAsync(
        long? pacienteId, bool incluirInactivos,
        int pagina, int tamano)
    {
        var consulta = _context.DatosContacto.AsNoTracking();

        if (pacienteId.HasValue)
            consulta = consulta.Where(x =>
                x.PaId == pacienteId.Value);

        if (!incluirInactivos)
            consulta = consulta.Where(x => x.DaEstatus);

        return await consulta
            .OrderBy(x => x.PaId)
            .ThenByDescending(x => x.DaPrinc)
            .ThenBy(x => x.DaId)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();
    }

    public Task<DatoContacto?> ObtenerAsync(long id) =>
        _context.DatosContacto.FirstOrDefaultAsync(x =>
            x.DaId == id);

    public Task<DatoContacto?> BuscarAsync(
        long pacienteId, string tipo, string valor) =>
        _context.DatosContacto.FirstOrDefaultAsync(x =>
            x.PaId == pacienteId &&
            x.DaTipo == tipo &&
            x.DaValor == valor);

    public Task<bool> PacienteActivoAsync(long id) =>
        _context.Pacientes.AnyAsync(x =>
            x.PaId == id && x.PaEstatus);

    public async Task<DatoContacto> CrearAsync(DatoContacto contacto)
    {
        _context.DatosContacto.Add(contacto);
        await _context.SaveChangesAsync();
        return contacto;
    }

    public Task GuardarAsync() => _context.SaveChangesAsync();
}
