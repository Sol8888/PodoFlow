using PodoFlow.Api.DTOs.Documento;
using PodoFlow.Api.DTOs.EvaluacionImpacto;
using PodoFlow.Api.DTOs.EvaluacionRiesgo;
using PodoFlow.Api.DTOs.Incidente;
using PodoFlow.Api.DTOs.Riesgo;

namespace PodoFlow.Api.Tests.Servicios.Privacidad;

public class IncidenteServicioPruebas : PruebaServicioBase
{
    public IncidenteServicioPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    private IIncidenteServicio Servicio => Servicio<IIncidenteServicio>();

    private Task<IncidenteRespuestaDto> RegistrarAsync()
        => Servicio.CrearAsync(new IncidenteCrearDto
        {
            UsuarioId = Semilla.UsuarioAdmin,
            FechaDeteccion = DateTime.UtcNow.AddHours(-2),
            Tipo = " acceso_no_autorizado ",
            Modulo = "clinica",
            Descripcion = "Se detectó un acceso indebido",
            DatosAfectados = "Historias clínicas"
        });

    private async Task<IncidenteRespuestaDto> CambiarAsync(IncidenteRespuestaDto incidente, string estado, string? observacion = null)
        => (await Servicio.CambiarEstadoAsync(incidente.Id, new IncidenteCambiarEstadoDto
        {
            Estado = estado,
            UsuarioId = Semilla.UsuarioAdmin,
            Observacion = observacion,
            Version = incidente.Version
        }))!;

    [Fact]
    public async Task Registra_el_incidente_abierto_con_su_accion_inicial()
    {
        var creado = await RegistrarAsync();

        Assert.Equal("ABIERTO", creado.Estado);
        Assert.Equal("ACCESO_NO_AUTORIZADO", creado.Tipo);
        Assert.Equal("CLINICA", creado.Modulo);
        Assert.Equal(0, creado.FechaDeteccion.Millisecond);
        Assert.Equal("REGISTRO", Assert.Single((await Servicio.ObtenerAccionesAsync(creado.Id))!).Tipo);
        Assert.Contains(await Servicio.ObtenerTodosAsync("abierto", 1, 100), i => i.Id == creado.Id);
    }

    [Fact]
    public async Task Rechaza_fechas_futuras_sin_zona_utc_o_usuario_inactivo()
    {
        var dto = new IncidenteCrearDto
        {
            UsuarioId = Semilla.UsuarioAdmin,
            FechaDeteccion = DateTime.UtcNow.AddHours(2),
            Tipo = "FUGA",
            Modulo = "PRI",
            Descripcion = "x"
        };
        await Falla<ArgumentException>(() => Servicio.CrearAsync(dto));

        dto.FechaDeteccion = DateTime.Now.AddHours(-1);
        await Falla<ArgumentException>(() => Servicio.CrearAsync(dto));

        dto.FechaDeteccion = null;
        await Falla<ArgumentException>(() => Servicio.CrearAsync(dto));

        dto.FechaDeteccion = DateTime.UtcNow.AddHours(-1);
        dto.UsuarioId = Semilla.UsuarioInactivo;
        await Falla<ArgumentException>(() => Servicio.CrearAsync(dto));
    }

    [Fact]
    public async Task Sigue_las_transiciones_hasta_cerrar_con_observacion()
    {
        var creado = await RegistrarAsync();

        var analisis = await CambiarAsync(creado, "ANALISIS");
        var contenido = await CambiarAsync(analisis, "CONTENIDO");
        var recuperado = await CambiarAsync(contenido, "RECUPERADO");
        await Falla<ArgumentException>(() => CambiarAsync(recuperado, "CERRADO"));
        var cerrado = await CambiarAsync(recuperado, "CERRADO", "Se restableció el acceso");

        Assert.Equal("CERRADO", cerrado.Estado);
        Assert.NotNull(cerrado.FechaCierre);
        Assert.Equal(5, (await Servicio.ObtenerAccionesAsync(creado.Id))!.Count);
        await Falla<InvalidOperationException>(() => CambiarAsync(cerrado, "ABIERTO"));
        await Falla<InvalidOperationException>(() => Servicio.RegistrarAccionAsync(creado.Id, new AccionIncidenteCrearDto { UsuarioId = Semilla.UsuarioAdmin, Tipo = "NOTA", Accion = "x" }));
        await Falla<InvalidOperationException>(() => Servicio.ActualizarAsync(creado.Id, new IncidenteActualizarDto
        {
            FechaDeteccion = DateTime.UtcNow.AddHours(-1),
            Tipo = "X",
            Modulo = "X",
            Descripcion = "X",
            Version = cerrado.Version
        }));
    }

