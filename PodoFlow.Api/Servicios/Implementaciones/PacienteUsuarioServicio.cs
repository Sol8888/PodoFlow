
using PodoFlow.Api.DTOs.PacienteUsuario;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones;

public class PacienteUsuarioServicio : IPacienteUsuarioServicio
{
    private readonly IPacienteUsuarioRepositorio _repo;

    public PacienteUsuarioServicio(IPacienteUsuarioRepositorio repo)
    {
        _repo = repo;
    }

    public async Task<List<PacienteUsuarioRespuestaDto>> ListarAsync(
        bool incluirInactivos, int pagina, int tamano)
    {
        var datos = await _repo.ListarAsync(
            incluirInactivos, pagina, tamano);
        return datos.Select(Mapear).ToList();
    }

    public async Task<PacienteUsuarioRespuestaDto?> ObtenerAsync(
        long pacienteId, long usuarioId)
    {
        var relacion = await _repo.ObtenerAsync(pacienteId, usuarioId);
        return relacion == null ? null : Mapear(relacion);
    }

    public async Task<PacienteUsuarioRespuestaDto> CrearAsync(
        PacienteUsuarioCrearDto dto)
    {
        await ValidarActivos(dto.PacienteId, dto.UsuarioId);

        var existente = await _repo.ObtenerAsync(
            dto.PacienteId, dto.UsuarioId);

        if (existente != null)
        {
            if (existente.PuEstatus)
                throw new InvalidOperationException(
                    "La relación ya está activa.");

            existente.PuEstatus = true;
            await _repo.GuardarAsync();
            return Mapear(existente);
        }

        // Las restricciones UNIQUE no permiten otra pareja,
        // aunque su registro anterior esté inactivo.
        if (await _repo.PorPacienteAsync(dto.PacienteId) != null ||
            await _repo.PorUsuarioAsync(dto.UsuarioId) != null)
            throw new InvalidOperationException(
                "El paciente o usuario ya está vinculado. " +
                "La base de datos no permite reemplazar esa pareja.");

        var relacion = new PacienteUsuario
        {
            PaId = dto.PacienteId,
            UsId = dto.UsuarioId,
            PuFechaAsig = DateTime.UtcNow,
            PuEstatus = true
        };

        await _repo.CrearAsync(relacion);
        return Mapear(relacion);
    }

    public async Task<bool> ActualizarAsync(
        long pacienteId, long usuarioId,
        PacienteUsuarioActualizarDto dto)
    {
        var existente = await _repo.ObtenerAsync(
            pacienteId, usuarioId);

        if (existente == null) return false;

        if (dto.NuevoUsuarioId == usuarioId)
            return true;

        throw new InvalidOperationException(
            "No se puede cambiar us_id: la base usa una clave " +
            "compuesta y UNIQUE por paciente y usuario. " +
            "Para permitir reasignaciones debe diseñarse " +
            "un procedimiento específico con historial.");
    }

    public async Task<bool> EliminarAsync(
        long pacienteId, long usuarioId)
    {
        var relacion = await _repo.ObtenerAsync(
            pacienteId, usuarioId);

        if (relacion == null) return false;
        if (!relacion.PuEstatus) return true;

        relacion.PuEstatus = false;
        await _repo.GuardarAsync();
        return true;
    }

    public async Task<bool> ReactivarAsync(
        long pacienteId, long usuarioId)
    {
        var relacion = await _repo.ObtenerAsync(
            pacienteId, usuarioId);

        if (relacion == null) return false;

        await ValidarActivos(pacienteId, usuarioId);

        if (relacion.PuEstatus) return true;

        relacion.PuEstatus = true;
        await _repo.GuardarAsync();
        return true;
    }

    private async Task ValidarActivos(long pacienteId, long usuarioId)
    {
        if (!await _repo.PacienteActivoAsync(pacienteId))
            throw new ArgumentException(
                "El paciente no existe o está inactivo.");

        if (!await _repo.UsuarioActivoAsync(usuarioId))
            throw new ArgumentException(
                "El usuario no existe o está inactivo.");
    }

    private static PacienteUsuarioRespuestaDto Mapear(
        PacienteUsuario x) => new()
        {
            PacienteId = x.PaId,
            UsuarioId = x.UsId,
            FechaAsignacion = x.PuFechaAsig,
            Estatus = x.PuEstatus
        };
}
