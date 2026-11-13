using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Tests.Infraestructura;

[Collection(ColeccionApi.Nombre)]
public abstract class PruebaServicioBase : IAsyncLifetime
{
    private static readonly TimeZoneInfo ZonaEcuador =
        TimeZoneInfo.TryFindSystemTimeZoneById("America/Guayaquil", out var zona)
            ? zona
            : TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");

    private AsyncServiceScope _alcance;
    private IDbContextTransaction? _transaccion;

    protected PruebaServicioBase(FabricaApi fabrica)
    {
        Fabrica = fabrica;
    }

    protected FabricaApi Fabrica { get; }

    protected PodoFlowContext Contexto { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Fabrica.Correo.Reiniciar();
        Fabrica.WhatsApp.Reiniciar();

        _alcance = Fabrica.Services.CreateAsyncScope();
        Contexto = _alcance.ServiceProvider.GetRequiredService<PodoFlowContext>();

        var baseDatos = Contexto.Database.GetDbConnection().Database;

        if (!string.Equals(baseDatos, ConfiguracionPruebas.BaseDatos, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Conexión inesperada a la base {baseDatos}.");

        _transaccion = await Contexto.Database.BeginTransactionAsync();
    }

    public async Task DisposeAsync()
    {
        try
        {
            if (_transaccion != null)
            {
                await _transaccion.RollbackAsync();
                await _transaccion.DisposeAsync();
            }
        }
        finally
        {
            await _alcance.DisposeAsync();
        }
    }

    protected T Servicio<T>() where T : notnull
        => _alcance.ServiceProvider.GetRequiredService<T>();

    protected void OlvidarSeguimiento()
        => Contexto.ChangeTracker.Clear();

    protected static DateTime Utc(DateTime valor)
        => DateTime.SpecifyKind(valor, DateTimeKind.Utc);

    protected static DateTime AlMinuto(DateTime valor)
        => Utc(new DateTime(valor.Year, valor.Month, valor.Day, valor.Hour, valor.Minute, 0));

    protected static DateTime HoraLocalEnUtc(DateOnly fecha, int hora, int minuto = 0)
        => Utc(TimeZoneInfo.ConvertTimeToUtc(fecha.ToDateTime(new TimeOnly(hora, minuto)), ZonaEcuador));

    protected static DateOnly DiaFuturo(int dias)
        => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ZonaEcuador)).AddDays(dias);

    protected async Task<int> FinalidadIdAsync(string codigo)
        => await Contexto.Finalidades.Where(f => f.FiCodigo == codigo).Select(f => f.FiId).SingleAsync();

    protected async Task<Usuario> NuevoUsuarioAsync(bool activo = true)
    {
        var usuario = new Usuario
        {
            UsNombre = "Usuario " + Identificaciones.Sufijo(),
            UsCorreo = $"u{Identificaciones.Sufijo().ToLowerInvariant()}@pruebas.podoflow.test",
            UsFechaCrea = DateTime.UtcNow,
            UsEstatus = activo
        };

        Contexto.Usuarios.Add(usuario);
        await Contexto.SaveChangesAsync();

        return usuario;
    }

    protected async Task<Paciente> NuevoPacienteAsync(DateTime? nacimiento = null, bool activo = true)
    {
        var paciente = new Paciente
        {
            PaTipoIdent = "CEDULA",
            PaIdent = Identificaciones.CedulaValida(),
            PaNombres = "Paciente",
            PaApelli = "Prueba " + Identificaciones.Sufijo(),
            PaFechaNaci = nacimiento ?? new DateTime(1988, 4, 12),
            PaFechaCrea = DateTime.UtcNow,
            PaEstatus = activo
        };

        Contexto.Pacientes.Add(paciente);
        await Contexto.SaveChangesAsync();

        return paciente;
    }

    protected async Task<DatoContacto> NuevoContactoAsync(long pacienteId, string tipo, string valor)
    {
        var contacto = new DatoContacto
        {
            PaId = pacienteId,
            DaTipo = tipo,
            DaValor = valor,
            DaPrinc = true,
            DaVerif = true,
            DaFechaCrea = DateTime.UtcNow,
            DaEstatus = true
        };

        Contexto.DatosContacto.Add(contacto);
        await Contexto.SaveChangesAsync();

        return contacto;
    }

    protected async Task<Consentimiento> NuevoConsentimientoAsync(long pacienteId, string codigoFinalidad, string estado = "ACEPTADO")
    {
        var consentimiento = new Consentimiento
        {
            PaId = pacienteId,
            FiId = await FinalidadIdAsync(codigoFinalidad),
            AvId = Semilla.Aviso,
            CoEstad = estado,
            CoMedio = "PRESENCIAL",
            CoFechaObte = DateTime.UtcNow,
            CoFechaRevo = estado == "REVOCADO" ? DateTime.UtcNow : null,
            CoObser = "Autorizado por el titular"
        };

        Contexto.Consentimientos.Add(consentimiento);
        await Contexto.SaveChangesAsync();

        return consentimiento;
    }