    [Fact]
    public async Task No_salta_de_abierto_a_recuperado_ni_acepta_estados_desconocidos()
    {
        var creado = await RegistrarAsync();

        await Falla<InvalidOperationException>(() => CambiarAsync(creado, "RECUPERADO"));
        await Falla<ArgumentException>(() => CambiarAsync(creado, "PERDIDO"));
        await Falla<ArgumentException>(() => Servicio.ObtenerTodosAsync("PERDIDO", 1, 10));
    }

    [Fact]
    public async Task Registra_acciones_manuales_pero_no_las_reservadas_al_sistema()
    {
        var creado = await RegistrarAsync();

        var accion = await Servicio.RegistrarAccionAsync(creado.Id, new AccionIncidenteCrearDto
        {
            UsuarioId = Semilla.UsuarioAdmin,
            Tipo = "notificacion_autoridad",
            Accion = "Se notificó a la autoridad",
            Observacion = " dentro de 72 horas "
        });

        Assert.Equal("NOTIFICACION_AUTORIDAD", accion!.Tipo);
        await Falla<ArgumentException>(() => Servicio.RegistrarAccionAsync(creado.Id, new AccionIncidenteCrearDto { UsuarioId = Semilla.UsuarioAdmin, Tipo = "CAMBIO_ESTADO", Accion = "x" }));
        await Falla<ArgumentException>(() => Servicio.RegistrarAccionAsync(creado.Id, new AccionIncidenteCrearDto { UsuarioId = Semilla.UsuarioAdmin, Tipo = " ", Accion = "x" }));
        await Falla<ArgumentException>(() => Servicio.RegistrarAccionAsync(creado.Id, new AccionIncidenteCrearDto { UsuarioId = Semilla.UsuarioAdmin, Tipo = "NOTA", Accion = " " }));
        await Falla<ArgumentException>(() => Servicio.RegistrarAccionAsync(creado.Id, new AccionIncidenteCrearDto { UsuarioId = Semilla.UsuarioInactivo, Tipo = "NOTA", Accion = "x" }));
    }

    [Fact]
    public async Task Actualiza_con_version_vigente_y_rechaza_la_vencida()
    {
        var creado = await RegistrarAsync();
        var dto = new IncidenteActualizarDto
        {
            FechaDeteccion = DateTime.UtcNow.AddHours(-3),
            Tipo = "perdida",
            Modulo = "inv",
            Descripcion = "Descripción corregida",
            Version = creado.Version
        };

        Assert.True(await Servicio.ActualizarAsync(creado.Id, dto));
        Assert.Equal("PERDIDA", (await Servicio.ObtenerPorIdAsync(creado.Id))!.Tipo);

        dto.Version = new byte[8];
        dto.Descripcion = "Cambio con versión vencida";
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => Servicio.ActualizarAsync(creado.Id, dto));
    }

    [Fact]
    public async Task Devuelve_nulo_o_falso_si_no_existe()
    {
        Assert.Null(await Servicio.ObtenerPorIdAsync(Semilla.IdInexistente));
        Assert.Null(await Servicio.ObtenerAccionesAsync(Semilla.IdInexistente));
        Assert.Null(await Servicio.RegistrarAccionAsync(Semilla.IdInexistente, new AccionIncidenteCrearDto { UsuarioId = 1, Tipo = "NOTA", Accion = "x" }));
        Assert.Null(await Servicio.CambiarEstadoAsync(Semilla.IdInexistente, new IncidenteCambiarEstadoDto { Estado = "ANALISIS", UsuarioId = 1, Version = new byte[8] }));
        Assert.False(await Servicio.ActualizarAsync(Semilla.IdInexistente, new IncidenteActualizarDto { FechaDeteccion = DateTime.UtcNow, Tipo = "x", Modulo = "x", Descripcion = "x", Version = new byte[8] }));
    }
}

public class RiesgoServicioPruebas : PruebaServicioBase
{
    public RiesgoServicioPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    private IRiesgoServicio Servicio => Servicio<IRiesgoServicio>();

