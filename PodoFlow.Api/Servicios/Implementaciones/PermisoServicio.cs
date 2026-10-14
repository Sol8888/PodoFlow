
using PodoFlow.Api.DTOs.Permiso;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones;

public class PermisoServicio : IPermisoServicio
{
    private readonly IPermisoRepositorio _repositorio;

    public PermisoServicio(IPermisoRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<List<PermisoRespuestaDto>> ObtenerTodosAsync(
        bool incluirInactivos, int pagina, int tamano)
    {
        var permisos = await _repositorio.ObtenerTodosAsync(
            incluirInactivos, pagina, tamano);

        return permisos.Select(MapearRespuesta).ToList();
    }

    public async Task<PermisoRespuestaDto?> ObtenerPorIdAsync(int id)
    {
        var permiso = await _repositorio.ObtenerPorIdAsync(id);

        return permiso == null
            ? null
            : MapearRespuesta(permiso);
    }

    public async Task<PermisoRespuestaDto> CrearAsync(
        PermisoCrearDto dto)
    {
        var codigo = Normalizar(dto.Codigo);
        var nombre = dto.Nombre.Trim();
        var modulo = Normalizar(dto.Modulo);

        ValidarDatos(codigo, nombre, modulo);

        var existente = await _repositorio.ObtenerPorCodigoAsync(codigo);

        if (existente != null)
        {
            if (existente.PeEstatus)
            {
                throw new InvalidOperationException(
                    "Ya existe un permiso activo con ese código.");
            }

            existente.PeNombre = nombre;
            existente.PeModulo = modulo;
            existente.PeDescri = dto.Descripcion?.Trim();
            existente.PeEstatus = true;

            await _repositorio.GuardarCambiosAsync();

            return MapearRespuesta(existente);
        }

        var permiso = new Permiso
        {
            PeCodigo = codigo,
            PeNombre = nombre,
            PeModulo = modulo,
            PeDescri = dto.Descripcion?.Trim(),
            PeEstatus = true
        };

        var creado = await _repositorio.CrearAsync(permiso);

        return MapearRespuesta(creado);
    }

    public async Task<bool> ActualizarAsync(
        int id, PermisoActualizarDto dto)
    {
        var permiso = await _repositorio.ObtenerPorIdAsync(id);

        if (permiso == null)
            return false;

        var codigo = Normalizar(dto.Codigo);
        var nombre = dto.Nombre.Trim();
        var modulo = Normalizar(dto.Modulo);

        ValidarDatos(codigo, nombre, modulo);

        var existente = await _repositorio.ObtenerPorCodigoAsync(codigo);

        if (existente != null && existente.PeId != id)
        {
            throw new InvalidOperationException(
                "El código ya pertenece a otro permiso.");
        }

        permiso.PeCodigo = codigo;
        permiso.PeNombre = nombre;
        permiso.PeModulo = modulo;
        permiso.PeDescri = dto.Descripcion?.Trim();

        await _repositorio.GuardarCambiosAsync();

        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var permiso = await _repositorio.ObtenerPorIdAsync(id);

        if (permiso == null)
            return false;

        if (!permiso.PeEstatus)
            return true;

        permiso.PeEstatus = false;

        await _repositorio.GuardarCambiosAsync();

        return true;
    }

    public async Task<bool> ReactivarAsync(int id)
    {
        var permiso = await _repositorio.ObtenerPorIdAsync(id);

        if (permiso == null)
            return false;

        if (permiso.PeEstatus)
            return true;

        permiso.PeEstatus = true;

        await _repositorio.GuardarCambiosAsync();

        return true;
    }

    private static string Normalizar(string valor)
    {
        return valor.Trim().ToUpperInvariant();
    }

    private static void ValidarDatos(
        string codigo, string nombre, string modulo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException(
                "El código del permiso es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException(
                "El nombre del permiso es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(modulo))
        {
            throw new ArgumentException(
                "El módulo del permiso es obligatorio.");
        }
    }

    private static PermisoRespuestaDto MapearRespuesta(Permiso permiso)
    {
        return new PermisoRespuestaDto
        {
            Id = permiso.PeId,
            Codigo = permiso.PeCodigo,
            Nombre = permiso.PeNombre,
            Modulo = permiso.PeModulo,
            Descripcion = permiso.PeDescri,
            Estatus = permiso.PeEstatus
        };
    }
}
