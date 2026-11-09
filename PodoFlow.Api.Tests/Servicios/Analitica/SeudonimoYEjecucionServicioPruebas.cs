using PodoFlow.Api.DTOs.Ejecucion;
using PodoFlow.Api.DTOs.Encargado;
using PodoFlow.Api.DTOs.Seudonimo;

namespace PodoFlow.Api.Tests.Servicios.Analitica;

public class SeudonimoServicioPruebas : PruebaServicioBase
{
    public SeudonimoServicioPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    private ISeudonimoServicio Servicio => Servicio<ISeudonimoServicio>();

    [Fact]
    public async Task Solo_seudonimiza_pacientes_con_consentimiento_de_analitica()
    {
        var sinConsentimiento = await NuevoPacienteAsync();
        var conConsentimiento = await NuevoPacienteAsync();
        await NuevoConsentimientoAsync(conConsentimiento.PaId, Semilla.FinalidadAnalitica);

        var error = await Falla<InvalidOperationException>(() => Servicio.CrearAsync(new SeudonimoCrearDto { PacienteId = sinConsentimiento.PaId }));
        var creado = await Servicio.CrearAsync(new SeudonimoCrearDto { PacienteId = conConsentimiento.PaId });

        Assert.Contains("ANALITICA", error.Message);
        Assert.NotEqual(Guid.Empty, creado.Id);
        Assert.Equal(conConsentimiento.PaId, (await Servicio.ObtenerPorPacienteAsync(conConsentimiento.PaId))!.PacienteId);
        Assert.NotNull(await Servicio.ObtenerPorIdAsync(creado.Id));
        await Falla<InvalidOperationException>(() => Servicio.CrearAsync(new SeudonimoCrearDto { PacienteId = conConsentimiento.PaId }));
        await Falla<ArgumentException>(() => Servicio.CrearAsync(new SeudonimoCrearDto { PacienteId = Semilla.PacienteInactivo }));
    }

    [Fact]
    public async Task Un_consentimiento_revocado_impide_seudonimizar()
    {
        var paciente = await NuevoPacienteAsync();
        await NuevoConsentimientoAsync(paciente.PaId, Semilla.FinalidadAnalitica, "REVOCADO");

        var error = await Falla<InvalidOperationException>(() => Servicio.CrearAsync(new SeudonimoCrearDto { PacienteId = paciente.PaId }));

        Assert.Contains("REVOCADO", error.Message);
    }

    [Fact]
    public async Task Genera_seudonimos_en_lote_sin_repetir()
    {
        var p1 = await NuevoPacienteAsync();
        var p2 = await NuevoPacienteAsync();
        await NuevoConsentimientoAsync(p1.PaId, Semilla.FinalidadAnalitica);
        await NuevoConsentimientoAsync(p2.PaId, Semilla.FinalidadAnalitica);

        var primera = await Servicio.GenerarAsync();
        var segunda = await Servicio.GenerarAsync();

        Assert.True(primera.Creados >= 2);
        Assert.Equal(0, segunda.Creados);
        Assert.True(segunda.YaExistentes >= 2);
    }

    [Fact]
    public async Task Da_de_baja_y_reactiva_validando_el_consentimiento()
    {
        var paciente = await NuevoPacienteAsync();
        var consentimiento = await NuevoConsentimientoAsync(paciente.PaId, Semilla.FinalidadAnalitica);
        var creado = await Servicio.CrearAsync(new SeudonimoCrearDto { PacienteId = paciente.PaId });

        Assert.True(await Servicio.EliminarAsync(creado.Id));
        Assert.True(await Servicio.EliminarAsync(creado.Id));
        Assert.DoesNotContain(await Servicio.ObtenerTodosAsync(false, 1, 100), s => s.Id == creado.Id);
        var recreado = await Servicio.CrearAsync(new SeudonimoCrearDto { PacienteId = paciente.PaId });
        Assert.Equal(creado.Id, recreado.Id);

        await Servicio.EliminarAsync(creado.Id);
        consentimiento.CoEstad = "REVOCADO";
        await Contexto.SaveChangesAsync();
        await Falla<InvalidOperationException>(() => Servicio.ReactivarAsync(creado.Id));

        consentimiento.CoEstad = "ACEPTADO";
        await Contexto.SaveChangesAsync();
        Assert.True(await Servicio.ReactivarAsync(creado.Id));
        Assert.True(await Servicio.ReactivarAsync(creado.Id));
    }