    [Fact]
    public async Task Calcula_el_nivel_como_probabilidad_por_impacto()
    {
        var tratamiento = await NuevoTratamientoAsync();

        var creado = await Servicio.CrearAsync(new RiesgoCrearDto { TratamientoId = tratamiento.TrId, Descripcion = " Fuga de datos ", Probabilidad = 4, Impacto = 5 });

        Assert.Equal(20, creado.Nivel);
        Assert.Equal("ABIERTO", creado.Estado);
        Assert.Contains(await Servicio.ObtenerTodosAsync(tratamiento.TrId, "abierto", 1, 10), r => r.Id == creado.Id);
    }

    [Fact]
    public async Task El_residual_no_puede_superar_el_nivel_y_el_tratamiento_debe_estar_activo()
    {
        var tratamiento = await NuevoTratamientoAsync();

        await Falla<ArgumentException>(() => Servicio.CrearAsync(new RiesgoCrearDto { TratamientoId = tratamiento.TrId, Descripcion = "x", Probabilidad = 2, Impacto = 2, RiesgoResidual = 5 }));
        await Falla<ArgumentException>(() => Servicio.CrearAsync(new RiesgoCrearDto { TratamientoId = int.MaxValue, Descripcion = "x", Probabilidad = 2, Impacto = 2 }));
    }

    [Fact]
    public async Task Mitigar_exige_medidas_y_residual_y_cerrar_bloquea_cambios()
    {
        var tratamiento = await NuevoTratamientoAsync();
        var creado = await Servicio.CrearAsync(new RiesgoCrearDto { TratamientoId = tratamiento.TrId, Descripcion = "x", Probabilidad = 3, Impacto = 3 });

        await Falla<ArgumentException>(() => Servicio.CambiarEstadoAsync(creado.Id, new RiesgoCambiarEstadoDto { Estado = "MITIGADO" }));
        await Falla<ArgumentException>(() => Servicio.CambiarEstadoAsync(creado.Id, new RiesgoCambiarEstadoDto { Estado = "MITIGADO", MedidasMitigacion = "Cifrado" }));
        var mitigado = await Servicio.CambiarEstadoAsync(creado.Id, new RiesgoCambiarEstadoDto { Estado = "mitigado", MedidasMitigacion = "Cifrado", RiesgoResidual = 3 });
        Assert.Equal("MITIGADO", mitigado!.Estado);

        var abierto = await Servicio.CambiarEstadoAsync(creado.Id, new RiesgoCambiarEstadoDto { Estado = "ABIERTO" });
        Assert.Equal("ABIERTO", abierto!.Estado);
        Assert.True(await Servicio.ActualizarAsync(creado.Id, new RiesgoActualizarDto { Descripcion = "Actualizado", Probabilidad = 1, Impacto = 2 }));
        Assert.Equal(2, (await Servicio.ObtenerPorIdAsync(creado.Id))!.Nivel);

        await Servicio.CambiarEstadoAsync(creado.Id, new RiesgoCambiarEstadoDto { Estado = "CERRADO" });
        await Falla<InvalidOperationException>(() => Servicio.CambiarEstadoAsync(creado.Id, new RiesgoCambiarEstadoDto { Estado = "ABIERTO" }));
        await Falla<InvalidOperationException>(() => Servicio.ActualizarAsync(creado.Id, new RiesgoActualizarDto { Descripcion = "x", Probabilidad = 1, Impacto = 1 }));
    }

    [Fact]
    public async Task Editar_un_riesgo_mitigado_no_puede_quitarle_medidas_ni_residual()
    {
        var tratamiento = await NuevoTratamientoAsync();
        var creado = await Servicio.CrearAsync(new RiesgoCrearDto { TratamientoId = tratamiento.TrId, Descripcion = "x", Probabilidad = 3, Impacto = 3 });
        await Servicio.CambiarEstadoAsync(creado.Id, new RiesgoCambiarEstadoDto { Estado = "MITIGADO", MedidasMitigacion = "Cifrado", RiesgoResidual = 3 });

        await Falla<ArgumentException>(() => Servicio.ActualizarAsync(creado.Id, new RiesgoActualizarDto
        {
            Descripcion = "Editado sin medidas",
            Probabilidad = 3,
            Impacto = 3
        }));
    }

