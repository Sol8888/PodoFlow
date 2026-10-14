
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.DTOs.Movimiento;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones;

public class MovimientoServicio : IMovimientoServicio
{
    private const decimal MaximoStock = 9999999999.99m;

    private readonly IMovimientoRepositorio _repo;
    private readonly IEventoAuditoriaServicio _auditoria;
    private readonly PodoFlowContext _context;

    public MovimientoServicio(
        IMovimientoRepositorio repo,
        IEventoAuditoriaServicio auditoria,
        PodoFlowContext context)
    {
        _repo = repo;
        _auditoria = auditoria;
        _context = context;
    }

    public async Task<List<MovimientoRespuestaDto>> ObtenerTodosAsync(
        int? insumoId,
        long? atencionId,
        string? tipo,
        int pagina,
        int tamano)
    {
        if (pagina < 1 || tamano < 1 || tamano > 100)
            throw new ArgumentException(
                "La página debe ser mayor que cero y " +
                "el tamaño debe estar entre 1 y 100.");

        if (insumoId.HasValue && insumoId.Value <= 0)
            throw new ArgumentException("Insumo inválido.");

        if (atencionId.HasValue && atencionId.Value <= 0)
            throw new ArgumentException("Atención inválida.");

        string? tipoNormalizado = null;

        if (!string.IsNullOrWhiteSpace(tipo))
        {
            tipoNormalizado = tipo.Trim().ToUpperInvariant();

            if (!TipoValido(tipoNormalizado))
                throw new ArgumentException(
                    "Tipo de movimiento inválido.");
        }

        var datos = await _repo.ObtenerTodosAsync(
            insumoId,
            atencionId,
            tipoNormalizado,
            pagina,
            tamano);

        return datos.Select(Mapear).ToList();
    }

    public async Task<MovimientoRespuestaDto?> ObtenerPorIdAsync(
        long id)
    {
        if (id <= 0)
            throw new ArgumentException("ID inválido.");

        var movimiento = await _repo.ObtenerPorIdAsync(id);

        return movimiento == null ? null : Mapear(movimiento);
    }

