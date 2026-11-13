using PodoFlow.Api.DTOs.OpcionComunicacion;
using PodoFlow.Api.DTOs.Plantilla;

namespace PodoFlow.Api.Tests.Servicios.Comunicacion;

public class PlantillaServicioPruebas : PruebaServicioBase
{
    public PlantillaServicioPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    private IPlantillaServicio Servicio => Servicio<IPlantillaServicio>();

    private static PlantillaCrearDto Nueva(string canal, string tipo, string? asunto = "Asunto")
        => new()
        {
            Nombre = "plantilla " + Identificaciones.Sufijo(),
            Canal = canal,
            Tipo = tipo,
            Asunto = asunto,
            Contenido = "Hola {{1}}"
        };

    [Theory]
    [InlineData("RECORDATORIO_CITA")]
    [InlineData("confirmacion_cita")]
    [InlineData("AVISO_ADMINISTRATIVO")]
    public async Task WhatsApp_acepta_los_tipos_operativos(string tipo)
    {
        var creada = await Servicio.CrearAsync(Nueva("whatsapp", tipo));

        Assert.Equal("WHATSAPP", creada.Canal);
        Assert.Null(creada.Asunto);
        Assert.Equal("1.0", creada.Version);
    }

    [Theory]
    [InlineData("PROMOCION")]
    [InlineData("FIDELIZACION")]
    public async Task WhatsApp_rechaza_los_tipos_no_operativos(string tipo)
    {
        var error = await Falla<ArgumentException>(() => Servicio.CrearAsync(Nueva("WHATSAPP", tipo)));

        Assert.Contains("solo se envían mensajes operativos", error.Message);
    }

    [Fact]
    public async Task WhatsApp_limita_el_contenido_a_4096_caracteres()
    {
        var dto = Nueva("WHATSAPP", "RECORDATORIO_CITA");
        dto.Contenido = new string('a', 4097);

        await Falla<ArgumentException>(() => Servicio.CrearAsync(dto));
    }

    [Fact]
    public async Task Correo_exige_asunto_y_admite_tipos_de_fidelizacion()
    {
        await Falla<ArgumentException>(() => Servicio.CrearAsync(Nueva("CORREO", "PROMOCION", asunto: " ")));

        var creada = await Servicio.CrearAsync(Nueva("CORREO", "PROMOCION"));

        Assert.Equal("PROMOCION", creada.Tipo);
    }

    [Fact]
    public async Task Rechaza_canal_desconocido_tipo_o_contenido_vacio()
    {
        await Falla<ArgumentException>(() => Servicio.CrearAsync(Nueva("SMS", "RECORDATORIO_CITA")));

        var sinTipo = Nueva("CORREO", "  ");
        await Falla<ArgumentException>(() => Servicio.CrearAsync(sinTipo));

        var sinContenido = Nueva("CORREO", "AVISO_ADMINISTRATIVO");
        sinContenido.Contenido = "   ";
        await Falla<ArgumentException>(() => Servicio.CrearAsync(sinContenido));
    }

    [Fact]
    public async Task No_permite_duplicar_nombre_canal_y_version()
    {
        var dto = Nueva("CORREO", "AVISO_ADMINISTRATIVO");
        var creada = await Servicio.CrearAsync(dto);

        await Falla<InvalidOperationException>(() => Servicio.CrearAsync(dto));

        Assert.True(await Servicio.EliminarAsync(creada.Id));
        var error = await Falla<InvalidOperationException>(() => Servicio.CrearAsync(dto));
        Assert.Contains("dada de baja", error.Message);
    }

    [Fact]
    public async Task Edita_mientras_no_se_haya_usado_y_bloquea_despues()
    {
        var creada = await Servicio.CrearAsync(Nueva("CORREO", "AVISO_ADMINISTRATIVO"));

        Assert.True(await Servicio.ActualizarAsync(creada.Id, new PlantillaActualizarDto
        {
            Nombre = creada.Nombre,
            Canal = "CORREO",
            Tipo = "AVISO_ADMINISTRATIVO",
            Asunto = "Nuevo asunto",
            Contenido = "Nuevo contenido",
            Version = "2.0"
        }));
        Assert.Equal("2.0", (await Servicio.ObtenerPorIdAsync(creada.Id))!.Version);

        var paciente = await NuevoPacienteAsync();
        var opcion = await NuevaOpcionAsync(paciente.PaId, Semilla.FinalidadCitas, "CORREO", true);
        Contexto.Notificaciones.Add(new PodoFlow.Api.Modelos.Notificacion
        {
            PaId = paciente.PaId,
            PlId = creada.Id,
            OpId = opcion.OpId,
            NoTipo = "AVISO_ADMINISTRATIVO",
            NoCanal = "CORREO",
            NoFechaProg = DateTime.UtcNow.AddHours(1),
            NoEstad = "PENDIENTE",
            NoFechaCrea = DateTime.UtcNow
        });
        await Contexto.SaveChangesAsync();

        await Falla<InvalidOperationException>(() => Servicio.ActualizarAsync(creada.Id, new PlantillaActualizarDto
        {
            Nombre = creada.Nombre,
            Canal = "CORREO",
            Tipo = "AVISO_ADMINISTRATIVO",
            Asunto = "Otro",
            Contenido = "Otro"
        }));
        var error = await Falla<InvalidOperationException>(() => Servicio.EliminarAsync(creada.Id));
        Assert.Contains("pendientes", error.Message);
    }

