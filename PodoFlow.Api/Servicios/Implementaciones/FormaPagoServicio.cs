
using PodoFlow.Api.Datos;
using PodoFlow.Api.DTOs.FormaPago;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones;

public class FormaPagoServicio : IFormaPagoServicio
{
    private readonly IFormaPagoRepositorio _repo;
    private readonly IEventoAuditoriaServicio _auditoria;
    private readonly PodoFlowContext _context;

    public FormaPagoServicio(
        IFormaPagoRepositorio repo,
        IEventoAuditoriaServicio auditoria,
        PodoFlowContext context)
    {
        _repo = repo;
        _auditoria = auditoria;
        _context = context;
    }

    public async Task<List<FormaPagoRespuestaDto>> ObtenerTodosAsync(
        bool incluirInactivos,
        int pagina,
        int tamano)
    {
        if (pagina < 1 || tamano < 1 || tamano > 100)
        {
            throw new ArgumentException(
                "La página debe ser mayor que cero y " +
                "el tamaño debe estar entre 1 y 100.");
        }

        var datos = await _repo.ObtenerTodosAsync(
            incluirInactivos,
            pagina,
            tamano);

        return datos.Select(Mapear).ToList();
    }

    public async Task<FormaPagoRespuestaDto?> ObtenerPorIdAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID inválido.");

        var forma = await _repo.ObtenerPorIdAsync(id);

        return forma == null ? null : Mapear(forma);
    }

    public async Task<FormaPagoRespuestaDto> CrearAsync(
        FormaPagoCrearDto dto)
    {
        ValidarDatos(dto.Codigo, dto.Nombre);

        string codigo = dto.Codigo.Trim().ToUpperInvariant();
        string nombre = dto.Nombre.Trim();

        await using var tx =
            await _context.Database.BeginTransactionAsync();

        var existente = await _repo.ObtenerPorCodigoAsync(codigo);

        if (existente != null)
        {
            if (existente.FoEstatus)
            {
                throw new InvalidOperationException(
                    "Ya existe una forma de pago activa con ese código.");
            }

            existente.FoNombre = nombre;
            existente.FoEstatus = true;

            await _repo.GuardarCambiosAsync();

            await AuditarAsync(existente.FoId, "REACTIVAR");

            await tx.CommitAsync();

            return Mapear(existente);
        }

        var formaPago = new FormaPago
        {
            FoCodigo = codigo,
            FoNombre = nombre,
            FoEstatus = true
        };

        await _repo.AgregarAsync(formaPago);
        await _repo.GuardarCambiosAsync();

        await AuditarAsync(formaPago.FoId, "CREAR");

        await tx.CommitAsync();

        return Mapear(formaPago);
    }

    public async Task<bool> ActualizarAsync(
        int id,
        FormaPagoActualizarDto dto)
    {
        if (id <= 0)
            throw new ArgumentException("ID inválido.");

        ValidarDatos(dto.Codigo, dto.Nombre);

        string codigo = dto.Codigo.Trim().ToUpperInvariant();
        string nombre = dto.Nombre.Trim();

        await using var tx =
            await _context.Database.BeginTransactionAsync();

        var forma = await _repo.ObtenerPorIdAsync(id);

        if (forma == null)
            return false;

        if (!forma.FoEstatus)
        {
            throw new InvalidOperationException(
                "No se puede actualizar una forma de pago inactiva.");
        }

        var otro = await _repo.ObtenerPorCodigoAsync(codigo);

        if (otro != null && otro.FoId != id)
        {
            throw new InvalidOperationException(
                "El código ya pertenece a otra forma de pago.");
        }

        forma.FoCodigo = codigo;
        forma.FoNombre = nombre;

        await _repo.GuardarCambiosAsync();

        await AuditarAsync(forma.FoId, "ACTUALIZAR");

        await tx.CommitAsync();

        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID inválido.");

        await using var tx =
            await _context.Database.BeginTransactionAsync();

        var forma = await _repo.ObtenerPorIdAsync(id);

        if (forma == null)
            return false;

        // Baja lógica idempotente:
        // no se repite la auditoría si ya está inactiva.
        if (!forma.FoEstatus)
            return true;

        forma.FoEstatus = false;

        await _repo.GuardarCambiosAsync();

        await AuditarAsync(forma.FoId, "BAJA_LOGICA");

        await tx.CommitAsync();

        return true;
    }

    public async Task<bool> ReactivarAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID inválido.");

        await using var tx =
            await _context.Database.BeginTransactionAsync();

        var forma = await _repo.ObtenerPorIdAsync(id);

        if (forma == null)
            return false;

        if (forma.FoEstatus)
            return true;

        // La restricción UNIQUE se conserva aun
        // cuando la forma de pago está inactiva.
        forma.FoEstatus = true;

        await _repo.GuardarCambiosAsync();

        await AuditarAsync(forma.FoId, "REACTIVAR");

        await tx.CommitAsync();

        return true;
    }

    private static void ValidarDatos(
        string? codigo,
        string? nombre)
    {
        if (string.IsNullOrWhiteSpace(codigo) ||
            codigo.Trim().Length > 30)
        {
            throw new ArgumentException(
                "El código es obligatorio, máximo 30 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(nombre) ||
            nombre.Trim().Length > 100)
        {
            throw new ArgumentException(
                "El nombre es obligatorio, máximo 100 caracteres.");
        }
    }

    private async Task AuditarAsync(int id, string accion)
    {
        await _auditoria.RegistrarAsync(
            us_id: null,
            pa_id: null,
            ev_modulo: "FINANZAS",
            ev_accion: accion,
            ev_entid: "FORMA_PAGO",
            ev_entid_id: id.ToString(),
            ev_resul: "CORRECTO",
            ev_direc_ip: null,
            ev_agent_usua: null,
            ev_metad_json: null);
    }

    private static FormaPagoRespuestaDto Mapear(FormaPago forma)
    {
        return new FormaPagoRespuestaDto
        {
            Id = forma.FoId,
            Codigo = forma.FoCodigo,
            Nombre = forma.FoNombre,
            Estatus = forma.FoEstatus
        };
    }
}