    public async Task<MovimientoRespuestaDto> CrearAsync(
        MovimientoCrearDto dto)
    {
        ValidarDatos(dto);

        var tipo = dto.Tipo.Trim().ToUpperInvariant();
        var motivo = dto.Motivo.Trim();

        // Utilizamos una transacción serializable para que
        // la lectura del stock y su actualización sean
        // consistentes frente a operaciones simultáneas.
        try
        {
            await using var tx =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            var insumo =
                await _repo.ObtenerInsumoAsync(dto.InsumoId);

            if (insumo == null)
                throw new ArgumentException(
                    "El insumo no existe.");

            if (!insumo.InEstatus)
                throw new InvalidOperationException(
                    "No puedes registrar movimientos " +
                    "de un insumo inactivo.");

            if (!await _repo.UsuarioActivoAsync(dto.UsuarioId))
                throw new ArgumentException(
                    "El usuario no existe o está inactivo.");

            if (dto.AtencionId.HasValue &&
                !await _repo.AtencionExisteAsync(
                    dto.AtencionId.Value))
            {
                throw new ArgumentException(
                    "La atención indicada no existe.");
            }

            decimal stockAnterior = insumo.InStockActu;

            bool esEntrada =
                tipo == "INGRESO" || tipo == "AJUSTE_POS";

            decimal variacion =
                esEntrada ? dto.Cantidad : -dto.Cantidad;

            decimal stockNuevo =
                stockAnterior + variacion;

            if (stockNuevo < 0)
            {
                throw new InvalidOperationException(
                    $"Stock insuficiente. Disponible: " +
                    $"{stockAnterior:0.##}; " +
                    $"cantidad solicitada: {dto.Cantidad:0.##}.");
            }

            if (stockNuevo > MaximoStock)
            {
                throw new InvalidOperationException(
                    "El movimiento supera el límite " +
                    "DECIMAL(12,2) del stock.");
            }

            var fecha = DateTime.UtcNow;

            int filas = await _repo.ActualizarStockAsync(
                insumo.InId,
                insumo.InVersion,
                stockAnterior,
                stockNuevo,
                fecha);

            if (filas != 1)
            {
                throw new InvalidOperationException(
                    "El stock cambió durante la operación. " +
                    "Consulta el insumo y vuelve a intentarlo.");
            }

            var movimiento = new Movimiento
            {
                InId = dto.InsumoId,
                AtId = dto.AtencionId,
                UsId = dto.UsuarioId,
                MoTipo = tipo,
                MoCant = dto.Cantidad,
                MoStockAnte = stockAnterior,
                MoStockNuev = stockNuevo,
                MoFechaHora = fecha,
                MoMotivo = motivo
            };

            await _repo.AgregarAsync(movimiento);
            await _repo.GuardarCambiosAsync();

            // Auditoría en el mismo DbContext scoped
            // y dentro de la misma transacción.
            await _auditoria.RegistrarAsync(
                us_id: dto.UsuarioId,
                pa_id: null,
                ev_modulo: "INVENTARIO",
                ev_accion: "CREAR",
                ev_entid: "MOVIMIENTO",
                ev_entid_id: movimiento.MoId.ToString(),
                ev_resul: "CORRECTO",
                ev_direc_ip: null,
                ev_agent_usua: null,
                ev_metad_json: null);

            await tx.CommitAsync();

            return Mapear(movimiento);
        }
        catch (SqlException ex) when (ex.Number == 1205)
        {
            // Deadlock: SQL Server canceló esta transacción.
            // El cliente puede consultar y reintentar.
            throw new InvalidOperationException(
                "Otro movimiento se procesó simultáneamente. " +
                "Consulta el stock actual y vuelve a intentarlo.",
                ex);
        }
    }

    private static void ValidarDatos(MovimientoCrearDto dto)
    {
        if (dto.InsumoId <= 0)
            throw new ArgumentException("Insumo inválido.");

        if (dto.UsuarioId <= 0)
            throw new ArgumentException("Usuario inválido.");

        if (dto.AtencionId.HasValue && dto.AtencionId <= 0)
            throw new ArgumentException("Atención inválida.");

        if (string.IsNullOrWhiteSpace(dto.Tipo) ||
            !TipoValido(dto.Tipo.Trim().ToUpperInvariant()))
        {
            throw new ArgumentException(
                "Tipo permitido: INGRESO, CONSUMO, " +
                "AJUSTE_POS o AJUSTE_NEG.");
        }

        if (dto.Cantidad <= 0 ||
            dto.Cantidad > MaximoStock ||
            decimal.Round(dto.Cantidad, 2) != dto.Cantidad)
        {
            throw new ArgumentException(
                "La cantidad debe ser positiva, " +
                "con máximo dos decimales y dentro " +
                "del rango DECIMAL(12,2).");
        }

        if (string.IsNullOrWhiteSpace(dto.Motivo) ||
            dto.Motivo.Trim().Length > 500)
        {
            throw new ArgumentException(
                "El motivo es obligatorio " +
                "(máximo 500 caracteres).");
        }
    }

    private static bool TipoValido(string tipo)
    {
        return tipo is
            "INGRESO" or
            "CONSUMO" or
            "AJUSTE_POS" or
            "AJUSTE_NEG";
    }

    private static MovimientoRespuestaDto Mapear(Movimiento m)
    {
        return new MovimientoRespuestaDto
        {
            Id = m.MoId,
            InsumoId = m.InId,
            AtencionId = m.AtId,
            UsuarioId = m.UsId,
            Tipo = m.MoTipo,
            Cantidad = m.MoCant,
            StockAnterior = m.MoStockAnte,
            StockNuevo = m.MoStockNuev,
            FechaHora = m.MoFechaHora,
            Motivo = m.MoMotivo
        };
    }
}
