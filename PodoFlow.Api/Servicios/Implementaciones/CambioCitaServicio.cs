
using System.Data;
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.DTOs.CambioCita;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones;

public class CambioCitaServicio : ICambioCitaServicio
{
    private readonly ICambioCitaRepositorio _repositorio;
    private readonly IEventoAuditoriaServicio _auditoria;
    private readonly PodoFlowContext _context;

    private static readonly string[] EstadosValidos =
    {
        "SOLICITADA",
        "CONFIRMADA",
        "ATENDIDA",
        "CANCELADA",
        "REPROGRAMADA",
        "NO_ASISTE"
    };

    public CambioCitaServicio(
        ICambioCitaRepositorio repositorio,
        IEventoAuditoriaServicio auditoria,
        PodoFlowContext context)
    {
        _repositorio = repositorio;
        _auditoria = auditoria;
        _context = context;
    }

    public async Task<CambioCitaRespuestaDto> CrearAsync(
        CambioCitaCrearDto dto)
    {
        if (dto.CitaId <= 0)
            throw new ArgumentException("El ID de cita es inválido.");

        if (dto.UsuarioId.HasValue &&
            dto.UsuarioId.Value <= 0)
            throw new ArgumentException("El usuario es inválido.");

        var anterior = NormalizarEstado(dto.EstadoAnterior);
        var nuevo = NormalizarEstado(dto.EstadoNuevo);

        if (anterior == null && nuevo == null &&
            dto.FechaAnterior == dto.FechaNueva)
            throw new ArgumentException(
                "Debes indicar un cambio de estado o fecha.");

        if (anterior == nuevo &&
            dto.FechaAnterior == dto.FechaNueva)
            throw new ArgumentException(
                "No se detectó ningún cambio.");

        if (dto.Motivo?.Trim().Length > 500)
            throw new ArgumentException(
                "El motivo supera 500 caracteres.");

        if (dto.UsuarioId.HasValue)
        {
            var usuarioExiste = await _context.Usuarios
                .AnyAsync(x => x.UsId == dto.UsuarioId.Value);

            if (!usuarioExiste)
                throw new ArgumentException(
                    "El usuario responsable no existe.");
        }

        var cambio = new CambioCita
        {
            CiId = dto.CitaId,
            UsId = dto.UsuarioId,
            CaEstadAnte = anterior,
            CaEstadNuev = nuevo,
            CaFechaAnte = dto.FechaAnterior,
            CaFechaNuev = dto.FechaNueva,
            CaMotivo = dto.Motivo?.Trim(),
            CaFechaCamb = DateTime.UtcNow
        };

        // Una transacción propia, solo cuando no existe una
        // transacción exterior coordinada por el CRUD de Cita.
        var txExterior = _context.Database.CurrentTransaction;

        if (txExterior == null)
        {
            await using var tx =
                await _context.Database.BeginTransactionAsync();

            var creado = await _repositorio.CrearAsync(cambio);
            await AuditarAsync(creado);
            await tx.CommitAsync();

            return Mapear(creado);
        }

        var registro = await _repositorio.CrearAsync(cambio);
        await AuditarAsync(registro);
        return Mapear(registro);
    }

    public async Task<CambioCitaRespuestaDto?> ObtenerPorIdAsync(
        long id)
    {
        var cambio = await _repositorio.ObtenerPorIdAsync(id);
        return cambio == null ? null : Mapear(cambio);
    }

    public async Task<List<CambioCitaRespuestaDto>> ObtenerTodosAsync(
        long? citaId, int pagina, int tamano)
    {
        if (pagina < 1 || tamano < 1 || tamano > 100)
            throw new ArgumentException("Paginación inválida.");

        if (citaId.HasValue && citaId.Value <= 0)
            throw new ArgumentException("ID de cita inválido.");

        var cambios = await _repositorio.ObtenerTodosAsync(
            citaId, pagina, tamano);

        return cambios.Select(Mapear).ToList();
    }

    private static string? NormalizarEstado(string? estado)
    {
        if (string.IsNullOrWhiteSpace(estado))
            return null;

        var valor = estado.Trim().ToUpperInvariant();

        if (!EstadosValidos.Contains(valor))
            throw new ArgumentException(
                $"Estado de cita inválido: {valor}");

        return valor;
    }

    private async Task AuditarAsync(CambioCita cambio)
    {
        await _auditoria.RegistrarAsync(
            us_id: cambio.UsId,
            pa_id: null,
            ev_modulo: "AGENDA",
            ev_accion: "REGISTRAR_CAMBIO",
            ev_entid: "CAMBIO_CITA",
            ev_entid_id: cambio.CaId.ToString(),
            ev_resul: "CORRECTO",
            ev_direc_ip: null,
            ev_agent_usua: null,
            ev_metad_json: null
        );
    }

    private static CambioCitaRespuestaDto Mapear(CambioCita c)
    {
        return new CambioCitaRespuestaDto
        {
            Id = c.CaId,
            CitaId = c.CiId,
            UsuarioId = c.UsId,
            EstadoAnterior = c.CaEstadAnte,
            EstadoNuevo = c.CaEstadNuev,
            FechaAnterior = c.CaFechaAnte,
            FechaNueva = c.CaFechaNuev,
            Motivo = c.CaMotivo,
            FechaCambio = c.CaFechaCamb
        };
    }
}
