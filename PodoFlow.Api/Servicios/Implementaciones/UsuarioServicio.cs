
using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.DTOs.Usuario;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones;

public class UsuarioServicio : IUsuarioServicio
{
    private readonly IUsuarioRepositorio _repositorio;

    public UsuarioServicio(IUsuarioRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<List<UsuarioRespuestaDto>> ObtenerTodosAsync(
        bool incluirInactivos, int pagina, int tamano)
    {
        var usuarios = await _repositorio.ObtenerTodosAsync(
            incluirInactivos, pagina, tamano);

        return usuarios.Select(MapearRespuesta).ToList();
    }

    public async Task<UsuarioRespuestaDto?> ObtenerPorIdAsync(long id)
    {
        var usuario = await _repositorio.ObtenerPorIdAsync(id);

        return usuario == null ? null : MapearRespuesta(usuario);
    }

    public async Task<UsuarioRespuestaDto> CrearAsync(
        UsuarioCrearDto dto)
    {
        var correo = dto.Correo.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(dto.Nombre))
            throw new ArgumentException("El nombre es obligatorio.");

        var existente = await _repositorio.ObtenerPorCorreoAsync(correo);

        if (existente != null)
        {
            if (existente.UsEstatus)
                throw new InvalidOperationException(
                    "Ya existe un usuario activo con este correo.");

            existente.UsNombre = dto.Nombre.Trim();
            existente.UsEstatus = true;
            existente.UsFechaModi = DateTime.UtcNow;

            await _repositorio.GuardarCambiosAsync();
            return MapearRespuesta(existente);
        }

        var usuario = new Usuario
        {
            UsNombre = dto.Nombre.Trim(),
            UsCorreo = correo,
            UsFechaCrea = DateTime.UtcNow,
            UsEstatus = true
        };

        var creado = await _repositorio.CrearAsync(usuario);
        return MapearRespuesta(creado);
    }

    public async Task<bool> ActualizarAsync(
        long id, UsuarioActualizarDto dto)
    {
        var usuario = await _repositorio.ObtenerPorIdAsync(id);
        if (usuario == null) return false;

        if (string.IsNullOrWhiteSpace(dto.Nombre))
            throw new ArgumentException("El nombre es obligatorio.");

        byte[] version;

        try
        {
            version = Convert.FromBase64String(dto.Version);
        }
        catch (FormatException)
        {
            throw new ArgumentException("La versión no es válida.");
        }

        if (!usuario.UsVersion.SequenceEqual(version))
            throw new DbUpdateConcurrencyException(
                "El usuario fue modificado por otro proceso.");

        var correo = dto.Correo.Trim().ToLowerInvariant();
        var existente = await _repositorio.ObtenerPorCorreoAsync(correo);

        if (existente != null && existente.UsId != id)
            throw new InvalidOperationException(
                "El correo ya pertenece a otro usuario.");

        usuario.UsNombre = dto.Nombre.Trim();
        usuario.UsCorreo = correo;
        usuario.UsFechaModi = DateTime.UtcNow;

        await _repositorio.GuardarCambiosAsync();
        return true;
    }

    public async Task<bool> EliminarAsync(long id)
    {
        var usuario = await _repositorio.ObtenerPorIdAsync(id);
        if (usuario == null) return false;

        if (!usuario.UsEstatus) return true;

        usuario.UsEstatus = false;
        usuario.UsFechaModi = DateTime.UtcNow;

        await _repositorio.GuardarCambiosAsync();
        return true;
    }

    public async Task<bool> ReactivarAsync(long id)
    {
        var usuario = await _repositorio.ObtenerPorIdAsync(id);
        if (usuario == null) return false;

        if (usuario.UsEstatus) return true;

        usuario.UsEstatus = true;
        usuario.UsFechaModi = DateTime.UtcNow;

        await _repositorio.GuardarCambiosAsync();
        return true;
    }

    private static UsuarioRespuestaDto MapearRespuesta(Usuario usuario)
    {
        return new UsuarioRespuestaDto
        {
            Id = usuario.UsId,
            Nombre = usuario.UsNombre,
            Correo = usuario.UsCorreo,
            FechaCreacion = usuario.UsFechaCrea,
            UltimoAcceso = usuario.UsUltimAcces,
            Estatus = usuario.UsEstatus,
            Version = Convert.ToBase64String(usuario.UsVersion)
        };
    }
}

