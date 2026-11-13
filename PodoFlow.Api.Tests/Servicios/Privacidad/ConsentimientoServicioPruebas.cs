using PodoFlow.Api.DTOs.Consentimiento;

namespace PodoFlow.Api.Tests.Servicios.Privacidad;

public class ConsentimientoServicioPruebas : PruebaServicioBase
{
    public ConsentimientoServicioPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    private IConsentimientoServicio Servicio => Servicio<IConsentimientoServicio>();

    private async Task<ConsentimientoCrearDto> SolicitudAsync(long pacienteId, string finalidad = Semilla.FinalidadRecordatorios)
        => new()
        {
            PacienteId = pacienteId,
            FinalidadId = await FinalidadIdAsync(finalidad),
            AvisoId = Semilla.Aviso,
            Estado = "aceptado",
            Medio = "presencial",
            UsuarioId = Semilla.UsuarioAdmin
        };

    [Fact]
    public async Task Registra_el_consentimiento_del_titular_adulto_y_su_evento()
    {
        var paciente = await NuevoPacienteAsync();

        var creado = await Servicio.CrearAsync(await SolicitudAsync(paciente.PaId));

        Assert.Equal("ACEPTADO", creado.Estado);
        Assert.Equal("PRESENCIAL", creado.Medio);
        Assert.Equal("Autorizado por el titular", creado.Observacion);
        var eventos = await Servicio.ObtenerEventosAsync(creado.Id);
        Assert.Single(eventos!);
        Assert.Equal("ACEPTADO", eventos![0].Estado);
        Assert.Contains(await Servicio.ObtenerPorPacienteAsync(paciente.PaId), c => c.Id == creado.Id);
        Assert.NotNull(await Servicio.ObtenerPorIdAsync(creado.Id));
        Assert.NotEmpty(await Servicio.ObtenerTodosAsync(1, 10));
    }

    [Fact]
    public async Task Exige_representante_cuando_el_paciente_es_menor_de_edad()
    {
        var dto = await SolicitudAsync(Semilla.PacienteMenor);

        var error = await Falla<ArgumentException>(() => Servicio.CrearAsync(dto));

        Assert.Contains("menor de edad", error.Message);
    }

    [Fact]
    public async Task Acepta_al_menor_si_firma_un_representante_valido()
    {
        var menor = await NuevoPacienteAsync(DateTime.UtcNow.AddYears(-10));
        var dto = await SolicitudAsync(menor.PaId);
        dto.RepresentanteNombre = "  María   Pérez ";
        dto.RepresentanteRelacion = "madre o padre";
        dto.Observacion = "Firmado en consulta";

        var creado = await Servicio.CrearAsync(dto);

        Assert.Equal("Autorizado por representante: María Pérez (Madre o padre). Firmado en consulta", creado.Observacion);
    }

    [Theory]
    [InlineData("Jo", "Madre o padre", "al menos 3 letras")]
    [InlineData("Juan 2", "Madre o padre", "al menos 3 letras")]
    [InlineData("Juan Pérez", "Vecino", "La relación del representante")]
    public async Task Rechaza_representantes_con_datos_invalidos(string nombre, string relacion, string mensaje)
    {
        var dto = await SolicitudAsync(Semilla.PacienteMenor);
        dto.RepresentanteNombre = nombre;
        dto.RepresentanteRelacion = relacion;

        var error = await Falla<ArgumentException>(() => Servicio.CrearAsync(dto));

        Assert.Contains(mensaje, error.Message);
    }

    [Fact]
    public async Task Rechaza_un_medio_fuera_de_la_lista()
    {
        var dto = await SolicitudAsync(Semilla.PacienteAdulto);
        dto.Medio = "FAX";

        var error = await Falla<ArgumentException>(() => Servicio.CrearAsync(dto));

        Assert.Contains("PRESENCIAL, CORREO, WHATSAPP o TELEFONO", error.Message);
    }

    [Fact]
    public async Task Rechaza_una_finalidad_que_no_requiere_consentimiento()
    {
        var dto = await SolicitudAsync(Semilla.PacienteAdulto, Semilla.FinalidadAtencion);

        var error = await Falla<ArgumentException>(() => Servicio.CrearAsync(dto));

        Assert.Contains("no requiere consentimiento", error.Message);
    }

