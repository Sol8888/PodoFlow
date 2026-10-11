
using PodoFlow.Api.DTOs.RolPermiso;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones;

public class RolPermisoServicio : IRolPermisoServicio
{
    private readonly IRolPermisoRepositorio _repositorio;

    public RolPermisoServicio(
        IRolPermisoRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<List<RolPermisoRespuestaDto>> ObtenerTodosAsync(
        bool incluirInactivos, int pagina, int tamano)
    {
        var relaciones = await _repositorio.ObtenerTodosAsync(
            incluirInactivos, pagina, tamano);

        return relaciones.Select(MapearRespuesta).ToList();
    }

    public async Task<RolPermisoRespuestaDto?> ObtenerPorIdAsync(
        int rolId, int permisoId)
    {
        var relacion = await _repositorio.ObtenerPorIdAsync(
            rolId, permisoId);

        return relacion == null
            ? null
            : MapearRespuesta(relacion);
    }

    public async Task<RolPermisoRespuestaDto> CrearAsync(
        RolPermisoCrearDto dto)
    {
        await ValidarRolYPermisoAsync(dto.RolId, dto.PermisoId);

        var existente = await _repositorio.ObtenerPorIdAsync(
            dto.RolId, dto.PermisoId);

        if (existente != null)
        {
            if (existente.RpEstatus)
                throw new InvalidOperationException(
                    "Este rol ya tiene asignado el permiso.");

            existente.RpEstatus = true;

            await _repositorio.GuardarCambiosAsync();
            return MapearRespuesta(existente);
        }

        var relacion = new RolPermiso
        {
            RoId = dto.RolId,
            PeId = dto.PermisoId,
            RpFechaAsig = DateTime.UtcNow,
            RpEstatus = true
        };

        var creada = await _repositorio.CrearAsync(relacion);
        return MapearRespuesta(creada);
    }

    public async Task<bool> ActualizarAsync(
        int rolId,
        int permisoId,
        RolPermisoActualizarDto dto)
    {
        var anterior = await _repositorio.ObtenerPorIdAsync(
            rolId, permisoId);

        if (anterior == null)
            return false;

        if (!anterior.RpEstatus)
            throw new InvalidOperationException(
                "No se puede cambiar una asignación inactiva.");

        await ValidarRolYPermisoAsync(rolId, dto.NuevoPermisoId);

        if (permisoId == dto.NuevoPermisoId)
            return true;

        var nueva = await _repositorio.ObtenerPorIdAsync(
            rolId, dto.NuevoPermisoId);

        if (nueva != null && nueva.RpEstatus)
            throw new InvalidOperationException(
                "El rol ya tiene el nuevo permiso.");

        await _repositorio.CambiarPermisoAsync(
            anterior, nueva, dto.NuevoPermisoId);

        return true;
    }

    public async Task<bool> EliminarAsync(int rolId, int permisoId)
    {
        var relacion = await _repositorio.ObtenerPorIdAsync(
            rolId, permisoId);

        if (relacion == null)
            return false;

        if (!relacion.RpEstatus)
            return true;

        relacion.RpEstatus = false;

        await _repositorio.GuardarCambiosAsync();
        return true;
    }

    public async Task<bool> ReactivarAsync(int rolId, int permisoId)
    {
        var relacion = await _repositorio.ObtenerPorIdAsync(
            rolId, permisoId);

        if (relacion == null)
            return false;

        await ValidarRolYPermisoAsync(rolId, permisoId);

        if (relacion.RpEstatus)
            return true;

        relacion.RpEstatus = true;

        await _repositorio.GuardarCambiosAsync();
        return true;
    }

    private async Task ValidarRolYPermisoAsync(
        int rolId, int permisoId)
    {
        if (!await _repositorio.RolActivoAsync(rolId))
            throw new ArgumentException(
                "El rol no existe o está inactivo.");

        if (!await _repositorio.PermisoActivoAsync(permisoId))
            throw new ArgumentException(
                "El permiso no existe o está inactivo.");
    }

    private static RolPermisoRespuestaDto MapearRespuesta(
        RolPermiso relacion)
    {
        return new RolPermisoRespuestaDto
        {
            RolId = relacion.RoId,
            PermisoId = relacion.PeId,
            FechaAsignacion = relacion.RpFechaAsig,
            Estatus = relacion.RpEstatus
        };
    }
}