    protected async Task<OpcionComunicacion> NuevaOpcionAsync(
        long pacienteId, string codigoFinalidad, string canal, bool permitido, long? consentimientoId = null)
    {
        var opcion = new OpcionComunicacion
        {
            PaId = pacienteId,
            FiId = await FinalidadIdAsync(codigoFinalidad),
            CoId = consentimientoId,
            OpCanal = canal,
            OpPermi = permitido,
            OpFechaActu = DateTime.UtcNow,
            OpEstatus = true
        };

        Contexto.OpcionesComunicacion.Add(opcion);
        await Contexto.SaveChangesAsync();

        return opcion;
    }

    protected async Task<Plantilla> NuevaPlantillaAsync(string canal, string tipo, string? contenido = null, DateTime? creada = null)
    {
        var plantilla = new Plantilla
        {
            PlNombre = "plantilla_" + Identificaciones.Sufijo().ToLowerInvariant(),
            PlCanal = canal,
            PlTipo = tipo,
            PlAsunto = canal == "CORREO" ? "Asunto {{paciente}}" : null,
            PlConte = contenido ?? "Hola {{1}}, su cita es el {{2}} a las {{3}}.",
            PlVersi = "1.0",
            PlFechaCrea = creada ?? DateTime.UtcNow,
            PlEstatus = true
        };

        Contexto.Plantillas.Add(plantilla);
        await Contexto.SaveChangesAsync();

        return plantilla;
    }

    protected async Task<Servicio> NuevoServicioAsync(short duracion = 30, decimal precio = 20m)
    {
        var servicio = new Servicio
        {
            SeNombre = "Servicio " + Identificaciones.Sufijo(),
            SeDescri = "Servicio de pruebas",
            SePrecio = precio,
            SeDuracMin = duracion,
            SeFechaCrea = DateTime.UtcNow,
            SeEstatus = true
        };

        Contexto.Servicios.Add(servicio);
        await Contexto.SaveChangesAsync();

        return servicio;
    }

    protected async Task<Cita> NuevaCitaAsync(
        long pacienteId, DateTime inicioUtc, string estado = "CONFIRMADA", long? usuarioId = null, int? servicioId = null, int minutos = 30)
    {
        var cita = new Cita
        {
            PaId = pacienteId,
            SeId = servicioId ?? Semilla.Servicio,
            UsId = usuarioId,
            CiFechaIni = inicioUtc,
            CiFechaFin = inicioUtc.AddMinutes(minutos),
            CiEstad = estado,
            CiCanal = "SISTEMA",
            CiFechaCrea = DateTime.UtcNow
        };

        Contexto.Citas.Add(cita);
        await Contexto.SaveChangesAsync();

        return cita;
    }

    protected async Task NuevoHorarioSemanalAsync(long? usuarioId = null, int horaInicio = 6, int horaFin = 22)
    {
        for (byte dia = 1; dia <= 7; dia++)
        {
            Contexto.HorariosAtencion.Add(new HorarioAtencion
            {
                UsId = usuarioId,
                HoDiaSem = dia,
                HoHoraIni = TimeSpan.FromHours(horaInicio),
                HoHoraFin = TimeSpan.FromHours(horaFin),
                HoEstatus = true
            });
        }

        await Contexto.SaveChangesAsync();
    }

    protected async Task<Historia> NuevaHistoriaAsync(long pacienteId, string estado = "ACTIVA")
    {
        var historia = new Historia
        {
            PaId = pacienteId,
            UsId = Semilla.UsuarioAdmin,
            HiFechaAper = DateTime.UtcNow,
            HiEstad = estado
        };

        Contexto.Historias.Add(historia);
        await Contexto.SaveChangesAsync();

        return historia;
    }

    protected async Task<Atencion> NuevaAtencionAsync(long historiaId, long? citaId = null, string estado = "ABIERTA")
    {
        var atencion = new Atencion
        {
            HiId = historiaId,
            CiId = citaId,
            UsId = Semilla.UsuarioAdmin,
            AtFecha = DateTime.UtcNow,
            AtMotivCons = "Control de rutina",
            AtEstad = estado
        };

        Contexto.Atenciones.Add(atencion);
        await Contexto.SaveChangesAsync();

        return atencion;
    }

    protected async Task<Insumo> NuevoInsumoAsync(decimal stock = 0m)
    {
        var insumo = new Insumo
        {
            InNombre = "Insumo " + Identificaciones.Sufijo(),
            InCateg = "Curaciones",
            InUnidad = "unidad",
            InStockActu = stock,
            InStockMini = 1m,
            InFechaCrea = DateTime.UtcNow,
            InEstatus = true
        };

        Contexto.Insumos.Add(insumo);
        await Contexto.SaveChangesAsync();

        return insumo;
    }

    protected async Task<TratamientoDatos> NuevoTratamientoAsync()
    {
        var tratamiento = new TratamientoDatos
        {
            TrCodigo = "TR_" + Identificaciones.Sufijo(),
            FiId = await FinalidadIdAsync(Semilla.FinalidadAtencion),
            TrNombre = "Tratamiento de pruebas",
            TrModulo = "CLINICA",
            TrEstatus = true
        };

        Contexto.TratamientosDatos.Add(tratamiento);
        await Contexto.SaveChangesAsync();

        return tratamiento;
    }

    protected static async Task<TException> Falla<TException>(Func<Task> accion) where TException : Exception
        => await Assert.ThrowsAsync<TException>(accion);
}
