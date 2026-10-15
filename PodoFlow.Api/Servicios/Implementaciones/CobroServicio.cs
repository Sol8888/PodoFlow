
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.DTOs.Cobro;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones;

public class CobroServicio : ICobroServicio
{
    private const decimal Maximo = 9999999999.99m;

    private readonly ICobroRepositorio _repo;
    private readonly IEventoAuditoriaServicio _auditoria;
    private readonly PodoFlowContext _context;

    public CobroServicio(
        ICobroRepositorio repo,
        IEventoAuditoriaServicio auditoria,
        PodoFlowContext context)
    {
        _repo = repo;
        _auditoria = auditoria;
        _context = context;
    }

    public async Task<List<CobroRespuestaDto>> ObtenerTodosAsync(
        long? prestacionId,
        string? estado,
        int pagina,
        int tamano)
    {
        if (pagina < 1 || tamano < 1 || tamano > 100)
            throw new ArgumentException("Paginación inválida.");

        if (prestacionId.HasValue && prestacionId.Value <= 0)
            throw new ArgumentException("Prestación inválida.");

        string? filtro = null;

        if (!string.IsNullOrWhiteSpace(estado))
        {
            filtro = estado.Trim().ToUpperInvariant();

            if (filtro is not ("REGISTRADO" or "ANULADO"))
                throw new ArgumentException("Estado inválido.");
        }

        var datos = await _repo.ObtenerTodosAsync(
            prestacionId, filtro, pagina, tamano);

        var resultado = new List<CobroRespuestaDto>();

        foreach (var cobro in datos)
        {
            resultado.Add(await MapearAsync(cobro));
        }

        return resultado;
    }

    public async Task<CobroRespuestaDto?> ObtenerPorIdAsync(long id)
    {
        if (id <= 0)
            throw new ArgumentException("ID inválido.");

        var cobro = await _repo.ObtenerPorIdSinSeguimientoAsync(id);

        return cobro == null
            ? null
            : await MapearAsync(cobro);
    }

    public async Task<CobroRespuestaDto> CrearAsync(
        CobroCrearDto dto)
    {
        ValidarValor(dto.Valor);
        ValidarTextos(dto.Referencia, dto.Observaciones);

        if (dto.PrestacionId <= 0 ||
            dto.FormaPagoId <= 0 ||
            dto.UsuarioId <= 0)
            throw new ArgumentException(
                "Prestación, forma de pago y usuario son obligatorios.");

        await using var tx =
            await _context.Database.BeginTransactionAsync();

        // Bloquear antes de calcular el saldo.
        var prestacion = await _repo.BloquearPrestacionAsync(
            dto.PrestacionId);

        if (prestacion == null)
            throw new ArgumentException("La prestación no existe.");

        ValidarPrestacion(prestacion);

        await ValidarFormaPagoAsync(dto.FormaPagoId);

        if (!await _repo.UsuarioActivoAsync(dto.UsuarioId))
            throw new ArgumentException(
                "El usuario no existe o está inactivo.");

        var totalAnterior = await _repo.TotalCobradoAsync(
            dto.PrestacionId);

        var totalNuevo = totalAnterior + dto.Valor;

        ValidarTotal(totalNuevo, prestacion.PrTotal);

        var cobro = new Cobro
        {
            PrId = dto.PrestacionId,
            FoId = dto.FormaPagoId,
            UsId = dto.UsuarioId,
            CoFechaPago = DateTime.UtcNow,
            CoValor = dto.Valor,
            CoRefe = Limpiar(dto.Referencia),
            CoObser = Limpiar(dto.Observaciones),
            CoEstad = "REGISTRADO"
        };

        await _repo.AgregarAsync(cobro);
        await _repo.GuardarCambiosAsync();

        RecalcularEstado(prestacion, totalNuevo);
        await _repo.GuardarCambiosAsync();

        await AuditarAsync(cobro, prestacion.PaId, "CREAR");

        await tx.CommitAsync();

        return await MapearAsync(cobro);
    }

