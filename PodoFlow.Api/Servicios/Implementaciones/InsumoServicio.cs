
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.DTOs.Insumo;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones;

public class InsumoServicio : IInsumoServicio
{
    private readonly IInsumoRepositorio _repo;
    private readonly IEventoAuditoriaServicio _auditoria;
    private readonly PodoFlowContext _context;

    public InsumoServicio(
        IInsumoRepositorio repo,
        IEventoAuditoriaServicio auditoria,
        PodoFlowContext context)
    {
        _repo = repo;
        _auditoria = auditoria;
        _context = context;
    }

    public async Task<List<InsumoRespuestaDto>> ObtenerTodosAsync(
        bool incluirInactivos,
        int pagina,
        int tamano)
    {
        if (pagina < 1 || tamano < 1 || tamano > 100)
            throw new ArgumentException(
                "La página debe ser mayor que cero y " +
                "el tamaño debe estar entre 1 y 100.");

        var insumos = await _repo.ObtenerTodosAsync(
            incluirInactivos, pagina, tamano);

        return insumos.Select(Mapear).ToList();
    }

    public async Task<InsumoRespuestaDto?> ObtenerPorIdAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID inválido.");

        var insumo = await _repo.ObtenerPorIdAsync(id);

        return insumo == null ? null : Mapear(insumo);
    }

    public async Task<InsumoRespuestaDto> CrearAsync(
        InsumoCrearDto dto)
    {
        ValidarDatos(
            dto.Nombre,
            dto.Categoria,
            dto.Unidad,
            dto.StockMinimo);

        string nombre = dto.Nombre.Trim();
        string categoria = dto.Categoria.Trim();
        string unidad = dto.Unidad.Trim();

        await using var tx =
            await _context.Database.BeginTransactionAsync();

        var existentes = await _repo.BuscarIdenticosAsync(
            nombre, categoria, unidad);

        if (existentes.Any(x => x.InEstatus))
        {
            throw new InvalidOperationException(
                "Ya existe un insumo activo con esos datos.");
        }

        var inactivo = existentes.FirstOrDefault();

        if (inactivo != null)
        {
            // Se conserva el stock histórico del registro.
            // Crear no genera entradas de inventario.
            inactivo.InEstatus = true;
            inactivo.InStockMini = dto.StockMinimo;
            inactivo.InFechaModi = DateTime.UtcNow;

            await _repo.GuardarCambiosAsync();

            await AuditarAsync(
                inactivo.InId, "REACTIVAR");

            await tx.CommitAsync();

            return Mapear(inactivo);
        }

        var insumo = new Insumo
        {
            InNombre = nombre,
            InCateg = categoria,
            InUnidad = unidad,
            InStockActu = 0m,
            InStockMini = dto.StockMinimo,
            InFechaCrea = DateTime.UtcNow,
            InEstatus = true
        };

        await _repo.AgregarAsync(insumo);
        await _repo.GuardarCambiosAsync();

        await AuditarAsync(insumo.InId, "CREAR");

        await tx.CommitAsync();

        return Mapear(insumo);
    }

    public async Task<bool> ActualizarAsync(
        int id,
        InsumoActualizarDto dto)
    {
        ValidarDatos(
            dto.Nombre,
            dto.Categoria,
            dto.Unidad,
            dto.StockMinimo);

        byte[] version = LeerVersion(dto.Version);

        await using var tx =
            await _context.Database.BeginTransactionAsync();

        var insumo = await _repo.ObtenerPorIdAsync(id);

        if (insumo == null)
            return false;

        if (!insumo.InEstatus)
        {
            throw new InvalidOperationException(
                "No se puede actualizar un insumo inactivo.");
        }

        string nombre = dto.Nombre.Trim();
        string categoria = dto.Categoria.Trim();
        string unidad = dto.Unidad.Trim();

        var coincidentes = await _repo.BuscarIdenticosAsync(
            nombre, categoria, unidad);

        if (coincidentes.Any(x => x.InId != id && x.InEstatus))
        {
            throw new InvalidOperationException(
                "Ya existe otro insumo activo con esos datos.");
        }

        _repo.EstablecerVersionOriginal(insumo, version);

        insumo.InNombre = nombre;
        insumo.InCateg = categoria;
        insumo.InUnidad = unidad;
        insumo.InStockMini = dto.StockMinimo;
        insumo.InFechaModi = DateTime.UtcNow;

        // Garantiza que EF emita un UPDATE incluso si
        // los demás datos no han cambiado.
        _context.Entry(insumo)
            .Property(x => x.InFechaModi)
            .IsModified = true;

        await _repo.GuardarCambiosAsync();

        await AuditarAsync(insumo.InId, "ACTUALIZAR");

        await tx.CommitAsync();

        return true;
    }

    public async Task<bool> EliminarAsync(
        int id,
        string versionTexto)
    {
        byte[] version = LeerVersion(versionTexto);

        await using var tx =
            await _context.Database.BeginTransactionAsync();

        var insumo = await _repo.ObtenerPorIdAsync(id);

        if (insumo == null)
            return false;

        if (!insumo.InEstatus)
            return true;

        _repo.EstablecerVersionOriginal(insumo, version);

        insumo.InEstatus = false;
        insumo.InFechaModi = DateTime.UtcNow;

        await _repo.GuardarCambiosAsync();

        await AuditarAsync(insumo.InId, "BAJA_LOGICA");

        await tx.CommitAsync();

        return true;
    }

    public async Task<bool> ReactivarAsync(
        int id,
        string versionTexto)
    {
        byte[] version = LeerVersion(versionTexto);

        await using var tx =
            await _context.Database.BeginTransactionAsync();

        var insumo = await _repo.ObtenerPorIdAsync(id);

        if (insumo == null)
            return false;

        if (insumo.InEstatus)
            return true;

        var coincidentes = await _repo.BuscarIdenticosAsync(
            insumo.InNombre,
            insumo.InCateg,
            insumo.InUnidad);

        if (coincidentes.Any(x =>
            x.InId != insumo.InId && x.InEstatus))
        {
            throw new InvalidOperationException(
                "Ya existe otro insumo activo idéntico.");
        }

        _repo.EstablecerVersionOriginal(insumo, version);

        insumo.InEstatus = true;
        insumo.InFechaModi = DateTime.UtcNow;

        await _repo.GuardarCambiosAsync();

        await AuditarAsync(insumo.InId, "REACTIVAR");

        await tx.CommitAsync();

        return true;
    }

    private static void ValidarDatos(
        string? nombre,
        string? categoria,
        string? unidad,
        decimal stockMinimo)
    {
        if (string.IsNullOrWhiteSpace(nombre) ||
            nombre.Trim().Length > 150)
            throw new ArgumentException(
                "Nombre obligatorio, máximo 150 caracteres.");

        if (string.IsNullOrWhiteSpace(categoria) ||
            categoria.Trim().Length > 80)
            throw new ArgumentException(
                "Categoría obligatoria, máximo 80 caracteres.");

        if (string.IsNullOrWhiteSpace(unidad) ||
            unidad.Trim().Length > 30)
            throw new ArgumentException(
                "Unidad obligatoria, máximo 30 caracteres.");

        if (stockMinimo < 0 ||
            stockMinimo > 9999999999.99m ||
            decimal.Round(stockMinimo, 2) != stockMinimo)
            throw new ArgumentException(
                "Stock mínimo inválido. Usa un valor " +
                "no negativo con hasta dos decimales.");
    }

    private static byte[] LeerVersion(string? valor)
    {
        try
        {
            byte[] version =
                Convert.FromBase64String(valor ?? "");

            if (version.Length != 8)
                throw new FormatException();

            return version;
        }
        catch (FormatException)
        {
            throw new ArgumentException(
                "Versión inválida. Consulta primero el GET.");
        }
    }

    private async Task AuditarAsync(
        int insumoId,
        string accion)
    {
        await _auditoria.RegistrarAsync(
            us_id: null,
            pa_id: null,
            ev_modulo: "INVENTARIO",
            ev_accion: accion,
            ev_entid: "INSUMO",
            ev_entid_id: insumoId.ToString(),
            ev_resul: "CORRECTO",
            ev_direc_ip: null,
            ev_agent_usua: null,
            ev_metad_json: null);
    }

    private static InsumoRespuestaDto Mapear(Insumo i)
    {
        return new InsumoRespuestaDto
        {
            Id = i.InId,
            Nombre = i.InNombre,
            Categoria = i.InCateg,
            Unidad = i.InUnidad,
            StockActual = i.InStockActu,
            StockMinimo = i.InStockMini,
            BajoMinimo = i.InStockActu < i.InStockMini,
            FechaCreacion = i.InFechaCrea,
            FechaModificacion = i.InFechaModi,
            Estatus = i.InEstatus,
            Version = Convert.ToBase64String(i.InVersion)
        };
    }
}