    [Fact]
    public async Task Rechaza_estados_desconocidos_y_devuelve_nulo_si_no_existe()
    {
        await Falla<ArgumentException>(() => Servicio.ObtenerTodosAsync(null, "RARO", 1, 10));
        await Falla<ArgumentException>(() => Servicio.CambiarEstadoAsync(1, new RiesgoCambiarEstadoDto { Estado = "RARO" }));
        Assert.Null(await Servicio.ObtenerPorIdAsync(Semilla.IdInexistente));
        Assert.Null(await Servicio.CambiarEstadoAsync(Semilla.IdInexistente, new RiesgoCambiarEstadoDto { Estado = "CERRADO" }));
        Assert.False(await Servicio.ActualizarAsync(Semilla.IdInexistente, new RiesgoActualizarDto { Descripcion = "x", Probabilidad = 1, Impacto = 1 }));
    }
}

public class EvaluacionImpactoServicioPruebas : PruebaServicioBase
{
    public EvaluacionImpactoServicioPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    private IEvaluacionImpactoServicio Servicio => Servicio<IEvaluacionImpactoServicio>();

    private Task<EvaluacionImpactoRespuestaDto> NuevaAsync(string? conclusion = null, byte? residual = null)
        => Servicio.CrearAsync(new EvaluacionImpactoCrearDto
        {
            UsuarioId = Semilla.UsuarioAdmin,
            Version = " 1.0 ",
            Fecha = new DateOnly(2026, 6, 1),
            Alcance = " Historias clínicas ",
            Conclusion = conclusion,
            NivelResidual = residual
        });

    [Fact]
    public async Task Crea_en_borrador_y_se_edita_solo_en_borrador()
    {
        var creada = await NuevaAsync();

        Assert.Equal("BORRADOR", creada.Estado);
        Assert.True(await Servicio.ActualizarAsync(creada.Id, new EvaluacionImpactoActualizarDto
        {
            Version = "1.1",
            Fecha = new DateOnly(2026, 6, 2),
            Alcance = "Alcance ampliado",
            Conclusion = "Riesgo aceptable",
            NivelResidual = 6
        }));

        var aprobada = await Servicio.CambiarEstadoAsync(creada.Id, new EvaluacionImpactoCambiarEstadoDto { Estado = "aprobada" });

        Assert.Equal("APROBADA", aprobada!.Estado);
        await Falla<InvalidOperationException>(() => Servicio.ActualizarAsync(creada.Id, new EvaluacionImpactoActualizarDto { Version = "2", Fecha = new DateOnly(2026, 1, 1), Alcance = "x" }));
        Assert.Equal("REVISADA", (await Servicio.CambiarEstadoAsync(creada.Id, new EvaluacionImpactoCambiarEstadoDto { Estado = "REVISADA" }))!.Estado);
        Assert.Equal("CERRADA", (await Servicio.CambiarEstadoAsync(creada.Id, new EvaluacionImpactoCambiarEstadoDto { Estado = "CERRADA" }))!.Estado);
        await Falla<InvalidOperationException>(() => Servicio.CambiarEstadoAsync(creada.Id, new EvaluacionImpactoCambiarEstadoDto { Estado = "BORRADOR" }));
    }

    [Fact]
    public async Task Aprobar_exige_conclusion_y_nivel_residual()
    {
        var sinConclusion = await NuevaAsync(residual: 4);
        var sinResidual = await NuevaAsync(conclusion: "Aceptable");

        await Falla<ArgumentException>(() => Servicio.CambiarEstadoAsync(sinConclusion.Id, new EvaluacionImpactoCambiarEstadoDto { Estado = "APROBADA" }));
        await Falla<ArgumentException>(() => Servicio.CambiarEstadoAsync(sinResidual.Id, new EvaluacionImpactoCambiarEstadoDto { Estado = "APROBADA" }));
    }

    [Fact]
    public async Task Valida_usuario_estados_y_existencia()
    {
        await Falla<ArgumentException>(() => Servicio.CrearAsync(new EvaluacionImpactoCrearDto { UsuarioId = Semilla.UsuarioInactivo, Version = "1", Fecha = new DateOnly(2026, 1, 1), Alcance = "x" }));
        await Falla<ArgumentException>(() => Servicio.ObtenerTodosAsync("RARO", 1, 10));
        await Falla<ArgumentException>(() => Servicio.CambiarEstadoAsync(1, new EvaluacionImpactoCambiarEstadoDto { Estado = "RARO" }));
        Assert.NotNull(await Servicio.ObtenerTodosAsync("BORRADOR", 1, 10));
        Assert.Null(await Servicio.ObtenerPorIdAsync(Semilla.IdInexistente));
        Assert.Null(await Servicio.CambiarEstadoAsync(Semilla.IdInexistente, new EvaluacionImpactoCambiarEstadoDto { Estado = "CERRADA" }));
        Assert.False(await Servicio.ActualizarAsync(Semilla.IdInexistente, new EvaluacionImpactoActualizarDto { Version = "1", Fecha = new DateOnly(2026, 1, 1), Alcance = "x" }));
    }

