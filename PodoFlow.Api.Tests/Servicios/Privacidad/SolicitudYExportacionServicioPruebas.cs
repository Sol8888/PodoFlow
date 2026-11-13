using PodoFlow.Api.DTOs.EvidenciaSolicitud;
using PodoFlow.Api.DTOs.Exportacion;
using PodoFlow.Api.DTOs.Solicitud;
using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Tests.Servicios.Privacidad;

public class SolicitudServicioPruebas : PruebaServicioBase
{
    public SolicitudServicioPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    private ISolicitudServicio Servicio => Servicio<ISolicitudServicio>();

    private static SolicitudCrearDto DelTitular(Paciente paciente, string derecho = "ACCESO")
        => new()
        {
            PacienteId = paciente.PaId,
            UsuarioId = Semilla.UsuarioAdmin,
            TipoDerecho = derecho,
            TipoSolicitante = "TITULAR",
            NombreSolicitante = paciente.PaNombres + " " + paciente.PaApelli,
            IdentificacionSolicitante = paciente.PaIdent,
            MedioRespuesta = "CORREO"
        };

    private async Task<SolicitudRespuestaDto> AvanzarAsync(SolicitudRespuestaDto solicitud, string estado, string? respuesta = null)
        => (await Servicio.CambiarEstadoAsync(solicitud.Id, new SolicitudCambiarEstadoDto
        {
            Estado = estado,
            UsuarioId = Semilla.UsuarioAdmin,
            Respuesta = respuesta,
            Version = solicitud.Version
        }))!;

    [Fact]
    public async Task Registra_la_solicitud_del_titular_con_plazo_de_respuesta()
    {
        var paciente = await NuevoPacienteAsync();

        var creada = await Servicio.CrearAsync(DelTitular(paciente, "rectificacion"));

        Assert.Equal("RECIBIDA", creada.Estado);
        Assert.Equal("RECTIFICACION", creada.TipoDerecho);
        Assert.Equal(15, creada.DiasRestantes);
        Assert.False(creada.Vencida);
        Assert.Single((await Servicio.ObtenerAccionesAsync(creada.Id))!);
        Assert.Contains(await Servicio.ObtenerPorPacienteAsync(paciente.PaId), s => s.Id == creada.Id);
        Assert.NotEmpty(await Servicio.ObtenerTodosAsync(1, 10));
    }

    [Fact]
    public async Task La_identificacion_del_titular_debe_coincidir_con_la_del_paciente()
    {
        var paciente = await NuevoPacienteAsync();
        var dto = DelTitular(paciente);
        dto.IdentificacionSolicitante = Identificaciones.CedulaValida();

        var error = await Falla<ArgumentException>(() => Servicio.CrearAsync(dto));

        Assert.Contains("no coincide", error.Message);
    }

    [Fact]
    public async Task Valida_la_cedula_del_representante_y_que_no_sea_la_del_paciente()
    {
        var paciente = await NuevoPacienteAsync();
        var dto = DelTitular(paciente);
        dto.TipoSolicitante = "REPRESENTANTE";
        dto.TipoIdentificacion = "CEDULA";

        dto.IdentificacionSolicitante = Identificaciones.CedulaInvalida(Identificaciones.CedulaValida());
        await Falla<ArgumentException>(() => Servicio.CrearAsync(dto));

        dto.IdentificacionSolicitante = paciente.PaIdent;
        await Falla<ArgumentException>(() => Servicio.CrearAsync(dto));

        dto.TipoIdentificacion = null;
        await Falla<ArgumentException>(() => Servicio.CrearAsync(dto));

        dto.TipoIdentificacion = "PASAPORTE";
        dto.IdentificacionSolicitante = "AB1";
        await Falla<ArgumentException>(() => Servicio.CrearAsync(dto));

        dto.IdentificacionSolicitante = Identificaciones.Pasaporte();
        var creada = await Servicio.CrearAsync(dto);
        Assert.Equal("REPRESENTANTE", creada.TipoSolicitante);
    }