    [Fact]
    public async Task Devuelve_nulo_o_falso_si_no_existe()
    {
        Assert.Null(await Servicio.ObtenerPorIdAsync(Guid.NewGuid()));
        Assert.Null(await Servicio.ObtenerPorPacienteAsync(Semilla.IdInexistente));
        Assert.False(await Servicio.EliminarAsync(Guid.NewGuid()));
        Assert.False(await Servicio.ReactivarAsync(Guid.NewGuid()));
    }
}

public class EjecucionServicioPruebas : PruebaServicioBase
{
    public EjecucionServicioPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    private IEjecucionServicio Servicio => Servicio<IEjecucionServicio>();

    private async Task<Guid> SeudonimoAutorizadoAsync()
    {
        var paciente = await NuevoPacienteAsync();
        await NuevoConsentimientoAsync(paciente.PaId, Semilla.FinalidadAnalitica);
        return (await Servicio<ISeudonimoServicio>().CrearAsync(new SeudonimoCrearDto { PacienteId = paciente.PaId })).Id;
    }

    private async Task<EjecucionRespuestaDto> CambiarAsync(long id, string estado, string? entrada = null, string? salida = null, string? observacion = null)
        => (await Servicio.CambiarEstadoAsync(id, new EjecucionCambiarEstadoDto { Estado = estado, HuellaEntrada = entrada, HuellaSalida = salida, Observacion = observacion }))!;