    public async Task<bool> ActualizarAsync(
        long id,
        CobroActualizarDto dto)
    {
        ValidarValor(dto.Valor);
        ValidarTextos(dto.Referencia, dto.Observaciones);

        if (dto.FormaPagoId <= 0)
            throw new ArgumentException("Forma de pago inválida.");

        await using var tx =
            await _context.Database.BeginTransactionAsync();

        // Primero buscamos solo la relación con PRESTACION.
        var referencia = await _repo.ObtenerPorIdSinSeguimientoAsync(id);

        if (referencia == null)
            return false;

        var prestacion = await _repo.BloquearPrestacionAsync(
            referencia.PrId);

        if (prestacion == null)
            throw new ArgumentException("La prestación no existe.");

        var cobro = await _repo.ObtenerPorIdAsync(id);

        if (cobro == null)
            return false;

        if (cobro.CoEstad != "REGISTRADO")
            throw new InvalidOperationException(
                "No se puede modificar un cobro anulado.");

        ValidarPrestacion(prestacion);
        await ValidarFormaPagoAsync(dto.FormaPagoId);

        var totalAnterior = await _repo.TotalCobradoAsync(
            cobro.PrId);

        var totalNuevo =
            totalAnterior - cobro.CoValor + dto.Valor;

        ValidarTotal(totalNuevo, prestacion.PrTotal);

        cobro.FoId = dto.FormaPagoId;
        cobro.CoValor = dto.Valor;
        cobro.CoRefe = Limpiar(dto.Referencia);
        cobro.CoObser = Limpiar(dto.Observaciones);

        await _repo.GuardarCambiosAsync();

        RecalcularEstado(prestacion, totalNuevo);
        await _repo.GuardarCambiosAsync();

        await AuditarAsync(
            cobro, prestacion.PaId, "ACTUALIZAR");

        await tx.CommitAsync();

        return true;
    }

    public async Task<bool> CambiarEstadoAsync(
        long id,
        CobroCambiarEstadoDto dto)
    {
        var nuevo = dto.NuevoEstado?.Trim().ToUpperInvariant();

        if (nuevo != "ANULADO")
            throw new ArgumentException(
                "Solo se permite cambiar a ANULADO.");

        if (string.IsNullOrWhiteSpace(dto.Motivo) ||
            dto.Motivo.Trim().Length > 500)
            throw new ArgumentException(
                "El motivo es obligatorio (máximo 500 caracteres).");

        await using var tx =
            await _context.Database.BeginTransactionAsync();

        var referencia = await _repo.ObtenerPorIdSinSeguimientoAsync(id);

        if (referencia == null)
            return false;

        var prestacion = await _repo.BloquearPrestacionAsync(
            referencia.PrId);

        if (prestacion == null)
            throw new ArgumentException("La prestación no existe.");

        var cobro = await _repo.ObtenerPorIdAsync(id);

        if (cobro == null)
            return false;

        if (cobro.CoEstad == "ANULADO")
            return true;

        if (cobro.CoEstad != "REGISTRADO")
            throw new InvalidOperationException(
                "Estado actual del cobro inválido.");

        if (prestacion.PrEstad == "ANULADA")
            throw new InvalidOperationException(
                "La prestación está anulada.");

        var totalAnterior = await _repo.TotalCobradoAsync(
            cobro.PrId);

        var totalNuevo = totalAnterior - cobro.CoValor;

        if (totalNuevo < 0)
            throw new InvalidOperationException(
                "Los importes registrados son inconsistentes.");

        var nota = "[ANULACION] " + dto.Motivo.Trim();

        var observacion = string.IsNullOrWhiteSpace(cobro.CoObser)
            ? nota
            : cobro.CoObser + " | " + nota;

        if (observacion.Length > 1000)
            throw new InvalidOperationException(
                "No hay espacio para guardar el motivo de anulación.");

        cobro.CoObser = observacion;
        cobro.CoEstad = "ANULADO";

        await _repo.GuardarCambiosAsync();

        RecalcularEstado(prestacion, totalNuevo);
        await _repo.GuardarCambiosAsync();

        await AuditarAsync(
            cobro, prestacion.PaId, "ANULAR");

        await tx.CommitAsync();

        return true;
    }