    [Fact]
    public async Task Vincula_riesgos_a_la_evaluacion_y_los_copia_a_una_version_nueva()
    {
        var vinculos = Servicio<IEvaluacionRiesgoServicio>();
        var riesgos = Servicio<IRiesgoServicio>();
        var tratamiento = await NuevoTratamientoAsync();
        var r1 = await riesgos.CrearAsync(new RiesgoCrearDto { TratamientoId = tratamiento.TrId, Descripcion = "R1", Probabilidad = 2, Impacto = 3 });
        var r2 = await riesgos.CrearAsync(new RiesgoCrearDto { TratamientoId = tratamiento.TrId, Descripcion = "R2", Probabilidad = 1, Impacto = 3 });
        var r3 = await riesgos.CrearAsync(new RiesgoCrearDto { TratamientoId = tratamiento.TrId, Descripcion = "R3", Probabilidad = 1, Impacto = 1 });
        var origen = await NuevaAsync();
        var destino = await NuevaAsync();

        foreach (var riesgo in new[] { r1, r2, r3 })
            await vinculos.CrearAsync(new EvaluacionRiesgoCrearDto { EvaluacionId = origen.Id, RiesgoId = riesgo.Id, Observacion = "Incluido" });

        await Falla<InvalidOperationException>(() => vinculos.CrearAsync(new EvaluacionRiesgoCrearDto { EvaluacionId = origen.Id, RiesgoId = r1.Id }));
        await riesgos.CambiarEstadoAsync(r3.Id, new RiesgoCambiarEstadoDto { Estado = "CERRADO" });

        var copiados = await vinculos.CopiarAsync(new EvaluacionRiesgoCopiarDto
        {
            EvaluacionOrigenId = origen.Id,
            EvaluacionDestinoId = destino.Id,
            RiesgosExcluidos = [r2.Id]
        });

        Assert.Equal(r1.Id, Assert.Single(copiados).RiesgoId);
        Assert.NotNull(await vinculos.ObtenerAsync(destino.Id, r1.Id));
        Assert.Equal(3, (await vinculos.ObtenerTodosAsync(origen.Id, null, 1, 10)).Count);
        Assert.Empty(await vinculos.CopiarAsync(new EvaluacionRiesgoCopiarDto { EvaluacionOrigenId = origen.Id, EvaluacionDestinoId = destino.Id, RiesgosExcluidos = [r2.Id] }));
        await Falla<ArgumentException>(() => vinculos.CopiarAsync(new EvaluacionRiesgoCopiarDto { EvaluacionOrigenId = origen.Id, EvaluacionDestinoId = origen.Id, RiesgosExcluidos = [] }));
        await Falla<ArgumentException>(() => vinculos.CopiarAsync(new EvaluacionRiesgoCopiarDto { EvaluacionOrigenId = Semilla.IdInexistente, EvaluacionDestinoId = destino.Id, RiesgosExcluidos = [] }));
        await Falla<InvalidOperationException>(() => vinculos.CrearAsync(new EvaluacionRiesgoCrearDto { EvaluacionId = destino.Id, RiesgoId = r3.Id }));
        await Falla<ArgumentException>(() => vinculos.CrearAsync(new EvaluacionRiesgoCrearDto { EvaluacionId = destino.Id, RiesgoId = Semilla.IdInexistente }));
        await Falla<ArgumentException>(() => vinculos.CrearAsync(new EvaluacionRiesgoCrearDto { EvaluacionId = Semilla.IdInexistente, RiesgoId = r1.Id }));

        await Servicio.CambiarEstadoAsync(destino.Id, new EvaluacionImpactoCambiarEstadoDto { Estado = "CERRADA" });
        await Falla<InvalidOperationException>(() => vinculos.CrearAsync(new EvaluacionRiesgoCrearDto { EvaluacionId = destino.Id, RiesgoId = r2.Id }));
    }
}

