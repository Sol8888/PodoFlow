
using System.Data;
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.DTOs.Prestacion;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones;

public class PrestacionServicio : IPrestacionServicio
{
    private const decimal MaximoTotal = 9999999999.99m;

    private readonly IPrestacionRepositorio _repo;
    private readonly IEventoAuditoriaServicio _auditoria;
    private readonly PodoFlowContext _context;

    public PrestacionServicio(
        IPrestacionRepositorio repo,
        IEventoAuditoriaServicio auditoria,
        PodoFlowContext context)
    {
        _repo = repo;
        _auditoria = auditoria;
        _context = context;
    }

    public async Task<List<PrestacionRespuestaDto>> ObtenerTodosAsync(
        long? pacienteId,
        string? estado,
        int pagina,
        int tamano)
    {
        if (pagina < 1 || tamano < 1 || tamano > 100)
            throw new ArgumentException(
                "Paginación inválida. El tamaño máximo es 100.");

        if (pacienteId.HasValue && pacienteId.Value <= 0)
            throw new ArgumentException("Paciente inválido.");

        string? filtro = null;

        if (!string.IsNullOrWhiteSpace(estado))
        {
            filtro = estado.Trim().ToUpperInvariant();

            if (!EstadoValido(filtro))
                throw new ArgumentException("Estado inválido.");
        }

        var datos = await _repo.ObtenerTodosAsync(
            pacienteId, filtro, pagina, tamano);

        return datos.Select(Mapear).ToList();
    }

    public async Task<PrestacionRespuestaDto?> ObtenerPorIdAsync(
        long id)
    {
        if (id <= 0)
            throw new ArgumentException("ID inválido.");

        var prestacion = await _repo.ObtenerPorIdAsync(id);

        return prestacion == null ? null : Mapear(prestacion);
    }

    public async Task<PrestacionRespuestaDto> CrearAsync(
        PrestacionCrearDto dto)
    {
        ValidarImportes(
            dto.PrecioAplicado,
            dto.Cantidad,
            dto.Observaciones);

        if (dto.PacienteId <= 0 ||
            dto.ServicioId <= 0 ||
            dto.UsuarioId <= 0)
            throw new ArgumentException(
                "Paciente, servicio y usuario son obligatorios.");

        if (dto.CitaId.HasValue && dto.CitaId.Value <= 0)
            throw new ArgumentException("Cita inválida.");

        if (dto.AtencionId.HasValue && dto.AtencionId.Value <= 0)
            throw new ArgumentException("Atención inválida.");

        await using var tx =
            await _context.Database.BeginTransactionAsync();

        await ValidarRelacionesAsync(dto);

        var prestacion = new Prestacion
        {
            PaId = dto.PacienteId,
            CiId = dto.CitaId,
            AtId = dto.AtencionId,
            SeId = dto.ServicioId,
            UsId = dto.UsuarioId,
            PrFechaPres = DateTime.UtcNow,
            PrPrecioApli = dto.PrecioAplicado,
            PrCant = dto.Cantidad,
            PrEstad = "PENDIENTE",
            PrObser = Limpiar(dto.Observaciones)
        };

        await _repo.AgregarAsync(prestacion);
        await _repo.GuardarCambiosAsync();

        // Obtiene pr_total calculado por SQL Server.
        await _context.Entry(prestacion).ReloadAsync();

        await AuditarAsync(prestacion, "CREAR");

        await tx.CommitAsync();

        return Mapear(prestacion);
    }

    public async Task<bool> ActualizarAsync(
        long id,
        PrestacionActualizarDto dto)
    {
        ValidarImportes(
            dto.PrecioAplicado,
            dto.Cantidad,
            dto.Observaciones);

        await using var tx =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        var prestacion = await _repo.ObtenerPorIdAsync(id);

        if (prestacion == null)
            return false;

        if (prestacion.PrEstad != "PENDIENTE")
            throw new InvalidOperationException(
                "Solo se pueden editar prestaciones pendientes.");

        // No se debe cambiar el importe cuando ya hay
        // cobros registrados, incluso si la prestación
        // aún aparece como PENDIENTE.
        if (await TieneCobrosRegistradosAsync(id))
            throw new InvalidOperationException(
                "La prestación ya tiene cobros registrados. " +
                "No se puede modificar su importe.");

        prestacion.PrPrecioApli = dto.PrecioAplicado;
        prestacion.PrCant = dto.Cantidad;
        prestacion.PrObser = Limpiar(dto.Observaciones);

        await _repo.GuardarCambiosAsync();
        await _context.Entry(prestacion).ReloadAsync();

        await AuditarAsync(prestacion, "ACTUALIZAR");

        await tx.CommitAsync();

        return true;
    }

