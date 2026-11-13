using PodoFlow.Api.DTOs.BloqueoAgenda;
using PodoFlow.Api.DTOs.CambioCita;
using PodoFlow.Api.DTOs.Cita;
using PodoFlow.Api.DTOs.HorarioAtencion;

namespace PodoFlow.Api.Tests.Servicios.Agenda;

public class CitaServicioPruebas : PruebaServicioBase
{
    public CitaServicioPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    private ICitaServicio Servicio => Servicio<ICitaServicio>();

    private async Task<CitaCrearDto> SolicitudAsync(DateOnly dia, int hora, long? usuarioId = null)
    {
        var paciente = await NuevoPacienteAsync();
        var inicio = HoraLocalEnUtc(dia, hora);

        return new CitaCrearDto
        {
            PacienteId = paciente.PaId,
            ServicioId = Semilla.Servicio,
            UsuarioId = usuarioId,
            FechaInicio = inicio,
            FechaFin = inicio.AddMinutes(30),
            Canal = " web ",
            Observaciones = " Primera visita "
        };
    }

    [Fact]
    public async Task Reserva_una_cita_disponible_y_deja_historial_y_auditoria()
    {
        await NuevoHorarioSemanalAsync();
        var dto = await SolicitudAsync(DiaFuturo(8), 10);

        var creada = await Servicio.CrearAsync(dto);

        Assert.Equal("SOLICITADA", creada.Estado);
        Assert.Equal("WEB", creada.Canal);
        Assert.Single(await Servicio<ICambioCitaServicio>().ObtenerTodosAsync(creada.Id, 1, 10));
        Assert.True(await Contexto.EventosAuditoria.AnyAsync(e => e.EvEntidId == creada.Id.ToString() && e.EvAccion == "CREAR"));
        Assert.Contains(await Servicio.ObtenerTodosAsync(dto.PacienteId, null, "solicitada", 1, 10), c => c.Id == creada.Id);
    }

    [Fact]
    public async Task No_reserva_un_horario_ocupado_bloqueado_o_fuera_de_atencion()
    {
        await NuevoHorarioSemanalAsync(horaInicio: 8, horaFin: 18);
        var dia = DiaFuturo(9);
        var primera = await SolicitudAsync(dia, 10);
        await Servicio.CrearAsync(primera);

        var choque = await SolicitudAsync(dia, 10);
        await Falla<InvalidOperationException>(() => Servicio.CrearAsync(choque));

        var temprano = await SolicitudAsync(dia, 6);
        await Falla<InvalidOperationException>(() => Servicio.CrearAsync(temprano));

        Contexto.BloqueosAgenda.Add(new PodoFlow.Api.Modelos.BloqueoAgenda
        {
            BlFechaIni = HoraLocalEnUtc(dia, 14),
            BlFechaFin = HoraLocalEnUtc(dia, 16),
            BlMotivo = "Capacitación",
            BlFechaCrea = DateTime.UtcNow,
            BlEstatus = true
        });
        await Contexto.SaveChangesAsync();

        var bloqueada = await SolicitudAsync(dia, 15);
        await Falla<InvalidOperationException>(() => Servicio.CrearAsync(bloqueada));
    }

    [Fact]
    public async Task Valida_fechas_utc_precision_y_relaciones()
    {
        await NuevoHorarioSemanalAsync();
        var dto = await SolicitudAsync(DiaFuturo(10), 11);

        var local = await SolicitudAsync(DiaFuturo(10), 11);
        local.FechaInicio = DateTime.SpecifyKind(local.FechaInicio, DateTimeKind.Local);
        await Falla<ArgumentException>(() => Servicio.CrearAsync(local));

        var segundos = await SolicitudAsync(DiaFuturo(10), 11);
        segundos.FechaInicio = segundos.FechaInicio.AddSeconds(15);
        await Falla<ArgumentException>(() => Servicio.CrearAsync(segundos));

        var invertida = await SolicitudAsync(DiaFuturo(10), 11);
        invertida.FechaFin = invertida.FechaInicio.AddMinutes(-30);
        await Falla<ArgumentException>(() => Servicio.CrearAsync(invertida));

        var pasada = await SolicitudAsync(DiaFuturo(-2), 11);
        await Falla<ArgumentException>(() => Servicio.CrearAsync(pasada));

        dto.PacienteId = Semilla.PacienteInactivo;
        await Falla<ArgumentException>(() => Servicio.CrearAsync(dto));

        var servicio = await SolicitudAsync(DiaFuturo(10), 11);
        servicio.ServicioId = int.MaxValue;
        await Falla<ArgumentException>(() => Servicio.CrearAsync(servicio));

        var profesional = await SolicitudAsync(DiaFuturo(10), 11, Semilla.UsuarioInactivo);
        await Falla<ArgumentException>(() => Servicio.CrearAsync(profesional));

        var canal = await SolicitudAsync(DiaFuturo(10), 11);
        canal.Canal = " ";
        await Falla<ArgumentException>(() => Servicio.CrearAsync(canal));
    }

