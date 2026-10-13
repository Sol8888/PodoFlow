
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.DTOs.IntentoAcceso;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones;

public class IntentoAccesoServicio : IIntentoAccesoServicio
{
    private readonly IIntentoAccesoRepositorio _repositorio;
    private readonly PodoFlowContext _context;

    public IntentoAccesoServicio(
        IIntentoAccesoRepositorio repositorio,
        PodoFlowContext context)
    {
        _repositorio = repositorio;
        _context = context;
    }

    public async Task<IntentoAccesoRespuestaDto> CrearAsync(
        IntentoAccesoCrearDto dto,
        string? direccionIp,
        string? agenteUsuario)
    {
        if (dto.UsuarioId.HasValue && dto.UsuarioId.Value <= 0)
        {
            throw new ArgumentException(
                "El ID del usuario debe ser válido.");
        }

        if (dto.UsuarioId.HasValue)
        {
            var existe = await _context.Usuarios
                .AnyAsync(x => x.UsId == dto.UsuarioId.Value);

            if (!existe)
            {
                throw new ArgumentException(
                    "El usuario indicado no existe.");
            }
        }

        var identificador = dto.IdentificadorAcceso?.Trim();

        if (identificador?.Length > 320)
            throw new ArgumentException(
                "El identificador supera 320 caracteres.");

        if (dto.Motivo?.Length > 300)
            throw new ArgumentException(
                "El motivo supera 300 caracteres.");

        var intento = new IntentoAcceso
        {
            UsId = dto.UsuarioId,
            InIdentAcce = identificador,
            InFechaHora = DateTime.UtcNow,
            InExitoso = dto.Exitoso,
            InMotivo = dto.Motivo?.Trim(),
            InDirecIp = Recortar(direccionIp, 45),
            InAgentUsua = Recortar(agenteUsuario, 500)
        };

        var creado = await _repositorio.CrearAsync(intento);

        return MapearRespuesta(creado);
    }

    public async Task<IntentoAccesoRespuestaDto?> ObtenerPorIdAsync(
        long id)
    {
        var intento = await _repositorio.ObtenerPorIdAsync(id);

        return intento == null
            ? null
            : MapearRespuesta(intento);
    }

    public async Task<List<IntentoAccesoRespuestaDto>> ObtenerTodosAsync(
        long? usuarioId,
        bool? exitoso,
        int pagina,
        int tamano)
    {
        var intentos = await _repositorio.ObtenerTodosAsync(
            usuarioId, exitoso, pagina, tamano);

        return intentos.Select(MapearRespuesta).ToList();
    }

    private static string? Recortar(
        string? valor, int longitudMaxima)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return null;

        valor = valor.Trim();

        return valor.Length <= longitudMaxima
            ? valor
            : valor[..longitudMaxima];
    }

    private static IntentoAccesoRespuestaDto MapearRespuesta(
        IntentoAcceso intento)
    {
        return new IntentoAccesoRespuestaDto
        {
            Id = intento.InId,
            UsuarioId = intento.UsId,
            IdentificadorAcceso = intento.InIdentAcce,
            FechaHora = intento.InFechaHora,
            Exitoso = intento.InExitoso,
            Motivo = intento.InMotivo,
            DireccionIp = intento.InDirecIp,
            AgenteUsuario = intento.InAgentUsua
        };
    }
}