    [Fact]
    public async Task Rechaza_derecho_solicitante_paciente_o_usuario_invalidos()
    {
        var paciente = await NuevoPacienteAsync();

        var derecho = DelTitular(paciente, "OLVIDO");
        await Falla<ArgumentException>(() => Servicio.CrearAsync(derecho));

        var solicitante = DelTitular(paciente);
        solicitante.TipoSolicitante = "VECINO";
        await Falla<ArgumentException>(() => Servicio.CrearAsync(solicitante));

        var inactivo = DelTitular(paciente);
        inactivo.PacienteId = Semilla.PacienteInactivo;
        await Falla<ArgumentException>(() => Servicio.CrearAsync(inactivo));

        var usuario = DelTitular(paciente);
        usuario.UsuarioId = Semilla.UsuarioInactivo;
        await Falla<ArgumentException>(() => Servicio.CrearAsync(usuario));
    }

    [Fact]
    public async Task Sigue_el_flujo_hasta_cerrar_y_bloquea_la_edicion()
    {
        var paciente = await NuevoPacienteAsync();
        var creada = await Servicio.CrearAsync(DelTitular(paciente, "RECTIFICACION"));

        var validando = await AvanzarAsync(creada, "VALIDANDO");
        var proceso = await AvanzarAsync(validando, "EN_PROCESO");
        await Falla<ArgumentException>(() => AvanzarAsync(proceso, "RESPONDIDA"));
        var respondida = await AvanzarAsync(proceso, "RESPONDIDA", "Datos rectificados");
        var cerrada = await AvanzarAsync(respondida, "CERRADA");

        Assert.Equal("CERRADA", cerrada.Estado);
        Assert.NotNull(cerrada.FechaCierre);
        Assert.Null(cerrada.DiasRestantes);
        await Falla<InvalidOperationException>(() => Servicio.ActualizarAsync(creada.Id, new SolicitudActualizarDto
        {
            NombreSolicitante = "Otro",
            IdentificacionSolicitante = paciente.PaIdent,
            Version = cerrada.Version
        }));
        await Falla<InvalidOperationException>(() => AvanzarAsync(cerrada, "EN_PROCESO"));
    }

    [Fact]
    public async Task Rechazar_exige_motivo_y_es_estado_final()
    {
        var paciente = await NuevoPacienteAsync();
        var creada = await Servicio.CrearAsync(DelTitular(paciente));

        await Falla<ArgumentException>(() => AvanzarAsync(creada, "RECHAZADA"));
        var rechazada = await AvanzarAsync(creada, "RECHAZADA", "Identidad no verificada");

        Assert.Equal("RECHAZADA", rechazada.Estado);
        await Falla<ArgumentException>(() => AvanzarAsync(rechazada, "INVENTADO"));
    }

    [Fact]
    public async Task Una_solicitud_de_acceso_no_se_responde_sin_exportacion_entregada()
    {
        var paciente = await NuevoPacienteAsync();
        var creada = await Servicio.CrearAsync(DelTitular(paciente, "ACCESO"));
        var proceso = await AvanzarAsync(await AvanzarAsync(creada, "VALIDANDO"), "EN_PROCESO");

        var error = await Falla<InvalidOperationException>(() => AvanzarAsync(proceso, "RESPONDIDA", "Adjunto sus datos"));

        Assert.Contains("ENTREGADA", error.Message);
    }

    [Fact]
    public async Task Actualiza_los_datos_del_solicitante_y_detecta_version_vencida()
    {
        var paciente = await NuevoPacienteAsync();
        var creada = await Servicio.CrearAsync(DelTitular(paciente));

        Assert.True(await Servicio.ActualizarAsync(creada.Id, new SolicitudActualizarDto
        {
            NombreSolicitante = "Nombre corregido",
            IdentificacionSolicitante = paciente.PaIdent,
            Detalle = "Detalle nuevo",
            Version = creada.Version
        }));

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => Servicio.ActualizarAsync(creada.Id, new SolicitudActualizarDto
        {
            NombreSolicitante = "Otro nombre",
            IdentificacionSolicitante = paciente.PaIdent,
            Version = new byte[8]
        }));
    }

    [Fact]
    public async Task Devuelve_nulo_o_falso_si_no_existe()
    {
        Assert.Null(await Servicio.ObtenerPorIdAsync(Semilla.IdInexistente));
        Assert.Null(await Servicio.ObtenerAccionesAsync(Semilla.IdInexistente));
        Assert.Null(await Servicio.CambiarEstadoAsync(Semilla.IdInexistente, new SolicitudCambiarEstadoDto { Estado = "VALIDANDO", UsuarioId = Semilla.UsuarioAdmin, Version = new byte[8] }));
        Assert.False(await Servicio.ActualizarAsync(Semilla.IdInexistente, new SolicitudActualizarDto { NombreSolicitante = "x", IdentificacionSolicitante = "x", Version = new byte[8] }));
    }
}

