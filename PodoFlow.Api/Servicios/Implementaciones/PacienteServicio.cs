
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.DTOs.Paciente;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones;

public class PacienteServicio : IPacienteServicio
{
    private readonly IPacienteRepositorio _repo;

    private static readonly string[] TiposValidos =
    {
        "CEDULA", "PASAPORTE", "RUC", "OTRO"
    };

    public PacienteServicio(IPacienteRepositorio repo)
    {
        _repo = repo;
    }

    public async Task<List<PacienteRespuestaDto>> ObtenerTodosAsync(
        bool incluirInactivos, int pagina, int tamano)
    {
        var pacientes = await _repo.ObtenerTodosAsync(
            incluirInactivos, pagina, tamano);

        return pacientes.Select(Mapear).ToList();
    }

    public async Task<PacienteRespuestaDto?> ObtenerPorIdAsync(long id)
    {
        var paciente = await _repo.ObtenerPorIdAsync(id);
        return paciente == null ? null : Mapear(paciente);
    }

    public async Task<PacienteRespuestaDto> CrearAsync(
        PacienteCrearDto dto)
    {
        Validar(dto);

        string tipo = dto.TipoIdentificacion.Trim().ToUpperInvariant();
        string ident = dto.Identificacion.Trim().ToUpperInvariant();

        var existente = await _repo.ObtenerPorIdentificacionAsync(
            tipo, ident);

        if (existente != null)
        {
            if (existente.PaEstatus)
                throw new InvalidOperationException(
                    "Ya existe un paciente activo con esa identificación.");

            CopiarDatos(existente, dto);
            existente.PaEstatus = true;
            existente.PaFechaBaja = null;
            existente.PaMotivBaja = null;
            existente.PaFechaModi = DateTime.UtcNow;

            await _repo.GuardarCambiosAsync();
            return Mapear(existente);
        }

        var paciente = new Paciente
        {
            PaFechaCrea = DateTime.UtcNow,
            PaEstatus = true
        };

        CopiarDatos(paciente, dto);

        return Mapear(await _repo.CrearAsync(paciente));
    }

    public async Task<bool> ActualizarAsync(
        long id, PacienteActualizarDto dto)
    {
        Validar(dto);

        var paciente = await _repo.ObtenerPorIdAsync(id);
        if (paciente == null) return false;

        if (!paciente.PaEstatus)
            throw new InvalidOperationException(
                "No se puede editar un paciente inactivo.");

        byte[] version;

        try
        {
            version = Convert.FromBase64String(dto.Version);
        }
        catch (FormatException)
        {
            throw new ArgumentException("Versión inválida.");
        }

        if (version.Length != 8)
            throw new ArgumentException("Versión inválida.");

        var tipo = dto.TipoIdentificacion.Trim().ToUpperInvariant();
        var ident = dto.Identificacion.Trim().ToUpperInvariant();

        var duplicado = await _repo.ObtenerPorIdentificacionAsync(
            tipo, ident);

        if (duplicado != null && duplicado.PaId != id)
            throw new InvalidOperationException(
                "La identificación pertenece a otro paciente.");

        // EF verificará esta versión en el UPDATE SQL.
        _repo.EstablecerVersionOriginal(paciente, version);

        CopiarDatos(paciente, dto);
        paciente.PaFechaModi = DateTime.UtcNow;

        await _repo.GuardarCambiosAsync();
        return true;
    }

    public async Task<bool> EliminarAsync(long id, string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo) ||
            motivo.Trim().Length > 500)
            throw new ArgumentException(
                "El motivo de baja es obligatorio (máximo 500 caracteres).");

        var paciente = await _repo.ObtenerPorIdAsync(id);
        if (paciente == null) return false;
        if (!paciente.PaEstatus) return true;

        paciente.PaEstatus = false;
        paciente.PaFechaBaja = DateTime.UtcNow;
        paciente.PaMotivBaja = motivo.Trim();
        paciente.PaFechaModi = DateTime.UtcNow;

        await _repo.GuardarCambiosAsync();
        return true;
    }

    public async Task<bool> ReactivarAsync(long id)
    {
        var paciente = await _repo.ObtenerPorIdAsync(id);
        if (paciente == null) return false;
        if (paciente.PaEstatus) return true;

        paciente.PaEstatus = true;
        paciente.PaFechaBaja = null;
        paciente.PaMotivBaja = null;
        paciente.PaFechaModi = DateTime.UtcNow;

        await _repo.GuardarCambiosAsync();
        return true;
    }

    private static void Validar(PacienteCrearDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.TipoIdentificacion) ||
            !TiposValidos.Contains(
                dto.TipoIdentificacion.Trim().ToUpperInvariant()))
            throw new ArgumentException(
                "Tipo válido: CEDULA, PASAPORTE, RUC u OTRO.");

        if (string.IsNullOrWhiteSpace(dto.Identificacion) ||
            string.IsNullOrWhiteSpace(dto.Nombres) ||
            string.IsNullOrWhiteSpace(dto.Apellidos))
            throw new ArgumentException(
                "Identificación, nombres y apellidos son obligatorios.");

        if (dto.FechaNacimiento.HasValue &&
            dto.FechaNacimiento.Value.Date > DateTime.UtcNow.Date)
            throw new ArgumentException(
                "La fecha de nacimiento no puede ser futura.");
    }

    private static void CopiarDatos(
        Paciente paciente, PacienteCrearDto dto)
    {
        paciente.PaTipoIdent =
            dto.TipoIdentificacion.Trim().ToUpperInvariant();
        paciente.PaIdent =
            dto.Identificacion.Trim().ToUpperInvariant();
        paciente.PaNombres = dto.Nombres.Trim();
        paciente.PaApelli = dto.Apellidos.Trim();
        paciente.PaFechaNaci = dto.FechaNacimiento?.Date;
        paciente.PaGenero = dto.Genero?.Trim();
        paciente.PaDirec = dto.Direccion?.Trim();
    }

    private static PacienteRespuestaDto Mapear(Paciente p)
    {
        return new PacienteRespuestaDto
        {
            Id = p.PaId,
            TipoIdentificacion = p.PaTipoIdent,
            Identificacion = p.PaIdent,
            Nombres = p.PaNombres,
            Apellidos = p.PaApelli,
            FechaNacimiento = p.PaFechaNaci,
            Genero = p.PaGenero,
            Direccion = p.PaDirec,
            FechaCreacion = p.PaFechaCrea,
            FechaModificacion = p.PaFechaModi,
            FechaBaja = p.PaFechaBaja,
            MotivoBaja = p.PaMotivBaja,
            Estatus = p.PaEstatus,
            Version = Convert.ToBase64String(p.PaVersion)
        };
    }
}