    public async Task<bool> CambiarEstadoAsync(
        long id,
        PrestacionCambiarEstadoDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.NuevoEstado))
            throw new ArgumentException(
                "El estado es obligatorio.");

        if (string.IsNullOrWhiteSpace(dto.Motivo) ||
            dto.Motivo.Trim().Length > 1000)
            throw new ArgumentException(
                "El motivo es obligatorio (máximo 1000 caracteres).");

        var nuevo = dto.NuevoEstado.Trim().ToUpperInvariant();

        if (!EstadoValido(nuevo))
            throw new ArgumentException(
                "Estado permitido: PENDIENTE, PAGADA o ANULADA.");

        // PAGADA será asignado por el flujo de COBRO.
        // No se acepta desde este endpoint genérico.
        if (nuevo != "ANULADA")
            throw new InvalidOperationException(
                "Este endpoint solo permite ANULADA. " +
                "El estado PAGADA se gestionará mediante cobros.");

        await using var tx =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        var prestacion = await _repo.ObtenerPorIdAsync(id);

        if (prestacion == null)
            return false;

        if (prestacion.PrEstad == "ANULADA")
            return true;

        if (prestacion.PrEstad != "PENDIENTE")
            throw new InvalidOperationException(
                "No se puede anular una prestación pagada.");

        if (await TieneCobrosRegistradosAsync(id))
            throw new InvalidOperationException(
                "No se puede anular una prestación con " +
                "cobros vigentes. Revisa los cobros primero.");

        prestacion.PrEstad = "ANULADA";

        // SQL no tiene una columna para motivo de anulación.
        // Lo incorporamos a observaciones si cabe.
        var motivo = "[ANULACION] " + dto.Motivo.Trim();
        var anterior = prestacion.PrObser;

        var observacion = string.IsNullOrWhiteSpace(anterior)
            ? motivo
            : anterior + " | " + motivo;

        if (observacion.Length > 1000)
            throw new InvalidOperationException(
                "No hay espacio suficiente para registrar " +
                "el motivo en observaciones.");

        prestacion.PrObser = observacion;

        await _repo.GuardarCambiosAsync();
        await AuditarAsync(prestacion, "ANULAR");

        await tx.CommitAsync();

        return true;
    }

    private async Task ValidarRelacionesAsync(
        PrestacionCrearDto dto)
    {
        if (!await _repo.PacienteActivoAsync(dto.PacienteId))
            throw new ArgumentException(
                "El paciente no existe o está inactivo.");

        if (!await _repo.UsuarioActivoAsync(dto.UsuarioId))
            throw new ArgumentException(
                "El usuario no existe o está inactivo.");

        if (!await _repo.ServicioExisteAsync(dto.ServicioId))
            throw new ArgumentException(
                "El servicio no existe.");

        if (dto.CitaId.HasValue)
        {
            var cita = await _repo.ObtenerCitaAsync(
                dto.CitaId.Value);

            if (cita == null)
                throw new ArgumentException("La cita no existe.");

            if (cita.PaId != dto.PacienteId)
                throw new InvalidOperationException(
                    "La cita pertenece a otro paciente.");
        }

        if (dto.AtencionId.HasValue)
        {
            var atencion = await _repo.ObtenerAtencionAsync(
                dto.AtencionId.Value);

            if (atencion == null)
                throw new ArgumentException(
                    "La atención no existe.");

            var historia = await _repo.ObtenerHistoriaAsync(
                atencion.HiId);

            if (historia == null ||
                historia.PaId != dto.PacienteId)
                throw new InvalidOperationException(
                    "La atención pertenece a otro paciente.");

            if (dto.CitaId.HasValue &&
                atencion.CiId != dto.CitaId.Value)
                throw new InvalidOperationException(
                    "La cita no corresponde a la atención.");
        }
    }

    private async Task<bool> TieneCobrosRegistradosAsync(
        long prestacionId)
    {
        // Consulta SQL directa porque el CRUD COBRO
        // será implementado en el siguiente ticket.
        var cantidad = await _context.Database
            .SqlQueryRaw<int>(
                """
                SELECT COUNT(*) AS [Value]
                FROM fin.COBRO
                WHERE pr_id = {0}
                  AND co_estad = 'REGISTRADO'
                """,
                prestacionId)
            .SingleAsync();

        return cantidad > 0;
    }

    private static bool EstadoValido(string estado)
    {
        return estado is "PENDIENTE" or "PAGADA" or "ANULADA";
    }

    private static string? Limpiar(string? texto)
    {
        return string.IsNullOrWhiteSpace(texto)
            ? null : texto.Trim();
    }

    private static void ValidarImportes(
        decimal precio,
        decimal cantidad,
        string? observaciones)
    {
        if (precio < 0 ||
            precio > 9999999999.99m ||
            decimal.Round(precio, 2) != precio)
            throw new ArgumentException(
                "Precio inválido; usa hasta dos decimales.");

        if (cantidad <= 0 ||
            cantidad > 99999999.99m ||
            decimal.Round(cantidad, 2) != cantidad)
            throw new ArgumentException(
                "Cantidad inválida; usa hasta dos decimales.");

        // Validar el total antes de enviarlo a SQL.
        if (precio * cantidad > MaximoTotal)
            throw new ArgumentException(
                "El total supera DECIMAL(12,2).");

        if (observaciones?.Trim().Length > 1000)
            throw new ArgumentException(
                "Observaciones: máximo 1000 caracteres.");
    }

    private async Task AuditarAsync(
        Prestacion prestacion,
        string accion)
    {
        await _auditoria.RegistrarAsync(
            us_id: null,
            pa_id: prestacion.PaId,
            ev_modulo: "FINANZAS",
            ev_accion: accion,
            ev_entid: "PRESTACION",
            ev_entid_id: prestacion.PrId.ToString(),
            ev_resul: "CORRECTO",
            ev_direc_ip: null,
            ev_agent_usua: null,
            ev_metad_json: null);
    }

    private static PrestacionRespuestaDto Mapear(
        Prestacion p)
    {
        return new PrestacionRespuestaDto
        {
            Id = p.PrId,
            PacienteId = p.PaId,
            CitaId = p.CiId,
            AtencionId = p.AtId,
            ServicioId = p.SeId,
            UsuarioId = p.UsId,
            FechaPrestacion = p.PrFechaPres,
            PrecioAplicado = p.PrPrecioApli,
            Cantidad = p.PrCant,
            Total = p.PrTotal,
            Estado = p.PrEstad,
            Observaciones = p.PrObser
        };
    }
}