    [Fact]
    public async Task Confirma_atiende_y_bloquea_cambios_en_citas_finalizadas()
    {
        await NuevoHorarioSemanalAsync();
        var creada = await Servicio.CrearAsync(await SolicitudAsync(DiaFuturo(11), 9));

        Assert.True(await Servicio.CambiarEstadoAsync(creada.Id, new CitaCambiarEstadoDto { NuevoEstado = "confirmada", Motivo = "Llamada", Version = creada.Version }));
        var confirmada = await Servicio.ObtenerPorIdAsync(creada.Id);
        Assert.True(await Servicio.CambiarEstadoAsync(creada.Id, new CitaCambiarEstadoDto { NuevoEstado = "ATENDIDA", Motivo = "Asistió", Version = confirmada!.Version }));
        var atendida = await Servicio.ObtenerPorIdAsync(creada.Id);

        Assert.Equal("ATENDIDA", atendida!.Estado);
        await Falla<InvalidOperationException>(() => Servicio.CambiarEstadoAsync(creada.Id, new CitaCambiarEstadoDto { NuevoEstado = "CANCELADA", Motivo = "x", Version = atendida.Version }));
        var actualizar = await SolicitudAsync(DiaFuturo(11), 9);
        await Falla<InvalidOperationException>(() => Servicio.ActualizarAsync(creada.Id, new CitaActualizarDto
        {
            PacienteId = atendida.PacienteId,
            ServicioId = Semilla.Servicio,
            FechaInicio = actualizar.FechaInicio,
            FechaFin = actualizar.FechaFin,
            Canal = "WEB",
            Version = atendida.Version
        }));
        Assert.Equal(3, (await Servicio<ICambioCitaServicio>().ObtenerTodosAsync(creada.Id, 1, 10)).Count);
    }

