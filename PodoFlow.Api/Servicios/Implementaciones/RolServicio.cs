
using PodoFlow.Api.DTOs.Rol;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones;

public class RolServicio : IRolServicio
{
    private readonly IRolRepositorio _repositorio;

    public RolServicio(IRolRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<List<RolRespuestaDto>> ObtenerTodosAsync(
        bool incluirInactivos, int pagina, int tamano)
    {
        var roles = await _repositorio.ObtenerTodosAsync(
            incluirInactivos, pagina, tamano);

        return roles.Select(MapearRespuesta).ToList();
    }

    public async Task<RolRespuestaDto?> ObtenerPorIdAsync(int id)
    {
        var rol = await _repositorio.ObtenerPorIdAsync(id);

        return rol == null ? null : MapearRespuesta(rol);
    }

    public async Task<RolRespuestaDto> CrearAsync(RolCrearDto dto)
    {
        var codigo = NormalizarCodigo(dto.Codigo);
        var nombre = dto.Nombre.Trim();

        ValidarDatos(codigo, nombre);

        var existente = await _repositorio.ObtenerPorCodigoAsync(codigo);

        if (existente != null)
        {
            if (existente.RoEstatus)
            {
                throw new InvalidOperationException(
                    "Ya existe un rol activo con este código.");
            }

            existente.RoNombre = nombre;
            existente.RoDescri = dto.Descripcion?.Trim();
            existente.RoEstatus = true;

            await _repositorio.GuardarCambiosAsync();

            return MapearRespuesta(existente);
        }

        var rol = new Rol
        {
            RoCodigo = codigo,
            RoNombre = nombre,
            RoDescri = dto.Descripcion?.Trim(),
            RoFechaCrea = DateTime.UtcNow,
            RoEstatus = true
        };

        var creado = await _repositorio.CrearAsync(rol);

        return MapearRespuesta(creado);
    }

    public async Task<bool> ActualizarAsync(
        int id, RolActualizarDto dto)
    {
        var rol = await _repositorio.ObtenerPorIdAsync(id);

        if (rol == null)
            return false;

        var codigo = NormalizarCodigo(dto.Codigo);
        var nombre = dto.Nombre.Trim();

        ValidarDatos(codigo, nombre);

        var existente = await _repositorio.ObtenerPorCodigoAsync(codigo);

        if (existente != null && existente.RoId != id)
        {
            throw new InvalidOperationException(
                "El código ya pertenece a otro rol.");
        }

        rol.RoCodigo = codigo;
        rol.RoNombre = nombre;
        rol.RoDescri = dto.Descripcion?.Trim();

        await _repositorio.GuardarCambiosAsync();

        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var rol = await _repositorio.ObtenerPorIdAsync(id);

        if (rol == null)
            return false;

        if (!rol.RoEstatus)
            return true;

        rol.RoEstatus = false;

        await _repositorio.GuardarCambiosAsync();

        return true;
    }

    public async Task<bool> ReactivarAsync(int id)
    {
        var rol = await _repositorio.ObtenerPorIdAsync(id);

        if (rol == null)
            return false;

        if (rol.RoEstatus)
            return true;

        rol.RoEstatus = true;

        await _repositorio.GuardarCambiosAsync();

        return true;
    }

    private static string NormalizarCodigo(string codigo)
    {
        return codigo.Trim().ToUpperInvariant();
    }

    private static void ValidarDatos(string codigo, string nombre)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException(
                "El código del rol es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException(
                "El nombre del rol es obligatorio.");
        }
    }

    private static RolRespuestaDto MapearRespuesta(Rol rol)
    {
        return new RolRespuestaDto
        {
            Id = rol.RoId,
            Codigo = rol.RoCodigo,
            Nombre = rol.RoNombre,
            Descripcion = rol.RoDescri,
            FechaCreacion = rol.RoFechaCrea,
            Estatus = rol.RoEstatus
        };
    }
}
