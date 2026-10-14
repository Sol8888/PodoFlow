
using PodoFlow.Api.DTOs.DatoContacto;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones;

public class DatoContactoServicio : IDatoContactoServicio
{
    private readonly IDatoContactoRepositorio _repo;

    private static readonly string[] TiposValidos =
    {
        "CELULAR", "TELEFONO", "CORREO", "WHATSAPP", "OTRO"
    };

    public DatoContactoServicio(IDatoContactoRepositorio repo)
    {
        _repo = repo;
    }

    public async Task<List<DatoContactoRespuestaDto>> ListarAsync(
        long? pacienteId, bool incluirInactivos,
        int pagina, int tamano)
    {
        var datos = await _repo.ListarAsync(
            pacienteId, incluirInactivos, pagina, tamano);

        return datos.Select(Mapear).ToList();
    }

    public async Task<DatoContactoRespuestaDto?> ObtenerAsync(long id)
    {
        var dato = await _repo.ObtenerAsync(id);
        return dato == null ? null : Mapear(dato);
    }

    public async Task<DatoContactoRespuestaDto> CrearAsync(
        DatoContactoCrearDto dto)
    {
        Validar(dto.Tipo, dto.Valor);

        if (!await _repo.PacienteActivoAsync(dto.PacienteId))
            throw new ArgumentException(
                "El paciente no existe o está inactivo.");

        var tipo = dto.Tipo.Trim().ToUpperInvariant();
        var valor = dto.Valor.Trim();

        var existente = await _repo.BuscarAsync(
            dto.PacienteId, tipo, valor);

        if (existente != null)
        {
            if (existente.DaEstatus)
                throw new InvalidOperationException(
                    "El paciente ya tiene ese contacto activo.");

            existente.DaEstatus = true;
            existente.DaPrinc = dto.Principal;

            await _repo.GuardarAsync();
            return Mapear(existente);
        }

        var contacto = new DatoContacto
        {
            PaId = dto.PacienteId,
            DaTipo = tipo,
            DaValor = valor,
            DaPrinc = dto.Principal,
            DaVerif = false,
            DaFechaCrea = DateTime.UtcNow,
            DaEstatus = true
        };

        return Mapear(await _repo.CrearAsync(contacto));
    }

    public async Task<bool> ActualizarAsync(
        long id, DatoContactoActualizarDto dto)
    {
        Validar(dto.Tipo, dto.Valor);

        var contacto = await _repo.ObtenerAsync(id);
        if (contacto == null) return false;

        if (!contacto.DaEstatus)
            throw new InvalidOperationException(
                "No se puede editar un contacto inactivo.");

        if (!await _repo.PacienteActivoAsync(contacto.PaId))
            throw new ArgumentException(
                "El paciente está inactivo.");

        var tipo = dto.Tipo.Trim().ToUpperInvariant();
        var valor = dto.Valor.Trim();

        var duplicado = await _repo.BuscarAsync(
            contacto.PaId, tipo, valor);

        if (duplicado != null && duplicado.DaId != id)
            throw new InvalidOperationException(
                "El paciente ya tiene ese contacto registrado.");

        bool cambioDato = contacto.DaTipo != tipo ||
                          contacto.DaValor != valor;

        contacto.DaTipo = tipo;
        contacto.DaValor = valor;
        contacto.DaPrinc = dto.Principal;

        // Si cambia el dato, pierde su verificación.
        if (cambioDato)
            contacto.DaVerif = false;

        await _repo.GuardarAsync();
        return true;
    }

    public async Task<bool> EliminarAsync(long id)
    {
        var contacto = await _repo.ObtenerAsync(id);
        if (contacto == null) return false;
        if (!contacto.DaEstatus) return true;

        contacto.DaEstatus = false;
        contacto.DaPrinc = false;

        await _repo.GuardarAsync();
        return true;
    }

    public async Task<bool> ReactivarAsync(long id)
    {
        var contacto = await _repo.ObtenerAsync(id);
        if (contacto == null) return false;

        if (!await _repo.PacienteActivoAsync(contacto.PaId))
            throw new ArgumentException(
                "No se puede reactivar un contacto de un paciente inactivo.");

        if (contacto.DaEstatus) return true;

        contacto.DaEstatus = true;
        contacto.DaPrinc = false;

        await _repo.GuardarAsync();
        return true;
    }

    private static void Validar(string tipo, string valor)
    {
        if (string.IsNullOrWhiteSpace(tipo) ||
            !TiposValidos.Contains(tipo.Trim().ToUpperInvariant()))
            throw new ArgumentException(
                "Tipo inválido. Use CELULAR, TELEFONO, " +
                "CORREO, WHATSAPP u OTRO.");

        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException(
                "El valor del contacto es obligatorio.");
    }

    private static DatoContactoRespuestaDto Mapear(DatoContacto c)
    {
        return new DatoContactoRespuestaDto
        {
            Id = c.DaId,
            PacienteId = c.PaId,
            Tipo = c.DaTipo,
            Valor = c.DaValor,
            Principal = c.DaPrinc,
            Verificado = c.DaVerif,
            FechaCreacion = c.DaFechaCrea,
            Estatus = c.DaEstatus
        };
    }
}
