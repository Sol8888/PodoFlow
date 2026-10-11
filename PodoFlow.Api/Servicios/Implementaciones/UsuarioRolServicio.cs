
using PodoFlow.Api.DTOs.UsuarioRol;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones;

public class UsuarioRolServicio : IUsuarioRolServicio
{
    private readonly IUsuarioRolRepositorio _repositorio;

    public UsuarioRolServicio(
        IUsuarioRolRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<List<UsuarioRolRespuestaDto>> ObtenerTodosAsync(
        bool incluirInactivos, int pagina, int tamano)
    {
        var relaciones = await _repositorio.ObtenerTodosAsync(
            incluirInactivos, pagina, tamano);

        return relaciones.Select(MapearRespuesta).ToList();
    }

    public async Task<UsuarioRolRespuestaDto?> ObtenerPorIdAsync(
        long usuarioId, int rolId)
    {
        var relacion = await _repositorio.ObtenerPorIdAsync(
            usuarioId, rolId);

        return relacion == null
            ? null
            : MapearRespuesta(relacion);
    }

    public async Task<UsuarioRolRespuestaDto> CrearAsync(
        UsuarioRolCrearDto dto)
    {
        await ValidarUsuarioYRolAsync(dto.UsuarioId, dto.RolId);

        var existente = await _repositorio.ObtenerPorIdAsync(
            dto.UsuarioId, dto.RolId);

        if (existente != null)
        {
            if (existente.UrEstatus)
            {
                throw new InvalidOperationException(
                    "El usuario ya tiene asignado este rol.");
            }

            // Recuperar relación previamente desactivada.
            existente.UrEstatus = true;

            await _repositorio.GuardarCambiosAsync();

            return MapearRespuesta(existente);
        }

        var relacion = new UsuarioRol
        {
            UsId = dto.UsuarioId,
            RoId = dto.RolId,
            UrFechaAsig = DateTime.UtcNow,
            UrEstatus = true
        };

        var creada = await _repositorio.CrearAsync(relacion);

        return MapearRespuesta(creada);
    }

    public async Task<bool> ActualizarAsync(
        long usuarioId,
        int rolId,
        UsuarioRolActualizarDto dto)
    {
        var actual = await _repositorio.ObtenerPorIdAsync(
            usuarioId, rolId);

        if (actual == null)
            return false;

        if (!actual.UrEstatus)
        {
            throw new InvalidOperationException(
                "No se puede cambiar una asignación inactiva.");
        }

        await ValidarUsuarioYRolAsync(
            usuarioId, dto.NuevoRolId);

        if (rolId == dto.NuevoRolId)
            return true;

        var nueva = await _repositorio.ObtenerPorIdAsync(
            usuarioId, dto.NuevoRolId);

        if (nueva != null && nueva.UrEstatus)
        {
            throw new InvalidOperationException(
                "El usuario ya tiene asignado el nuevo rol.");
        }

        await _repositorio.CambiarRolAsync(
            actual, nueva, dto.NuevoRolId);

        return true;
    }

    public async Task<bool> EliminarAsync(
        long usuarioId, int rolId)
    {
        var relacion = await _repositorio.ObtenerPorIdAsync(
            usuarioId, rolId);

        if (relacion == null)
            return false;

        if (!relacion.UrEstatus)
            return true;

        relacion.UrEstatus = false;

        await _repositorio.GuardarCambiosAsync();

        return true;
    }

    public async Task<bool> ReactivarAsync(
        long usuarioId, int rolId)
    {
        var relacion = await _repositorio.ObtenerPorIdAsync(
            usuarioId, rolId);

        if (relacion == null)
            return false;

        await ValidarUsuarioYRolAsync(usuarioId, rolId);

        if (relacion.UrEstatus)
            return true;

        relacion.UrEstatus = true;

        await _repositorio.GuardarCambiosAsync();

        return true;
    }

    private async Task ValidarUsuarioYRolAsync(
        long usuarioId, int rolId)
    {
        if (!await _repositorio.UsuarioActivoAsync(usuarioId))
        {
            throw new ArgumentException(
                "El usuario no existe o está inactivo.");
        }

        if (!await _repositorio.RolActivoAsync(rolId))
        {
            throw new ArgumentException(
                "El rol no existe o está inactivo.");
        }
    }

    private static UsuarioRolRespuestaDto MapearRespuesta(
        UsuarioRol relacion)
    {
        return new UsuarioRolRespuestaDto
        {
            UsuarioId = relacion.UsId,
            RolId = relacion.RoId,
            FechaAsignacion = relacion.UrFechaAsig,
            Estatus = relacion.UrEstatus
        };
    }
}