    [Fact]
    public async Task Rechaza_al_paciente_inactivo()
    {
        var dto = await SolicitudAsync(Semilla.PacienteInactivo);

        var error = await Falla<ArgumentException>(() => Servicio.CrearAsync(dto));

        Assert.Contains("paciente activo", error.Message);
    }

    [Fact]
    public async Task Rechaza_un_aviso_que_ya_no_esta_vigente()
    {
        var vencido = new PodoFlow.Api.Modelos.Aviso
        {
            AvVersi = "VENCIDO-" + Identificaciones.Sufijo(),
            AvFechaDesd = new DateOnly(2020, 1, 1),
            AvFechaHast = new DateOnly(2021, 1, 1),
            AvEstatus = true
        };
        Contexto.Avisos.Add(vencido);
        await Contexto.SaveChangesAsync();
        var dto = await SolicitudAsync(Semilla.PacienteAdulto);
        dto.AvisoId = vencido.AvId;

        var error = await Falla<ArgumentException>(() => Servicio.CrearAsync(dto));

        Assert.Contains("no está vigente", error.Message);
    }

    [Fact]
    public async Task Rechaza_estados_iniciales_distintos_de_aceptado_o_rechazado()
    {
        var dto = await SolicitudAsync(Semilla.PacienteAdulto);
        dto.Estado = "REVOCADO";

        await Falla<ArgumentException>(() => Servicio.CrearAsync(dto));

        dto.Estado = "PENDIENTE";
        await Falla<ArgumentException>(() => Servicio.CrearAsync(dto));
    }

    [Fact]
    public async Task Rechaza_un_usuario_inactivo()
    {
        var dto = await SolicitudAsync(Semilla.PacienteAdulto);
        dto.UsuarioId = Semilla.UsuarioInactivo;

        await Falla<ArgumentException>(() => Servicio.CrearAsync(dto));
    }

    [Fact]
    public async Task No_duplica_el_consentimiento_de_la_misma_finalidad()
    {
        var dto = await SolicitudAsync(Semilla.PacienteAdulto, Semilla.FinalidadFidelizacion);

        var error = await Falla<InvalidOperationException>(() => Servicio.CrearAsync(dto));

        Assert.Contains("ya tiene un consentimiento", error.Message);
    }

    [Fact]
    public async Task Sigue_las_transiciones_aceptado_revocado_aceptado()
    {
        var paciente = await NuevoPacienteAsync();
        var creado = await Servicio.CrearAsync(await SolicitudAsync(paciente.PaId));

        var revocado = await Servicio.CambiarEstadoAsync(creado.Id, new ConsentimientoCambiarEstadoDto
        {
            Estado = "REVOCADO",
            Medio = "CORREO",
            Version = creado.Version
        });

        Assert.Equal("REVOCADO", revocado!.Estado);
        Assert.NotNull(revocado.FechaRevocacion);

        var aceptado = await Servicio.CambiarEstadoAsync(creado.Id, new ConsentimientoCambiarEstadoDto
        {
            Estado = "ACEPTADO",
            Medio = "WHATSAPP",
            AvisoId = Semilla.Aviso,
            Version = revocado.Version
        });

        Assert.Equal("ACEPTADO", aceptado!.Estado);
        Assert.Null(aceptado.FechaRevocacion);
        Assert.Equal(3, (await Servicio.ObtenerEventosAsync(creado.Id))!.Count);
    }

    [Fact]
    public async Task No_permite_pasar_de_aceptado_a_rechazado()
    {
        var paciente = await NuevoPacienteAsync();
        var creado = await Servicio.CrearAsync(await SolicitudAsync(paciente.PaId));

        var error = await Falla<InvalidOperationException>(() => Servicio.CambiarEstadoAsync(creado.Id, new ConsentimientoCambiarEstadoDto
        {
            Estado = "RECHAZADO",
            Medio = "CORREO",
            Version = creado.Version
        }));

        Assert.Contains("No se puede pasar de ACEPTADO a RECHAZADO", error.Message);
    }

