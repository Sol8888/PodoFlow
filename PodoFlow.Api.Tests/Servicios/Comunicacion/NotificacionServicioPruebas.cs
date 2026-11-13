using PodoFlow.Api.DTOs.Notificacion;
using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Tests.Servicios.Comunicacion;

public class NotificacionServicioPruebas : PruebaServicioBase
{
    public NotificacionServicioPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    private INotificacionServicio Servicio => Servicio<INotificacionServicio>();

    private async Task<(Paciente Paciente, OpcionComunicacion Correo, OpcionComunicacion WhatsApp)> PacienteConRecordatoriosAsync(
        bool permitirCorreo = true, bool permitirWhatsApp = true, string consentimiento = "ACEPTADO")
    {
        var paciente = await NuevoPacienteAsync();
        var aceptado = await NuevoConsentimientoAsync(paciente.PaId, Semilla.FinalidadRecordatorios, consentimiento);
        var correo = await NuevaOpcionAsync(paciente.PaId, Semilla.FinalidadRecordatorios, "CORREO", permitirCorreo, aceptado.CoId);
        var whatsApp = await NuevaOpcionAsync(paciente.PaId, Semilla.FinalidadRecordatorios, "WHATSAPP", permitirWhatsApp, aceptado.CoId);

        return (paciente, correo, whatsApp);
    }

    private static DateTime EnHoras(double horas) => DateTime.UtcNow.AddHours(horas);

    [Fact]
    public async Task Programa_un_recordatorio_por_canal_para_citas_dentro_de_la_ventana()
    {
        var (paciente, correo, whatsApp) = await PacienteConRecordatoriosAsync();
        var cita = await NuevaCitaAsync(paciente.PaId, EnHoras(5));

        var (programadas, omitidas) = await Servicio.ProgramarRecordatoriosAsync(24);

        Assert.Equal(2, programadas);
        Assert.Equal(0, omitidas);
        var creadas = await Contexto.Notificaciones.Where(n => n.CiId == cita.CiId).ToListAsync();
        Assert.Equal(2, creadas.Count);
        Assert.Contains(creadas, n => n.OpId == correo.OpId && n.NoCanal == "CORREO");
        Assert.Contains(creadas, n => n.OpId == whatsApp.OpId && n.NoCanal == "WHATSAPP");
        Assert.All(creadas, n =>
        {
            Assert.Equal("PENDIENTE", n.NoEstad);
            Assert.Equal("RECORDATORIO_CITA", n.NoTipo);
            Assert.True(n.NoFechaProg < cita.CiFechaIni);
        });
    }

    [Fact]
    public async Task No_duplica_recordatorios_si_se_vuelve_a_programar()
    {
        var (paciente, _, _) = await PacienteConRecordatoriosAsync();
        var cita = await NuevaCitaAsync(paciente.PaId, EnHoras(6));

        await Servicio.ProgramarRecordatoriosAsync(24);
        var (programadas, _) = await Servicio.ProgramarRecordatoriosAsync(24);

        Assert.Equal(0, programadas);
        Assert.Equal(2, await Contexto.Notificaciones.CountAsync(n => n.CiId == cita.CiId));
    }

    [Fact]
    public async Task Ignora_citas_fuera_de_la_ventana_de_anticipacion()
    {
        var (paciente, _, _) = await PacienteConRecordatoriosAsync();
        var muyPronto = await NuevaCitaAsync(paciente.PaId, EnHoras(0.5));
        var muyLejos = await NuevaCitaAsync(paciente.PaId, EnHoras(30));

        await Servicio.ProgramarRecordatoriosAsync(24);

        Assert.False(await Contexto.Notificaciones.AnyAsync(n => n.CiId == muyPronto.CiId || n.CiId == muyLejos.CiId));
    }

    [Fact]
    public async Task Respeta_el_canal_sin_permiso_del_paciente()
    {
        var (paciente, correo, _) = await PacienteConRecordatoriosAsync(permitirWhatsApp: false);
        var cita = await NuevaCitaAsync(paciente.PaId, EnHoras(4));

        await Servicio.ProgramarRecordatoriosAsync(24);

        var creadas = await Contexto.Notificaciones.Where(n => n.CiId == cita.CiId).ToListAsync();
        Assert.Single(creadas);
        Assert.Equal(correo.OpId, creadas[0].OpId);
    }