public class ExportacionServicioPruebas : PruebaServicioBase
{
    public ExportacionServicioPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    private IExportacionServicio Servicio => Servicio<IExportacionServicio>();

    private ISolicitudServicio Solicitudes => Servicio<ISolicitudServicio>();

    private async Task<(Paciente Paciente, SolicitudRespuestaDto Solicitud)> SolicitudEnProcesoAsync(string derecho = "ACCESO", bool conEvidencia = true)
    {
        var paciente = await NuevoPacienteAsync();
        await NuevoContactoAsync(paciente.PaId, "CORREO", "titular@pruebas.podoflow.test");
        await NuevaCitaAsync(paciente.PaId, DateTime.UtcNow.AddDays(-3), "ATENDIDA");
        await NuevoConsentimientoAsync(paciente.PaId, Semilla.FinalidadRecordatorios);
        var historia = await NuevaHistoriaAsync(paciente.PaId);
        var atencion = await NuevaAtencionAsync(historia.HiId);
        Contexto.Evoluciones.Add(new Evolucion { AtId = atencion.AtId, UsId = Semilla.UsuarioAdmin, EvFecha = DateTime.UtcNow, EvDescri = "Mejoría, \"sin\" dolor, =control", EvEstatus = true });
        await Contexto.SaveChangesAsync();

        var solicitud = await Solicitudes.CrearAsync(new SolicitudCrearDto
        {
            PacienteId = paciente.PaId,
            UsuarioId = Semilla.UsuarioAdmin,
            TipoDerecho = derecho,
            TipoSolicitante = "TITULAR",
            NombreSolicitante = "Titular de pruebas",
            IdentificacionSolicitante = paciente.PaIdent,
            MedioRespuesta = "CORREO"
        });

        foreach (var estado in new[] { "VALIDANDO", "EN_PROCESO" })
        {
            solicitud = (await Solicitudes.CambiarEstadoAsync(solicitud.Id, new SolicitudCambiarEstadoDto
            {
                Estado = estado,
                UsuarioId = Semilla.UsuarioAdmin,
                Version = solicitud.Version
            }))!;
        }

        if (conEvidencia)
        {
            await Servicio<IEvidenciaSolicitudServicio>().CrearAsync(new EvidenciaSolicitudCrearDto
            {
                SolicitudId = solicitud.Id,
                UsuarioId = Semilla.UsuarioAdmin,
                Tipo = "CEDULA_VERIFICADA",
                RutaArchivo = "evidencias/cedula.pdf"
            });
        }

        return (paciente, solicitud);
    }

    private Task<ExportacionRespuestaDto> ExportarAsync(long solicitudId, string formato, List<string>? secciones = null)
        => Servicio.CrearAsync(new ExportacionCrearDto
        {
            SolicitudId = solicitudId,
            UsuarioId = Semilla.UsuarioAdmin,
            Formato = formato,
            Secciones = secciones,
            Observacion = "Exportación de pruebas"
        });