    [Fact]
    public async Task Da_de_baja_y_reactiva_sin_chocar_con_otra_activa()
    {
        var dto = Nueva("CORREO", "AVISO_ADMINISTRATIVO");
        var primera = await Servicio.CrearAsync(dto);
        await Servicio.EliminarAsync(primera.Id);
        Assert.True(await Servicio.EliminarAsync(primera.Id));

        Assert.True(await Servicio.ReactivarAsync(primera.Id));
        Assert.True(await Servicio.ReactivarAsync(primera.Id));
        Assert.True((await Servicio.ObtenerPorIdAsync(primera.Id))!.Estatus);
    }

    [Fact]
    public async Task Filtra_por_canal_y_tipo()
    {
        var lista = await Servicio.ObtenerTodosAsync("whatsapp", "recordatorio_cita", false, 1, 100);

        Assert.Contains(lista, p => p.Id == Semilla.PlantillaWhatsApp);
        Assert.All(lista, p => Assert.Equal("WHATSAPP", p.Canal));
        await Falla<ArgumentException>(() => Servicio.ObtenerTodosAsync("FAX", null, false, 1, 10));
    }

    [Fact]
    public async Task Devuelve_nulo_o_falso_si_no_existe()
    {
        Assert.Null(await Servicio.ObtenerPorIdAsync(int.MaxValue));
        Assert.False(await Servicio.EliminarAsync(int.MaxValue));
        Assert.False(await Servicio.ReactivarAsync(int.MaxValue));
        Assert.False(await Servicio.ActualizarAsync(int.MaxValue, new PlantillaActualizarDto { Nombre = "x", Canal = "CORREO", Tipo = "X", Asunto = "x", Contenido = "x" }));
    }
}

public class OpcionComunicacionServicioPruebas : PruebaServicioBase
{
    public OpcionComunicacionServicioPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    private IOpcionComunicacionServicio Servicio => Servicio<IOpcionComunicacionServicio>();

    [Fact]
    public async Task Permite_canales_de_finalidades_sin_consentimiento()
    {
        var paciente = await NuevoPacienteAsync();

        var creada = await Servicio.CrearAsync(new OpcionComunicacionCrearDto
        {
            PacienteId = paciente.PaId,
            FinalidadId = await FinalidadIdAsync(Semilla.FinalidadCitas),
            Canal = " correo ",
            Permitido = true
        });

        Assert.Equal("CORREO", creada.Canal);
        Assert.Null(creada.ConsentimientoId);
        Assert.Contains(await Servicio.ObtenerTodosAsync(paciente.PaId, null, "CORREO", false, 1, 10), o => o.Id == creada.Id);
    }

    [Fact]
    public async Task Exige_consentimiento_aceptado_para_finalidades_que_lo_requieren()
    {
        var paciente = await NuevoPacienteAsync();
        var finalidad = await FinalidadIdAsync(Semilla.FinalidadRecordatorios);

        var sinConsentimiento = await Falla<InvalidOperationException>(() => Servicio.CrearAsync(new OpcionComunicacionCrearDto
        {
            PacienteId = paciente.PaId,
            FinalidadId = finalidad,
            Canal = "WHATSAPP",
            Permitido = true
        }));
        Assert.Contains("no lo tiene registrado", sinConsentimiento.Message);

        var consentimiento = await NuevoConsentimientoAsync(paciente.PaId, Semilla.FinalidadRecordatorios);
        var creada = await Servicio.CrearAsync(new OpcionComunicacionCrearDto
        {
            PacienteId = paciente.PaId,
            FinalidadId = finalidad,
            Canal = "WHATSAPP",
            Permitido = true
        });
        Assert.Equal(consentimiento.CoId, creada.ConsentimientoId);
    }