    private async Task ValidarFormaPagoAsync(int id)
    {
        var forma = await _repo.ObtenerFormaPagoAsync(id);

        if (forma == null || !forma.FoEstatus)
            throw new ArgumentException(
                "La forma de pago no existe o está inactiva.");
    }

    private static void ValidarPrestacion(Prestacion p)
    {
        if (p.PrEstad == "ANULADA")
            throw new InvalidOperationException(
                "No se pueden registrar cobros de una prestación anulada.");

        if (p.PrEstad is not ("PENDIENTE" or "PAGADA"))
            throw new InvalidOperationException(
                "Estado de prestación inválido.");
    }

    private static void RecalcularEstado(
        Prestacion prestacion,
        decimal totalCobrado)
    {
        var nuevo = totalCobrado == prestacion.PrTotal
            ? "PAGADA"
            : "PENDIENTE";

        prestacion.PrEstad = nuevo;
    }

    private static void ValidarTotal(
        decimal totalCobrado,
        decimal totalPrestacion)
    {
        if (totalCobrado < 0 ||
            totalCobrado > Maximo ||
            totalCobrado > totalPrestacion)
        {
            throw new InvalidOperationException(
                "El valor excede el saldo pendiente " +
                "o produce un total de cobros inválido.");
        }
    }

    private static void ValidarValor(decimal valor)
    {
        if (valor <= 0 ||
            valor > Maximo ||
            decimal.Round(valor, 2) != valor)
        {
            throw new ArgumentException(
                "El valor debe ser positivo, con máximo " +
                "dos decimales y dentro de DECIMAL(12,2).");
        }
    }

    private static void ValidarTextos(
        string? referencia, string? observaciones)
    {
        if (referencia?.Trim().Length > 150)
            throw new ArgumentException(
                "La referencia admite máximo 150 caracteres.");

        if (observaciones?.Trim().Length > 1000)
            throw new ArgumentException(
                "Las observaciones admiten máximo 1000 caracteres.");
    }

    private static string? Limpiar(string? valor) =>
        string.IsNullOrWhiteSpace(valor)
            ? null : valor.Trim();

    private async Task AuditarAsync(
        Cobro cobro, long pacienteId, string accion)
    {
        await _auditoria.RegistrarAsync(
            us_id: null,
            pa_id: pacienteId,
            ev_modulo: "FINANZAS",
            ev_accion: accion,
            ev_entid: "COBRO",
            ev_entid_id: cobro.CoId.ToString(),
            ev_resul: "CORRECTO",
            ev_direc_ip: null,
            ev_agent_usua: null,
            ev_metad_json: null);
    }

    private async Task<CobroRespuestaDto> MapearAsync(Cobro c)
    {
        var prestacion = await _repo.ObtenerPrestacionAsync(c.PrId);

        var totalCobrado = await _repo.TotalCobradoAsync(c.PrId);

        var totalPrestacion = prestacion?.PrTotal ?? 0m;

        return new CobroRespuestaDto
        {
            Id = c.CoId,
            PrestacionId = c.PrId,
            FormaPagoId = c.FoId,
            UsuarioId = c.UsId,
            FechaPago = c.CoFechaPago,
            Valor = c.CoValor,
            Referencia = c.CoRefe,
            Observaciones = c.CoObser,
            Estado = c.CoEstad,
            TotalPrestacion = totalPrestacion,
            TotalCobrado = totalCobrado,
            SaldoPendiente = Math.Max(
                0m, totalPrestacion - totalCobrado),
            EstadoPrestacion = prestacion?.PrEstad ?? ""
        };
    }
}