    [Fact]
    public async Task Volver_a_aceptar_exige_la_version_del_aviso()
    {
        var paciente = await NuevoPacienteAsync();
        var dto = await SolicitudAsync(paciente.PaId);
        dto.Estado = "RECHAZADO";
        var creado = await Servicio.CrearAsync(dto);

        var error = await Falla<ArgumentException>(() => Servicio.CambiarEstadoAsync(creado.Id, new ConsentimientoCambiarEstadoDto
        {
            Estado = "ACEPTADO",
            Medio = "PRESENCIAL",
            Version = creado.Version
        }));

        Assert.Contains("AvisoId", error.Message);
    }

    [Fact]
    public async Task No_acepta_si_el_paciente_fue_dado_de_baja()
    {
        var paciente = await NuevoPacienteAsync();
        var dto = await SolicitudAsync(paciente.PaId);
        dto.Estado = "RECHAZADO";
        var creado = await Servicio.CrearAsync(dto);
        paciente.PaEstatus = false;
        await Contexto.SaveChangesAsync();

        var error = await Falla<InvalidOperationException>(() => Servicio.CambiarEstadoAsync(creado.Id, new ConsentimientoCambiarEstadoDto
        {
            Estado = "ACEPTADO",
            Medio = "PRESENCIAL",
            AvisoId = Semilla.Aviso,
            Version = creado.Version
        }));

        Assert.Contains("dado de baja", error.Message);
    }

    [Fact]
    public async Task El_cambio_de_estado_con_version_vencida_lanza_concurrencia()
    {
        var paciente = await NuevoPacienteAsync();
        var creado = await Servicio.CrearAsync(await SolicitudAsync(paciente.PaId));

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => Servicio.CambiarEstadoAsync(creado.Id, new ConsentimientoCambiarEstadoDto
        {
            Estado = "REVOCADO",
            Medio = "CORREO",
            Version = new byte[8]
        }));
    }

    [Fact]
    public async Task Devuelve_nulo_o_falso_cuando_no_existe()
    {
        Assert.Null(await Servicio.ObtenerPorIdAsync(Semilla.IdInexistente));
        Assert.Null(await Servicio.ObtenerEventosAsync(Semilla.IdInexistente));
        Assert.Null(await Servicio.CambiarEstadoAsync(Semilla.IdInexistente, new ConsentimientoCambiarEstadoDto
        {
            Estado = "REVOCADO",
            Medio = "CORREO",
            Version = new byte[8]
        }));
        Assert.False(await Servicio.ActualizarAsync(Semilla.IdInexistente, new ConsentimientoActualizarDto
        {
            Medio = "CORREO",
            Version = new byte[8]
        }));
    }

    [Fact]
    public async Task Actualiza_medio_y_observacion_con_la_version_vigente()
    {
        var paciente = await NuevoPacienteAsync();
        var creado = await Servicio.CrearAsync(await SolicitudAsync(paciente.PaId));

        var actualizado = await Servicio.ActualizarAsync(creado.Id, new ConsentimientoActualizarDto
        {
            Medio = "TELEFONO",
            Observacion = " Confirmado por llamada ",
            Version = creado.Version
        });

        Assert.True(actualizado);
        var leido = await Servicio.ObtenerPorIdAsync(creado.Id);
        Assert.Equal("TELEFONO", leido!.Medio);
        Assert.Equal("Confirmado por llamada", leido.Observacion);
    }

    [Fact]
    public async Task Actualizar_tambien_valida_que_el_medio_este_en_la_lista()
    {
        var paciente = await NuevoPacienteAsync();
        var creado = await Servicio.CrearAsync(await SolicitudAsync(paciente.PaId));

        var error = await Falla<ArgumentException>(() => Servicio.ActualizarAsync(creado.Id, new ConsentimientoActualizarDto
        {
            Medio = "fax",
            Version = creado.Version
        }));

        Assert.Contains("PRESENCIAL, CORREO, WHATSAPP o TELEFONO", error.Message);
    }
}