    [Fact]
    public async Task Un_consentimiento_revocado_impide_activar_el_permiso()
    {
        var paciente = await NuevoPacienteAsync();
        var revocado = await NuevoConsentimientoAsync(paciente.PaId, Semilla.FinalidadFidelizacion, "REVOCADO");

        var error = await Falla<InvalidOperationException>(() => Servicio.CrearAsync(new OpcionComunicacionCrearDto
        {
            PacienteId = paciente.PaId,
            FinalidadId = revocado.FiId,
            Canal = "CORREO",
            Permitido = true
        }));

        Assert.Contains("REVOCADO", error.Message);
    }

    [Fact]
    public async Task Registrar_un_no_permitido_no_exige_consentimiento()
    {
        var paciente = await NuevoPacienteAsync();

        var creada = await Servicio.CrearAsync(new OpcionComunicacionCrearDto
        {
            PacienteId = paciente.PaId,
            FinalidadId = await FinalidadIdAsync(Semilla.FinalidadFidelizacion),
            Canal = "CORREO",
            Permitido = false
        });

        Assert.False(creada.Permitido);
    }

    [Fact]
    public async Task Rechaza_canal_paciente_inactivo_y_duplicados()
    {
        var finalidad = await FinalidadIdAsync(Semilla.FinalidadCitas);

        await Falla<ArgumentException>(() => Servicio.CrearAsync(new OpcionComunicacionCrearDto { PacienteId = Semilla.PacienteAdulto, FinalidadId = finalidad, Canal = "SMS", Permitido = true }));
        await Falla<ArgumentException>(() => Servicio.CrearAsync(new OpcionComunicacionCrearDto { PacienteId = Semilla.PacienteInactivo, FinalidadId = finalidad, Canal = "CORREO", Permitido = true }));
        await Falla<ArgumentException>(() => Servicio.CrearAsync(new OpcionComunicacionCrearDto { PacienteId = Semilla.PacienteAdulto, FinalidadId = int.MaxValue, Canal = "CORREO", Permitido = true }));

        var paciente = await NuevoPacienteAsync();
        var dto = new OpcionComunicacionCrearDto { PacienteId = paciente.PaId, FinalidadId = finalidad, Canal = "CORREO", Permitido = true };
        await Servicio.CrearAsync(dto);
        await Falla<InvalidOperationException>(() => Servicio.CrearAsync(dto));
    }

    [Fact]
    public async Task Cambia_el_permiso_da_de_baja_y_reactiva()
    {
        var paciente = await NuevoPacienteAsync();
        var consentimiento = await NuevoConsentimientoAsync(paciente.PaId, Semilla.FinalidadFidelizacion);
        var creada = await Servicio.CrearAsync(new OpcionComunicacionCrearDto
        {
            PacienteId = paciente.PaId,
            FinalidadId = consentimiento.FiId,
            Canal = "CORREO",
            Permitido = true
        });

        Assert.True(await Servicio.ActualizarAsync(creada.Id, new OpcionComunicacionActualizarDto { Permitido = false }));
        Assert.Null((await Servicio.ObtenerPorIdAsync(creada.Id))!.ConsentimientoId);
        Assert.True(await Servicio.ActualizarAsync(creada.Id, new OpcionComunicacionActualizarDto { Permitido = true }));
        Assert.True(await Servicio.EliminarAsync(creada.Id));
        Assert.True(await Servicio.EliminarAsync(creada.Id));

        var recreada = await Servicio.CrearAsync(new OpcionComunicacionCrearDto
        {
            PacienteId = paciente.PaId,
            FinalidadId = consentimiento.FiId,
            Canal = "CORREO",
            Permitido = true
        });
        Assert.Equal(creada.Id, recreada.Id);

        await Servicio.EliminarAsync(creada.Id);
        Assert.True(await Servicio.ReactivarAsync(creada.Id));
        Assert.True(await Servicio.ReactivarAsync(creada.Id));
    }

    [Fact]
    public async Task Devuelve_nulo_o_falso_si_no_existe()
    {
        Assert.Null(await Servicio.ObtenerPorIdAsync(Semilla.IdInexistente));
        Assert.False(await Servicio.ActualizarAsync(Semilla.IdInexistente, new OpcionComunicacionActualizarDto { Permitido = true }));
        Assert.False(await Servicio.EliminarAsync(Semilla.IdInexistente));
        Assert.False(await Servicio.ReactivarAsync(Semilla.IdInexistente));
        await Falla<ArgumentException>(() => Servicio.ObtenerTodosAsync(null, null, "FAX", false, 1, 10));
    }
}