    [Fact]
    public async Task Omite_al_paciente_que_revoco_el_consentimiento()
    {
        var (paciente, _, _) = await PacienteConRecordatoriosAsync(consentimiento: "REVOCADO");
        var cita = await NuevaCitaAsync(paciente.PaId, EnHoras(4));

        var (_, omitidas) = await Servicio.ProgramarRecordatoriosAsync(24);

        Assert.True(omitidas >= 2);
        Assert.False(await Contexto.Notificaciones.AnyAsync(n => n.CiId == cita.CiId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(169)]
    public async Task Rechaza_horas_de_anticipacion_fuera_de_rango(int horas)
    {
        await Falla<ArgumentException>(() => Servicio.ProgramarRecordatoriosAsync(horas));
    }

    [Fact]
    public async Task Crea_una_notificacion_pendiente_con_plantilla_del_mismo_canal()
    {
        var (paciente, correo, _) = await PacienteConRecordatoriosAsync();
        var cita = await NuevaCitaAsync(paciente.PaId, EnHoras(48));

        var creada = await Servicio.CrearAsync(new NotificacionCrearDto
        {
            OpcionId = correo.OpId,
            PlantillaId = Semilla.PlantillaCorreo,
            CitaId = cita.CiId,
            UsuarioId = Semilla.UsuarioAdmin,
            FechaProgramada = EnHoras(24)
        });

        Assert.Equal("PENDIENTE", creada.Estado);
        Assert.Equal("RECORDATORIO_CITA", creada.Tipo);
        Assert.NotNull(await Servicio.ObtenerPorIdAsync(creada.Id));
        Assert.Contains(await Servicio.ObtenerTodosAsync(paciente.PaId, "pendiente", 1, 10), n => n.Id == creada.Id);
        Assert.Empty((await Servicio.ObtenerEnviosAsync(creada.Id))!);
    }

    [Fact]
    public async Task Rechaza_plantilla_de_otro_canal_fecha_local_o_pasada()
    {
        var (_, correo, _) = await PacienteConRecordatoriosAsync();

        await Falla<ArgumentException>(() => Servicio.CrearAsync(new NotificacionCrearDto
        {
            OpcionId = correo.OpId,
            PlantillaId = Semilla.PlantillaWhatsApp,
            FechaProgramada = EnHoras(2)
        }));

        await Falla<ArgumentException>(() => Servicio.CrearAsync(new NotificacionCrearDto
        {
            OpcionId = correo.OpId,
            PlantillaId = Semilla.PlantillaCorreo,
            FechaProgramada = DateTime.Now.AddHours(2)
        }));

        await Falla<ArgumentException>(() => Servicio.CrearAsync(new NotificacionCrearDto
        {
            OpcionId = correo.OpId,
            PlantillaId = Semilla.PlantillaCorreo,
            FechaProgramada = EnHoras(-2)
        }));

        await Falla<ArgumentException>(() => Servicio.CrearAsync(new NotificacionCrearDto
        {
            OpcionId = correo.OpId,
            PlantillaId = Semilla.PlantillaCorreo,
            FechaProgramada = null
        }));
    }

    [Fact]
    public async Task No_crea_notificaciones_sin_permiso_o_sin_consentimiento()
    {
        var (_, correo, whatsApp) = await PacienteConRecordatoriosAsync(permitirWhatsApp: false);

        await Falla<InvalidOperationException>(() => Servicio.CrearAsync(new NotificacionCrearDto
        {
            OpcionId = whatsApp.OpId,
            PlantillaId = Semilla.PlantillaWhatsApp,
            FechaProgramada = EnHoras(2)
        }));

        var consentimiento = await Contexto.Consentimientos.SingleAsync(c => c.PaId == correo.PaId);
        consentimiento.CoEstad = "REVOCADO";
        await Contexto.SaveChangesAsync();

        var error = await Falla<InvalidOperationException>(() => Servicio.CrearAsync(new NotificacionCrearDto
        {
            OpcionId = correo.OpId,
            PlantillaId = Semilla.PlantillaCorreo,
            FechaProgramada = EnHoras(2)
        }));

        Assert.Contains("REVOCADO", error.Message);
    }

    [Fact]
    public async Task La_notificacion_de_una_cita_debe_programarse_antes_de_la_cita()
    {
        var (paciente, correo, _) = await PacienteConRecordatoriosAsync();
        var cita = await NuevaCitaAsync(paciente.PaId, EnHoras(3));

        var error = await Falla<ArgumentException>(() => Servicio.CrearAsync(new NotificacionCrearDto
        {
            OpcionId = correo.OpId,
            PlantillaId = Semilla.PlantillaCorreo,
            CitaId = cita.CiId,
            FechaProgramada = EnHoras(5)
        }));

        Assert.Contains("antes de la cita", error.Message);
    }

    [Fact]
    public async Task Recorre_el_ciclo_pendiente_procesando_enviada_con_su_intento()
    {
        var (_, correo, _) = await PacienteConRecordatoriosAsync();
        var creada = await Servicio.CrearAsync(new NotificacionCrearDto
        {
            OpcionId = correo.OpId,
            PlantillaId = Semilla.PlantillaCorreo,
            FechaProgramada = EnHoras(1)
        });

        var procesando = await Servicio.CambiarEstadoAsync(creada.Id, new NotificacionCambiarEstadoDto
        {
            Estado = "PROCESANDO",
            Version = creada.Version
        });
        var envio = await Servicio.RegistrarEnvioAsync(creada.Id, new EnvioCrearDto
        {
            Resultado = "ENVIADO",
            IdProveedor = "msg-1"
        });

        Assert.Equal("PROCESANDO", procesando!.Estado);
        Assert.Equal("ENVIADO", envio!.Resultado);
        Assert.Equal("ENVIADA", (await Servicio.ObtenerPorIdAsync(creada.Id))!.Estado);
        Assert.Single((await Servicio.ObtenerEnviosAsync(creada.Id))!);
    }

    [Fact]
    public async Task Valida_estados_y_resultados_de_envio()
    {
        var (_, correo, _) = await PacienteConRecordatoriosAsync();
        var creada = await Servicio.CrearAsync(new NotificacionCrearDto
        {
            OpcionId = correo.OpId,
            PlantillaId = Semilla.PlantillaCorreo,
            FechaProgramada = EnHoras(1)
        });

        await Falla<ArgumentException>(() => Servicio.CambiarEstadoAsync(creada.Id, new NotificacionCambiarEstadoDto { Estado = "ENVIADA", Version = creada.Version }));
        await Falla<ArgumentException>(() => Servicio.CambiarEstadoAsync(creada.Id, new NotificacionCambiarEstadoDto { Estado = "OTRO", Version = creada.Version }));
        await Falla<InvalidOperationException>(() => Servicio.RegistrarEnvioAsync(creada.Id, new EnvioCrearDto { Resultado = "ENVIADO" }));
        await Falla<ArgumentException>(() => Servicio.RegistrarEnvioAsync(creada.Id, new EnvioCrearDto { Resultado = "ERROR" }));
        await Falla<ArgumentException>(() => Servicio.RegistrarEnvioAsync(creada.Id, new EnvioCrearDto { Resultado = "RARO" }));
        await Falla<ArgumentException>(() => Servicio.ObtenerTodosAsync(null, "RARO", 1, 10));
    }

    [Fact]
    public async Task Cancela_pendientes_y_no_permite_cancelar_las_enviadas()
    {
        var (_, correo, _) = await PacienteConRecordatoriosAsync();
        var creada = await Servicio.CrearAsync(new NotificacionCrearDto
        {
            OpcionId = correo.OpId,
            PlantillaId = Semilla.PlantillaCorreo,
            FechaProgramada = EnHoras(1)
        });

        var cancelada = await Servicio.CambiarEstadoAsync(creada.Id, new NotificacionCambiarEstadoDto { Estado = "CANCELADA", Version = creada.Version });

        Assert.Equal("CANCELADA", cancelada!.Estado);
        var error = await Falla<InvalidOperationException>(() => Servicio.CambiarEstadoAsync(creada.Id, new NotificacionCambiarEstadoDto { Estado = "CANCELADA", Version = cancelada.Version }));
        Assert.Contains("ya no se puede cancelar", error.Message);
        await Falla<InvalidOperationException>(() => Servicio.ActualizarAsync(creada.Id, new NotificacionActualizarDto
        {
            PlantillaId = Semilla.PlantillaCorreo,
            FechaProgramada = EnHoras(2),
            Version = cancelada.Version
        }));
    }

    [Fact]
    public async Task Un_error_de_envio_permite_reintentar()
    {
        var (_, correo, _) = await PacienteConRecordatoriosAsync();
        var creada = await Servicio.CrearAsync(new NotificacionCrearDto
        {
            OpcionId = correo.OpId,
            PlantillaId = Semilla.PlantillaCorreo,
            FechaProgramada = EnHoras(1)
        });
        await Servicio.CambiarEstadoAsync(creada.Id, new NotificacionCambiarEstadoDto { Estado = "PROCESANDO", Version = creada.Version });
        await Servicio.RegistrarEnvioAsync(creada.Id, new EnvioCrearDto { Resultado = "ERROR", CodigoError = "TIMEOUT" });
        var conError = await Servicio.ObtenerPorIdAsync(creada.Id);

        var reintento = await Servicio.CambiarEstadoAsync(creada.Id, new NotificacionCambiarEstadoDto { Estado = "PENDIENTE", Version = conError!.Version });

        Assert.Equal("ERROR", conError.Estado);
        Assert.Equal("PENDIENTE", reintento!.Estado);
    }

    [Fact]
    public async Task Actualiza_una_pendiente_y_detecta_version_vencida()
    {
        var (_, correo, _) = await PacienteConRecordatoriosAsync();
        var creada = await Servicio.CrearAsync(new NotificacionCrearDto
        {
            OpcionId = correo.OpId,
            PlantillaId = Semilla.PlantillaCorreo,
            FechaProgramada = EnHoras(1)
        });
        var otra = await NuevaPlantillaAsync("CORREO", "AVISO_ADMINISTRATIVO");

        Assert.True(await Servicio.ActualizarAsync(creada.Id, new NotificacionActualizarDto
        {
            PlantillaId = otra.PlId,
            FechaProgramada = EnHoras(3),
            Version = creada.Version
        }));
        Assert.Equal("AVISO_ADMINISTRATIVO", (await Servicio.ObtenerPorIdAsync(creada.Id))!.Tipo);

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => Servicio.ActualizarAsync(creada.Id, new NotificacionActualizarDto
        {
            PlantillaId = otra.PlId,
            FechaProgramada = EnHoras(4),
            Version = new byte[8]
        }));
    }

    [Fact]
    public async Task Devuelve_nulo_o_falso_si_la_notificacion_no_existe()
    {
        Assert.Null(await Servicio.ObtenerPorIdAsync(Semilla.IdInexistente));
        Assert.Null(await Servicio.ObtenerEnviosAsync(Semilla.IdInexistente));
        Assert.Null(await Servicio.CambiarEstadoAsync(Semilla.IdInexistente, new NotificacionCambiarEstadoDto { Estado = "CANCELADA", Version = new byte[8] }));
        Assert.Null(await Servicio.RegistrarEnvioAsync(Semilla.IdInexistente, new EnvioCrearDto { Resultado = "ENVIADO" }));
        Assert.False(await Servicio.ActualizarAsync(Semilla.IdInexistente, new NotificacionActualizarDto
        {
            PlantillaId = Semilla.PlantillaCorreo,
            FechaProgramada = EnHoras(1),
            Version = new byte[8]
        }));
        Assert.NotNull(await Servicio.ObtenerPorEnviarAsync(10));
    }
}