    [Fact]
    public async Task Valida_estado_motivo_y_version_al_cambiar_estado()
    {
        await NuevoHorarioSemanalAsync();
        var creada = await Servicio.CrearAsync(await SolicitudAsync(DiaFuturo(12), 9));

        await Falla<ArgumentException>(() => Servicio.CambiarEstadoAsync(creada.Id, new CitaCambiarEstadoDto { NuevoEstado = "PERDIDA", Motivo = "x", Version = creada.Version }));
        await Falla<ArgumentException>(() => Servicio.CambiarEstadoAsync(creada.Id, new CitaCambiarEstadoDto { NuevoEstado = "CANCELADA", Motivo = " ", Version = creada.Version }));
        await Falla<ArgumentException>(() => Servicio.CambiarEstadoAsync(creada.Id, new CitaCambiarEstadoDto { NuevoEstado = "CANCELADA", Motivo = "x", Version = "corta" }));
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => Servicio.CambiarEstadoAsync(creada.Id, new CitaCambiarEstadoDto { NuevoEstado = "CANCELADA", Motivo = "x", Version = Convert.ToBase64String(new byte[8]) }));
    }

    [Fact]
    public async Task Reprograma_con_motivo_a_un_horario_libre()
    {
        await NuevoHorarioSemanalAsync();
        var dia = DiaFuturo(13);
        var dto = await SolicitudAsync(dia, 9);
        var creada = await Servicio.CrearAsync(dto);
        var nuevoInicio = HoraLocalEnUtc(dia, 11);
        var cambio = new CitaActualizarDto
        {
            PacienteId = dto.PacienteId,
            ServicioId = dto.ServicioId,
            FechaInicio = nuevoInicio,
            FechaFin = nuevoInicio.AddMinutes(30),
            Canal = "WEB",
            Version = creada.Version
        };

        await Falla<ArgumentException>(() => Servicio.ActualizarAsync(creada.Id, cambio));
        OlvidarSeguimiento();
        cambio.MotivoCambio = "Pedido del paciente";

        Assert.True(await Servicio.ActualizarAsync(creada.Id, cambio));
        var reprogramada = await Servicio.ObtenerPorIdAsync(creada.Id);
        Assert.Equal("REPROGRAMADA", reprogramada!.Estado);

        var soloObservacion = new CitaActualizarDto
        {
            PacienteId = dto.PacienteId,
            ServicioId = dto.ServicioId,
            FechaInicio = nuevoInicio,
            FechaFin = nuevoInicio.AddMinutes(30),
            Canal = "WEB",
            Observaciones = "Trae exámenes",
            Version = reprogramada.Version
        };
        Assert.True(await Servicio.ActualizarAsync(creada.Id, soloObservacion));
    }

    [Fact]
    public async Task Calcula_los_huecos_disponibles_del_dia()
    {
        await NuevoHorarioSemanalAsync(horaInicio: 9, horaFin: 11);
        var dia = DiaFuturo(14);
        var ocupada = await SolicitudAsync(dia, 9);
        await Servicio.CrearAsync(ocupada);

        var huecos = await Servicio.ObtenerDisponibilidadAsync(Semilla.Servicio, dia, null);

        Assert.Equal(3, huecos.Count);
        Assert.DoesNotContain(huecos, h => h.FechaInicio == ocupada.FechaInicio);
        await Falla<ArgumentException>(() => Servicio.ObtenerDisponibilidadAsync(0, dia, null));
        await Falla<ArgumentException>(() => Servicio.ObtenerDisponibilidadAsync(int.MaxValue, dia, null));
        await Falla<ArgumentException>(() => Servicio.ObtenerDisponibilidadAsync(Semilla.Servicio, dia, Semilla.UsuarioInactivo));
        await Falla<ArgumentException>(() => Servicio.ObtenerDisponibilidadAsync(Semilla.Servicio, dia, -1));
    }

    [Fact]
    public async Task Valida_los_filtros_del_listado()
    {
        await Falla<ArgumentException>(() => Servicio.ObtenerTodosAsync(null, null, null, 0, 10));
        await Falla<ArgumentException>(() => Servicio.ObtenerTodosAsync(-1, null, null, 1, 10));
        await Falla<ArgumentException>(() => Servicio.ObtenerTodosAsync(null, -1, null, 1, 10));
        await Falla<ArgumentException>(() => Servicio.ObtenerTodosAsync(null, null, "RARA", 1, 10));
        Assert.Null(await Servicio.ObtenerPorIdAsync(Semilla.IdInexistente));
        Assert.False(await Servicio.CambiarEstadoAsync(Semilla.IdInexistente, new CitaCambiarEstadoDto { NuevoEstado = "CANCELADA", Motivo = "x", Version = Convert.ToBase64String(new byte[8]) }));
    }
}

public class CambioCitaServicioPruebas : PruebaServicioBase
{
    public CambioCitaServicioPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    [Fact]
    public async Task Registra_cambios_y_rechaza_los_vacios()
    {
        var servicio = Servicio<ICambioCitaServicio>();

        var cambio = await servicio.CrearAsync(new CambioCitaCrearDto { CitaId = Semilla.CitaFutura, UsuarioId = Semilla.UsuarioAdmin, EstadoAnterior = "solicitada", EstadoNuevo = "confirmada", Motivo = " ok " });

        Assert.Equal("CONFIRMADA", cambio.EstadoNuevo);
        Assert.NotNull(await servicio.ObtenerPorIdAsync(cambio.Id));
        Assert.Null(await servicio.ObtenerPorIdAsync(Semilla.IdInexistente));
        await Falla<ArgumentException>(() => servicio.CrearAsync(new CambioCitaCrearDto { CitaId = Semilla.CitaFutura }));
        await Falla<ArgumentException>(() => servicio.CrearAsync(new CambioCitaCrearDto { CitaId = Semilla.CitaFutura, EstadoAnterior = "CONFIRMADA", EstadoNuevo = "CONFIRMADA" }));
        await Falla<ArgumentException>(() => servicio.CrearAsync(new CambioCitaCrearDto { CitaId = 0, EstadoNuevo = "CONFIRMADA" }));
        await Falla<ArgumentException>(() => servicio.CrearAsync(new CambioCitaCrearDto { CitaId = Semilla.CitaFutura, EstadoNuevo = "PERDIDA" }));
        await Falla<ArgumentException>(() => servicio.CrearAsync(new CambioCitaCrearDto { CitaId = Semilla.CitaFutura, EstadoNuevo = "CANCELADA", UsuarioId = Semilla.IdInexistente }));
        await Falla<ArgumentException>(() => servicio.ObtenerTodosAsync(null, 0, 10));
        await Falla<ArgumentException>(() => servicio.ObtenerTodosAsync(-5, 1, 10));
    }
}