public class DocumentoServicioPruebas : PruebaServicioBase
{
    public DocumentoServicioPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    private IDocumentoServicio Servicio => Servicio<IDocumentoServicio>();

    private static DocumentoCrearDto Nuevo(string tipo = "rat", string? version = "1.0")
        => new()
        {
            UsuarioId = Semilla.UsuarioAdmin,
            Tipo = tipo,
            Version = version,
            Nombre = " Registro de actividades " + Identificaciones.Sufijo(),
            RutaArchivo = " documentos/rat.pdf ",
            Huella = " ABC123 "
        };

    [Fact]
    public async Task Registra_el_documento_con_tipo_valido()
    {
        var creado = await Servicio.CrearAsync(Nuevo());

        Assert.Equal("RAT", creado.Tipo);
        Assert.Equal("documentos/rat.pdf", creado.RutaArchivo);
        Assert.Contains(await Servicio.ObtenerTodosAsync("RAT", false, 1, 100), d => d.Id == creado.Id);
    }

    [Fact]
    public async Task Rechaza_tipo_desconocido_usuario_inactivo_y_duplicados()
    {
        await Falla<ArgumentException>(() => Servicio.CrearAsync(Nuevo("MEMO")));
        await Falla<ArgumentException>(() => Servicio.ObtenerTodosAsync("MEMO", false, 1, 10));

        var inactivo = Nuevo();
        inactivo.UsuarioId = Semilla.UsuarioInactivo;
        await Falla<ArgumentException>(() => Servicio.CrearAsync(inactivo));

        var dto = Nuevo("EIPD", null);
        var creado = await Servicio.CrearAsync(dto);
        await Falla<InvalidOperationException>(() => Servicio.CrearAsync(dto));
        await Servicio.EliminarAsync(creado.Id);
        var error = await Falla<InvalidOperationException>(() => Servicio.CrearAsync(dto));
        Assert.Contains("dado de baja", error.Message);
    }

    [Fact]
    public async Task Actualiza_da_de_baja_y_reactiva_sin_duplicar_activos()
    {
        var original = await Servicio.CrearAsync(Nuevo("OTRO"));
        Assert.True(await Servicio.ActualizarAsync(original.Id, new DocumentoActualizarDto { Tipo = "otro", Nombre = original.Nombre, Version = "2.0" }));
        Assert.True(await Servicio.EliminarAsync(original.Id));
        Assert.True(await Servicio.EliminarAsync(original.Id));

        await Falla<InvalidOperationException>(() => Servicio.CrearAsync(new DocumentoCrearDto
        {
            UsuarioId = Semilla.UsuarioAdmin,
            Tipo = "OTRO",
            Version = "2.0",
            Nombre = original.Nombre,
            RutaArchivo = "documentos/copia.pdf"
        }));

        var activo = new PodoFlow.Api.Modelos.Documento
        {
            UsId = Semilla.UsuarioAdmin,
            DoTipo = "OTRO",
            DoVersi = "2.0",
            DoNombre = original.Nombre,
            DoRutaArch = "documentos/copia.pdf",
            DoFechaCarg = DateTime.UtcNow,
            DoEstatus = true
        };
        Contexto.Documentos.Add(activo);
        await Contexto.SaveChangesAsync();

        await Falla<InvalidOperationException>(() => Servicio.ReactivarAsync(original.Id));
        await Servicio.EliminarAsync(activo.DoId);
        Assert.True(await Servicio.ReactivarAsync(original.Id));
        Assert.True(await Servicio.ReactivarAsync(original.Id));
        await Falla<InvalidOperationException>(() => Servicio.ActualizarAsync(activo.DoId, new DocumentoActualizarDto { Tipo = "OTRO", Nombre = original.Nombre, Version = "2.0" }));
    }

    [Fact]
    public async Task Devuelve_nulo_o_falso_si_no_existe()
    {
        Assert.Null(await Servicio.ObtenerPorIdAsync(Semilla.IdInexistente));
        Assert.False(await Servicio.ActualizarAsync(Semilla.IdInexistente, new DocumentoActualizarDto { Tipo = "RAT", Nombre = "x" }));
        Assert.False(await Servicio.EliminarAsync(Semilla.IdInexistente));
        Assert.False(await Servicio.ReactivarAsync(Semilla.IdInexistente));
    }
}