    [Theory]
    [InlineData("json", "application/json")]
    [InlineData("CSV", "text/csv")]
    [InlineData("pdf", "application/pdf")]
    public async Task Genera_la_exportacion_y_la_descarga_verificando_su_huella(string formato, string tipo)
    {
        var (_, solicitud) = await SolicitudEnProcesoAsync();

        var exportacion = await ExportarAsync(solicitud.Id, formato);
        var descarga = await Servicio.DescargarAsync(exportacion.Id, Semilla.UsuarioAdmin);

        Assert.Equal("GENERADA", exportacion.Estado);
        Assert.Equal(64, exportacion.Huella!.Length);
        Assert.Equal(tipo, descarga!.Value.TipoContenido);
        Assert.NotEmpty(descarga.Value.Contenido);
        Assert.Contains(await Servicio.ObtenerPorSolicitudAsync(solicitud.Id) ?? [], e => e.Id == exportacion.Id);
        Assert.NotNull(await Servicio.ObtenerPorIdAsync(exportacion.Id));
        Assert.NotEmpty(await Servicio.ObtenerTodosAsync(1, 10));
    }

    [Fact]
    public async Task El_csv_neutraliza_formulas_y_escapa_comillas()
    {
        var (_, solicitud) = await SolicitudEnProcesoAsync();

        var exportacion = await ExportarAsync(solicitud.Id, "CSV", ["clinica"]);
        var texto = System.Text.Encoding.UTF8.GetString((await Servicio.DescargarAsync(exportacion.Id, Semilla.UsuarioAdmin))!.Value.Contenido);

        Assert.StartsWith("﻿seccion,registro,campo,valor", texto);
        Assert.Contains("\"Mejoría, \"\"sin\"\" dolor, =control\"", texto);
        Assert.Contains("CLINICA", texto);
    }

    [Fact]
    public async Task Entregar_la_exportacion_permite_responder_la_solicitud_de_acceso()
    {
        var (_, solicitud) = await SolicitudEnProcesoAsync();
        var exportacion = await ExportarAsync(solicitud.Id, "JSON", ["personales", "citas"]);

        var entregada = await Servicio.CambiarEstadoAsync(exportacion.Id, new ExportacionCambiarEstadoDto { Estado = "entregada", UsuarioId = Semilla.UsuarioAdmin });
        var respondida = await Solicitudes.CambiarEstadoAsync(solicitud.Id, new SolicitudCambiarEstadoDto
        {
            Estado = "RESPONDIDA",
            UsuarioId = Semilla.UsuarioAdmin,
            Respuesta = "Se entregaron sus datos",
            Version = solicitud.Version
        });

        Assert.Equal("ENTREGADA", entregada!.Estado);
        Assert.NotNull(entregada.FechaEntrega);
        Assert.Equal("RESPONDIDA", respondida!.Estado);
        await Falla<InvalidOperationException>(() => Servicio.CambiarEstadoAsync(exportacion.Id, new ExportacionCambiarEstadoDto { Estado = "ANULADA", UsuarioId = Semilla.UsuarioAdmin }));
    }

    [Fact]
    public async Task Detecta_un_archivo_alterado_y_no_entrega_una_anulada()
    {
        var (_, solicitud) = await SolicitudEnProcesoAsync();
        var exportacion = await ExportarAsync(solicitud.Id, "JSON");
        var ruta = Path.Combine(Fabrica.CarpetaExportaciones, exportacion.RutaArchivo);
        await File.AppendAllTextAsync(ruta, " ");

        var alterado = await Falla<InvalidOperationException>(() => Servicio.DescargarAsync(exportacion.Id, Semilla.UsuarioAdmin));
        Assert.Contains("huella", alterado.Message);

        await Servicio.CambiarEstadoAsync(exportacion.Id, new ExportacionCambiarEstadoDto { Estado = "ANULADA", UsuarioId = Semilla.UsuarioAdmin });
        await Falla<InvalidOperationException>(() => Servicio.DescargarAsync(exportacion.Id, Semilla.UsuarioAdmin));
    }