public class HorarioAtencionServicioPruebas : PruebaServicioBase
{
    public HorarioAtencionServicioPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    private IHorarioAtencionServicio Servicio => Servicio<IHorarioAtencionServicio>();

    private static HorarioAtencionCrearDto Lunes(long usuarioId, int desde, int hasta)
        => new() { UsuarioId = usuarioId, DiaSemana = 1, HoraInicio = TimeSpan.FromHours(desde), HoraFin = TimeSpan.FromHours(hasta) };

    [Fact]
    public async Task Crea_horarios_sin_superposiciones()
    {
        var usuario = await NuevoUsuarioAsync();

        var manana = await Servicio.CrearAsync(Lunes(usuario.UsId, 8, 12));
        var tarde = await Servicio.CrearAsync(Lunes(usuario.UsId, 14, 18));

        Assert.True(manana.Estatus);
        await Falla<InvalidOperationException>(() => Servicio.CrearAsync(Lunes(usuario.UsId, 11, 15)));
        await Falla<InvalidOperationException>(() => Servicio.ActualizarAsync(tarde.Id, new HorarioAtencionActualizarDto { UsuarioId = usuario.UsId, DiaSemana = 1, HoraInicio = TimeSpan.FromHours(10), HoraFin = TimeSpan.FromHours(15) }));
        Assert.True(await Servicio.ActualizarAsync(tarde.Id, new HorarioAtencionActualizarDto { UsuarioId = usuario.UsId, DiaSemana = 1, HoraInicio = TimeSpan.FromHours(13), HoraFin = TimeSpan.FromHours(17) }));
        Assert.NotNull(await Servicio.ObtenerPorIdAsync(manana.Id));
        Assert.NotEmpty(await Servicio.ObtenerTodosAsync(false, 1, 100));
    }

    [Fact]
    public async Task Rechaza_dias_horas_fechas_y_usuarios_invalidos()
    {
        var usuario = await NuevoUsuarioAsync();

        var dia = Lunes(usuario.UsId, 8, 9);
        dia.DiaSemana = 8;
        await Falla<ArgumentException>(() => Servicio.CrearAsync(dia));
        await Falla<ArgumentException>(() => Servicio.CrearAsync(Lunes(usuario.UsId, 12, 9)));
        await Falla<ArgumentException>(() => Servicio.CrearAsync(new HorarioAtencionCrearDto { UsuarioId = usuario.UsId, DiaSemana = 2, HoraInicio = null, HoraFin = TimeSpan.FromHours(9) }));

        var fechas = Lunes(usuario.UsId, 8, 9);
        fechas.FechaDesde = new DateTime(2027, 5, 1);
        fechas.FechaHasta = new DateTime(2027, 4, 1);
        await Falla<ArgumentException>(() => Servicio.CrearAsync(fechas));

        await Falla<ArgumentException>(() => Servicio.CrearAsync(Lunes(Semilla.UsuarioInactivo, 8, 9)));
    }

    [Fact]
    public async Task Da_de_baja_y_reactiva_si_no_hay_choque()
    {
        var usuario = await NuevoUsuarioAsync();
        var original = await Servicio.CrearAsync(Lunes(usuario.UsId, 8, 12));

        Assert.True(await Servicio.EliminarAsync(original.Id));
        Assert.True(await Servicio.EliminarAsync(original.Id));
        await Falla<InvalidOperationException>(() => Servicio.ActualizarAsync(original.Id, new HorarioAtencionActualizarDto { UsuarioId = usuario.UsId, DiaSemana = 1, HoraInicio = TimeSpan.FromHours(8), HoraFin = TimeSpan.FromHours(9) }));

        var reemplazo = await Servicio.CrearAsync(Lunes(usuario.UsId, 9, 10));
        await Falla<InvalidOperationException>(() => Servicio.ReactivarAsync(original.Id));
        await Servicio.EliminarAsync(reemplazo.Id);
        Assert.True(await Servicio.ReactivarAsync(original.Id));
        Assert.True(await Servicio.ReactivarAsync(original.Id));

        Assert.Null(await Servicio.ObtenerPorIdAsync(int.MaxValue));
        Assert.False(await Servicio.EliminarAsync(int.MaxValue));
        Assert.False(await Servicio.ReactivarAsync(int.MaxValue));
        Assert.False(await Servicio.ActualizarAsync(int.MaxValue, new HorarioAtencionActualizarDto { DiaSemana = 1, HoraInicio = TimeSpan.FromHours(8), HoraFin = TimeSpan.FromHours(9) }));
    }
}

