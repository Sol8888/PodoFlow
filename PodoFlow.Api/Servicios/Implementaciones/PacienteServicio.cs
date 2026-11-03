
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
        ArgumentNullException.ThrowIfNull(dto);

        // ============================================
        // TIPO DE IDENTIFICACIÓN
        // ============================================

        string tipo = dto.TipoIdentificacion?
            .Trim()
            .ToUpperInvariant() ?? "";

        if (!TiposValidos.Contains(tipo))
        {
            throw new ArgumentException(
                "Tipo de identificación inválido. " +
                "Valores permitidos: CEDULA, PASAPORTE, RUC u OTRO.");
        }

        // ============================================
        // IDENTIFICACIÓN OBLIGATORIA
        // ============================================

        string identificacion = dto.Identificacion?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(identificacion))
        {
            throw new ArgumentException(
                "La identificación del paciente es obligatoria.");
        }

        if (identificacion.Length > 30)
        {
            throw new ArgumentException(
                "La identificación no puede superar los 30 caracteres.");
        }

        // ============================================
        // VALIDACIÓN DE CÉDULA ECUATORIANA
        // ============================================

        if (tipo == "CEDULA")
        {
            if (identificacion.Length != 10 ||
                !identificacion.All(char.IsAsciiDigit))
            {
                throw new ArgumentException(
                    "La cédula debe contener exactamente " +
                    "10 dígitos numéricos.");
            }

            if (!CedulaEcuatorianaValida(identificacion))
            {
                throw new ArgumentException(
                    "La cédula ecuatoriana ingresada no es válida. " +
                    "Verifica el número de identificación.");
            }
        }

        // ============================================
        // VALIDACIÓN BÁSICA DE RUC
        // ============================================

        if (tipo == "RUC")
        {
            if (identificacion.Length != 13 ||
                !identificacion.All(char.IsAsciiDigit))
            {
                throw new ArgumentException(
                    "El RUC debe contener exactamente " +
                    "13 dígitos numéricos.");
            }
        }

        // ============================================
        // NOMBRES Y APELLIDOS
        // ============================================

        if (string.IsNullOrWhiteSpace(dto.Nombres))
        {
            throw new ArgumentException(
                "Los nombres del paciente son obligatorios.");
        }

        if (string.IsNullOrWhiteSpace(dto.Apellidos))
        {
            throw new ArgumentException(
                "Los apellidos del paciente son obligatorios.");
        }

        if (dto.Nombres.Trim().Length > 100 ||
            dto.Apellidos.Trim().Length > 100)
        {
            throw new ArgumentException(
                "Los nombres y apellidos no pueden superar " +
                "los 100 caracteres respectivamente.");
        }

        // ============================================
        // FECHA DE NACIMIENTO
        // ============================================

        if (dto.FechaNacimiento.HasValue &&
            dto.FechaNacimiento.Value.Date > DateTime.UtcNow.Date)
        {
            throw new ArgumentException(
                "La fecha de nacimiento no puede ser futura.");
        }
    }

    private static bool CedulaEcuatorianaValida(string cedula)
    {
        // ============================================
        // FORMATO
        // ============================================

        if (cedula.Length != 10 ||
            !cedula.All(char.IsAsciiDigit))
        {
            return false;
        }

        // ============================================
        // PROVINCIA
        // ============================================

        int provincia = int.Parse(cedula[..2]);

        if (provincia < 1 || provincia > 24)
        {
            return false;
        }

        // ============================================
        // TERCER DÍGITO
        // ============================================

        int tercerDigito = cedula[2] - '0';

        if (tercerDigito >= 6)
        {
            return false;
        }

        // ============================================
        // COEFICIENTES
        // ============================================

        int[] coeficientes =
        {
        2, 1, 2, 1, 2, 1, 2, 1, 2
    };

        int suma = 0;

        for (int i = 0; i < 9; i++)
        {
            int digito = cedula[i] - '0';

            int producto = digito * coeficientes[i];

            if (producto >= 10)
            {
                producto -= 9;
            }

            suma += producto;
        }

        // ============================================
        // DÍGITO VERIFICADOR
        // ============================================

        int verificadorCalculado =
            (10 - (suma % 10)) % 10;

        int verificadorIngresado =
            cedula[9] - '0';

        return verificadorCalculado == verificadorIngresado;
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