    [Fact]
    public async Task Recorre_el_ciclo_completo_de_una_ejecucion_analitica()
    {
        var s1 = await SeudonimoAutorizadoAsync();
        var s2 = await SeudonimoAutorizadoAsync();
        var creada = await Servicio.CrearAsync(new EjecucionCrearDto { UsuarioId = Semilla.UsuarioAdmin, Observacion = " Segmentación " });

        Assert.Equal("PENDIENTE", creada.Estado);
        await Falla<ArgumentException>(() => CambiarAsync(creada.Id, "PROCESANDO"));
        await CambiarAsync(creada.Id, "procesando", entrada: "huella-entrada");

        var cargados = await Servicio.CargarDatosAsync(creada.Id, new DatoAnaliticoLoteDto
        {
            Datos =
            [
                new DatoAnaliticoCrearDto { SeudonimoId = s1, FrecuenciaAtenciones = 4, CategoriaServicio = " Quiropodia ", DiasInactividad = 20 },
                new DatoAnaliticoCrearDto { SeudonimoId = s2, FrecuenciaAtenciones = 1, FechaUltimaAtencion = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-90)) }
            ]
        });
        await Falla<ArgumentException>(() => CambiarAsync(creada.Id, "COMPLETADA"));
        var completada = await CambiarAsync(creada.Id, "COMPLETADA", salida: "huella-salida");

        Assert.Equal(2, cargados);
        Assert.Equal(2, completada.CantidadRegistros);
        Assert.Equal(2, (await Servicio.ObtenerDatosAsync(creada.Id, 1, 10))!.Count);

        var recomendacion = await Servicio.CrearRecomendacionAsync(creada.Id, new RecomendacionCrearDto
        {
            Segmento = "Inactivos",
            Hallazgo = "Pacientes sin visitas",
            Sugerencia = "Campaña de retorno",
            PacientesEstimados = 1
        });

        Assert.Equal("SEUDONIMIZADO", recomendacion!.NivelPrivacidad);
        Assert.Single((await Servicio.ObtenerRecomendacionesAsync(creada.Id))!);
        await Falla<ArgumentException>(() => Servicio.CrearRecomendacionAsync(creada.Id, new RecomendacionCrearDto { Segmento = "x", Hallazgo = "x", Sugerencia = "x", PacientesEstimados = 5 }));
        await Falla<ArgumentException>(() => Servicio.CrearRecomendacionAsync(creada.Id, new RecomendacionCrearDto { Segmento = "x", Hallazgo = "x", Sugerencia = "x", NivelPrivacidad = "NOMINAL" }));
        await Falla<InvalidOperationException>(() => CambiarAsync(creada.Id, "ERROR", observacion: "tarde"));
        await Falla<InvalidOperationException>(() => Servicio.ActualizarAsync(creada.Id, new EjecucionActualizarDto()));
    }

    [Fact]
    public async Task Rechaza_lotes_invalidos_o_con_seudonimos_no_autorizados()
    {
        var autorizado = await SeudonimoAutorizadoAsync();
        var creada = await Servicio.CrearAsync(new EjecucionCrearDto { UsuarioId = Semilla.UsuarioAdmin });

        await Falla<InvalidOperationException>(() => Servicio.CargarDatosAsync(creada.Id, new DatoAnaliticoLoteDto { Datos = [new() { SeudonimoId = autorizado }] }));
        await CambiarAsync(creada.Id, "PROCESANDO", entrada: "h");

        await Falla<ArgumentException>(() => Servicio.CargarDatosAsync(creada.Id, new DatoAnaliticoLoteDto { Datos = [new() { SeudonimoId = Guid.Empty }] }));
        await Falla<ArgumentException>(() => Servicio.CargarDatosAsync(creada.Id, new DatoAnaliticoLoteDto { Datos = [new() { SeudonimoId = autorizado, FechaUltimaAtencion = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)) }] }));
        await Falla<ArgumentException>(() => Servicio.CargarDatosAsync(creada.Id, new DatoAnaliticoLoteDto { Datos = [new() { SeudonimoId = autorizado }, new() { SeudonimoId = autorizado }] }));
        await Falla<InvalidOperationException>(() => Servicio.CargarDatosAsync(creada.Id, new DatoAnaliticoLoteDto { Datos = [new() { SeudonimoId = Guid.NewGuid() }] }));

        await Servicio.CargarDatosAsync(creada.Id, new DatoAnaliticoLoteDto { Datos = [new() { SeudonimoId = autorizado }] });
        await Falla<InvalidOperationException>(() => Servicio.CargarDatosAsync(creada.Id, new DatoAnaliticoLoteDto { Datos = [new() { SeudonimoId = autorizado }] }));
        await Falla<InvalidOperationException>(() => Servicio.CrearRecomendacionAsync(creada.Id, new RecomendacionCrearDto { Segmento = "x", Hallazgo = "x", Sugerencia = "x" }));
    }

    [Fact]
    public async Task Pasar_a_error_exige_motivo_y_valida_el_encargado()
    {
        var encargado = await Servicio<IEncargadoServicio>().CrearAsync(new EncargadoCrearDto { Nombre = "Analítica " + Identificaciones.Sufijo(), Servicio = "BI" });
        var creada = await Servicio.CrearAsync(new EjecucionCrearDto { UsuarioId = Semilla.UsuarioAdmin, EncargadoId = encargado.Id });

        Assert.True(await Servicio.ActualizarAsync(creada.Id, new EjecucionActualizarDto { EncargadoId = null, Observacion = "Sin encargado" }));
        await Falla<ArgumentException>(() => Servicio.ActualizarAsync(creada.Id, new EjecucionActualizarDto { EncargadoId = int.MaxValue }));
        await Falla<ArgumentException>(() => Servicio.CrearAsync(new EjecucionCrearDto { UsuarioId = Semilla.UsuarioInactivo }));
        await Falla<ArgumentException>(() => CambiarAsync(creada.Id, "ERROR"));
        Assert.Equal("ERROR", (await CambiarAsync(creada.Id, "ERROR", observacion: "Fuente no disponible")).Estado);
        await Falla<ArgumentException>(() => CambiarAsync(creada.Id, "RARO"));
        await Falla<ArgumentException>(() => Servicio.ObtenerTodosAsync("RARO", 1, 10));
        Assert.Contains(await Servicio.ObtenerTodosAsync("error", 1, 100), e => e.Id == creada.Id);
    }

    [Fact]
    public async Task Devuelve_nulo_o_falso_si_no_existe()
    {
        Assert.Null(await Servicio.ObtenerPorIdAsync(Semilla.IdInexistente));
        Assert.Null(await Servicio.ObtenerDatosAsync(Semilla.IdInexistente, 1, 10));
        Assert.Null(await Servicio.ObtenerRecomendacionesAsync(Semilla.IdInexistente));
        Assert.Null(await Servicio.CambiarEstadoAsync(Semilla.IdInexistente, new EjecucionCambiarEstadoDto { Estado = "ERROR" }));
        Assert.Null(await Servicio.CargarDatosAsync(Semilla.IdInexistente, new DatoAnaliticoLoteDto { Datos = [] }));
        Assert.Null(await Servicio.CrearRecomendacionAsync(Semilla.IdInexistente, new RecomendacionCrearDto { Segmento = "x", Hallazgo = "x", Sugerencia = "x" }));
        Assert.False(await Servicio.ActualizarAsync(Semilla.IdInexistente, new EjecucionActualizarDto()));
    }
}