    [Fact]
    public async Task Exige_evidencia_derecho_estado_y_formato_validos()
    {
        var (_, sinEvidencia) = await SolicitudEnProcesoAsync(conEvidencia: false);
        await Falla<InvalidOperationException>(() => ExportarAsync(sinEvidencia.Id, "JSON"));

        var (_, portabilidad) = await SolicitudEnProcesoAsync("PORTABILIDAD");
        await Falla<ArgumentException>(() => ExportarAsync(portabilidad.Id, "PDF"));
        await Falla<ArgumentException>(() => ExportarAsync(portabilidad.Id, "XML"));
        await Falla<ArgumentException>(() => ExportarAsync(portabilidad.Id, "JSON", ["FOTOS"]));

        var paciente = await NuevoPacienteAsync();
        var rectificacion = await Solicitudes.CrearAsync(new SolicitudCrearDto
        {
            PacienteId = paciente.PaId,
            TipoDerecho = "RECTIFICACION",
            TipoSolicitante = "TITULAR",
            NombreSolicitante = "Titular",
            IdentificacionSolicitante = paciente.PaIdent
        });
        await Falla<InvalidOperationException>(() => ExportarAsync(rectificacion.Id, "JSON"));
        await Falla<ArgumentException>(() => ExportarAsync(Semilla.IdInexistente, "JSON"));
    }

    [Fact]
    public async Task Devuelve_nulo_si_no_existe()
    {
        Assert.Null(await Servicio.ObtenerPorIdAsync(Semilla.IdInexistente));
        Assert.Null(await Servicio.ObtenerPorSolicitudAsync(Semilla.IdInexistente));
        Assert.Null(await Servicio.DescargarAsync(Semilla.IdInexistente, Semilla.UsuarioAdmin));
        Assert.Null(await Servicio.CambiarEstadoAsync(Semilla.IdInexistente, new ExportacionCambiarEstadoDto { Estado = "ANULADA", UsuarioId = Semilla.UsuarioAdmin }));
        await Falla<ArgumentException>(() => Servicio.CambiarEstadoAsync(1, new ExportacionCambiarEstadoDto { Estado = "PERDIDA", UsuarioId = Semilla.UsuarioAdmin }));
    }
}

public class EvidenciaSolicitudServicioPruebas : PruebaServicioBase
{
    public EvidenciaSolicitudServicioPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    [Fact]
    public async Task Registra_y_consulta_evidencias_y_rechaza_las_de_solicitudes_cerradas()
    {
        var servicio = Servicio<IEvidenciaSolicitudServicio>();
        var paciente = await NuevoPacienteAsync();
        var solicitud = await Servicio<ISolicitudServicio>().CrearAsync(new SolicitudCrearDto
        {
            PacienteId = paciente.PaId,
            TipoDerecho = "CONSULTA",
            TipoSolicitante = "TITULAR",
            NombreSolicitante = "Titular",
            IdentificacionSolicitante = paciente.PaIdent
        });

        var evidencia = await servicio.CrearAsync(new EvidenciaSolicitudCrearDto
        {
            SolicitudId = solicitud.Id,
            UsuarioId = Semilla.UsuarioAdmin,
            Tipo = "CEDULA",
            RutaArchivo = "evidencias/c.pdf"
        });

        Assert.NotNull(await servicio.ObtenerPorIdAsync(evidencia.Id));
        Assert.Single((await servicio.ObtenerPorSolicitudAsync(solicitud.Id))!);
        Assert.Null(await servicio.ObtenerPorSolicitudAsync(Semilla.IdInexistente));
        Assert.Null(await servicio.ObtenerPorIdAsync(Semilla.IdInexistente));
        await Falla<ArgumentException>(() => servicio.CrearAsync(new EvidenciaSolicitudCrearDto { SolicitudId = Semilla.IdInexistente, UsuarioId = Semilla.UsuarioAdmin, Tipo = "X", RutaArchivo = "x" }));
        await Falla<ArgumentException>(() => servicio.CrearAsync(new EvidenciaSolicitudCrearDto { SolicitudId = solicitud.Id, UsuarioId = Semilla.UsuarioInactivo, Tipo = "X", RutaArchivo = "x" }));

        await Servicio<ISolicitudServicio>().CambiarEstadoAsync(solicitud.Id, new SolicitudCambiarEstadoDto
        {
            Estado = "RECHAZADA",
            UsuarioId = Semilla.UsuarioAdmin,
            Respuesta = "No procede",
            Version = solicitud.Version
        });
        await Falla<InvalidOperationException>(() => servicio.CrearAsync(new EvidenciaSolicitudCrearDto { SolicitudId = solicitud.Id, UsuarioId = Semilla.UsuarioAdmin, Tipo = "X", RutaArchivo = "x" }));
    }
}