public class BloqueoAgendaServicioPruebas : PruebaServicioBase
{
    public BloqueoAgendaServicioPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    private IBloqueoAgendaServicio Servicio => Servicio<IBloqueoAgendaServicio>();

    private static BloqueoAgendaCrearDto Bloqueo(long usuarioId, int diasDesdeHoy, string motivo = "Vacaciones")
    {
        var inicio = DateTime.UtcNow.Date.AddDays(diasDesdeHoy);
        return new BloqueoAgendaCrearDto { UsuarioId = usuarioId, FechaInicio = inicio, FechaFin = inicio.AddHours(4), Motivo = motivo };
    }

    [Fact]
    public async Task Crea_bloqueos_sin_superposicion_y_los_audita()
    {
        var usuario = await NuevoUsuarioAsync();

        var creado = await Servicio.CrearAsync(Bloqueo(usuario.UsId, 20));

        Assert.True(creado.Estatus);
        await Falla<InvalidOperationException>(() => Servicio.CrearAsync(Bloqueo(usuario.UsId, 20)));
        await Falla<InvalidOperationException>(() => Servicio.CrearAsync(Bloqueo(usuario.UsId, 20, "Otro motivo")));
        Assert.True(await Contexto.EventosAuditoria.AnyAsync(e => e.EvEntid == "BLOQUEO_AGENDA" && e.EvEntidId == creado.Id.ToString()));
        Assert.Single(await Servicio.ObtenerTodosAsync(false, usuario.UsId, 1, 10));
    }

    [Fact]
    public async Task Valida_fechas_motivo_y_usuario()
    {
        var usuario = await NuevoUsuarioAsync();

        var invertido = Bloqueo(usuario.UsId, 21);
        invertido.FechaFin = invertido.FechaInicio.AddHours(-1);
        await Falla<ArgumentException>(() => Servicio.CrearAsync(invertido));
        await Falla<ArgumentException>(() => Servicio.CrearAsync(Bloqueo(usuario.UsId, 21, " ")));
        await Falla<ArgumentException>(() => Servicio.CrearAsync(Bloqueo(Semilla.UsuarioInactivo, 21)));
    }

    [Fact]
    public async Task Actualiza_da_de_baja_reactiva_y_reutiliza_el_bloqueo_identico()
    {
        var usuario = await NuevoUsuarioAsync();
        var dto = Bloqueo(usuario.UsId, 22);
        var creado = await Servicio.CrearAsync(dto);

        Assert.True(await Servicio.ActualizarAsync(creado.Id, new BloqueoAgendaActualizarDto { UsuarioId = usuario.UsId, FechaInicio = dto.FechaInicio, FechaFin = dto.FechaFin.AddHours(1), Motivo = "Vacaciones extendidas" }));
        Assert.True(await Servicio.EliminarAsync(creado.Id));
        Assert.True(await Servicio.EliminarAsync(creado.Id));
        await Falla<InvalidOperationException>(() => Servicio.ActualizarAsync(creado.Id, new BloqueoAgendaActualizarDto { UsuarioId = usuario.UsId, FechaInicio = dto.FechaInicio, FechaFin = dto.FechaFin, Motivo = "x" }));
        Assert.True(await Servicio.ReactivarAsync(creado.Id));
        Assert.True(await Servicio.ReactivarAsync(creado.Id));
        await Servicio.EliminarAsync(creado.Id);

        var reutilizado = await Servicio.CrearAsync(new BloqueoAgendaCrearDto { UsuarioId = usuario.UsId, FechaInicio = dto.FechaInicio, FechaFin = dto.FechaFin.AddHours(1), Motivo = "Vacaciones extendidas" });
        Assert.Equal(creado.Id, reutilizado.Id);
        Assert.NotNull(await Servicio.ObtenerPorIdAsync(creado.Id));
        Assert.Null(await Servicio.ObtenerPorIdAsync(Semilla.IdInexistente));
        Assert.False(await Servicio.EliminarAsync(Semilla.IdInexistente));
        Assert.False(await Servicio.ReactivarAsync(Semilla.IdInexistente));
        Assert.False(await Servicio.ActualizarAsync(Semilla.IdInexistente, dto.ToActualizar()));
    }
}

internal static class ExtensionesBloqueo
{
    public static BloqueoAgendaActualizarDto ToActualizar(this BloqueoAgendaCrearDto dto)
        => new() { UsuarioId = dto.UsuarioId, FechaInicio = dto.FechaInicio, FechaFin = dto.FechaFin, Motivo = dto.Motivo };
}
