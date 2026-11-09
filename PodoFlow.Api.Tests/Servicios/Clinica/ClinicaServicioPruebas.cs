using Microsoft.AspNetCore.Http;
using PodoFlow.Api.DTOs.Adjunto;
using PodoFlow.Api.DTOs.Antecedente;
using PodoFlow.Api.DTOs.Atencion;
using PodoFlow.Api.DTOs.Evolucion;
using PodoFlow.Api.DTOs.Historia;
using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Tests.Servicios.Clinica;

public class HistoriaServicioPruebas : PruebaServicioBase
{
    public HistoriaServicioPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    private IHistoriaServicio Servicio => Servicio<IHistoriaServicio>();

    [Fact]
    public async Task Abre_una_sola_historia_por_paciente()
    {
        var paciente = await NuevoPacienteAsync();

        var creada = await Servicio.CrearAsync(new HistoriaCrearDto { PacienteId = paciente.PaId, UsuarioId = Semilla.UsuarioAdmin });

        Assert.Equal("ACTIVA", creada.Estado);
        Assert.Equal(creada.Id, (await Servicio.ObtenerPorPacienteAsync(paciente.PaId))!.Id);
        await Falla<InvalidOperationException>(() => Servicio.CrearAsync(new HistoriaCrearDto { PacienteId = paciente.PaId }));
        await Falla<ArgumentException>(() => Servicio.CrearAsync(new HistoriaCrearDto { PacienteId = Semilla.PacienteInactivo }));
        var otro = await NuevoPacienteAsync();
        await Falla<ArgumentException>(() => Servicio.CrearAsync(new HistoriaCrearDto { PacienteId = otro.PaId, UsuarioId = Semilla.UsuarioInactivo }));
        Assert.Contains(await Servicio.ObtenerTodosAsync("activa", 1, 100), h => h.Id == creada.Id);
    }

    [Fact]
    public async Task Bloquea_reactiva_y_cierra_con_motivo()
    {
        var paciente = await NuevoPacienteAsync();
        var creada = await Servicio.CrearAsync(new HistoriaCrearDto { PacienteId = paciente.PaId });

        await Falla<ArgumentException>(() => Servicio.CambiarEstadoAsync(creada.Id, new HistoriaCambiarEstadoDto { NuevoEstado = "BLOQUEADA", Version = creada.Version }));
        Assert.True(await Servicio.CambiarEstadoAsync(creada.Id, new HistoriaCambiarEstadoDto { NuevoEstado = "bloqueada", Motivo = "Auditoría", Version = creada.Version }));
        var bloqueada = await Servicio.ObtenerPorIdAsync(creada.Id);
        Assert.Equal("Auditoría", bloqueada!.MotivoBloqueo);
        await Falla<InvalidOperationException>(() => Servicio.ActualizarAsync(creada.Id, new HistoriaActualizarDto { UsuarioId = Semilla.UsuarioAdmin, Version = bloqueada.Version }));

        Assert.True(await Servicio.CambiarEstadoAsync(creada.Id, new HistoriaCambiarEstadoDto { NuevoEstado = "ACTIVA", Version = bloqueada.Version }));
        var activa = await Servicio.ObtenerPorIdAsync(creada.Id);
        Assert.Null(activa!.MotivoBloqueo);
        Assert.True(await Servicio.ActualizarAsync(creada.Id, new HistoriaActualizarDto { UsuarioId = Semilla.UsuarioAdmin, Version = activa.Version }));

        var actual = await Servicio.ObtenerPorIdAsync(creada.Id);
        Assert.True(await Servicio.CambiarEstadoAsync(creada.Id, new HistoriaCambiarEstadoDto { NuevoEstado = "CERRADA", Motivo = "Alta", Version = actual!.Version }));
        var cerrada = await Servicio.ObtenerPorIdAsync(creada.Id);
        await Falla<InvalidOperationException>(() => Servicio.CambiarEstadoAsync(creada.Id, new HistoriaCambiarEstadoDto { NuevoEstado = "ACTIVA", Version = cerrada!.Version }));
    }

    [Fact]
    public async Task Valida_estado_version_y_paciente_activo()
    {
        var paciente = await NuevoPacienteAsync();
        var creada = await Servicio.CrearAsync(new HistoriaCrearDto { PacienteId = paciente.PaId });

        await Falla<ArgumentException>(() => Servicio.CambiarEstadoAsync(creada.Id, new HistoriaCambiarEstadoDto { NuevoEstado = "PERDIDA", Version = creada.Version }));
        await Falla<ArgumentException>(() => Servicio.CambiarEstadoAsync(creada.Id, new HistoriaCambiarEstadoDto { NuevoEstado = "CERRADA", Motivo = "x", Version = "xx" }));
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => Servicio.CambiarEstadoAsync(creada.Id, new HistoriaCambiarEstadoDto { NuevoEstado = "CERRADA", Motivo = "x", Version = Convert.ToBase64String(new byte[8]) }));
        await Falla<ArgumentException>(() => Servicio.ObtenerTodosAsync("RARA", 1, 10));
        await Falla<ArgumentException>(() => Servicio.ObtenerTodosAsync(null, 0, 10));
        Assert.Null(await Servicio.ObtenerPorIdAsync(Semilla.IdInexistente));
        Assert.Null(await Servicio.ObtenerPorPacienteAsync(Semilla.IdInexistente));
        Assert.False(await Servicio.ActualizarAsync(Semilla.IdInexistente, new HistoriaActualizarDto { Version = Convert.ToBase64String(new byte[8]) }));
        Assert.False(await Servicio.CambiarEstadoAsync(Semilla.IdInexistente, new HistoriaCambiarEstadoDto { NuevoEstado = "CERRADA", Motivo = "x", Version = Convert.ToBase64String(new byte[8]) }));
    }
}

public class AntecedenteServicioPruebas : PruebaServicioBase
{
    public AntecedenteServicioPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    [Fact]
    public async Task Registra_antecedentes_sin_duplicados_en_historias_activas()
    {
        var servicio = Servicio<IAntecedenteServicio>();
        var historia = await NuevaHistoriaAsync((await NuevoPacienteAsync()).PaId);
        var dto = new AntecedenteCrearDto { HistoriaId = historia.HiId, UsuarioId = Semilla.UsuarioAdmin, Tipo = " ALERGIA ", Descripcion = " Penicilina " };

        var creado = await servicio.CrearAsync(dto);

        Assert.Equal("Penicilina", creado.Descripcion);
        await Falla<InvalidOperationException>(() => servicio.CrearAsync(dto));
        var otro = await servicio.CrearAsync(new AntecedenteCrearDto { HistoriaId = historia.HiId, Tipo = "CIRUGIA", Descripcion = "Juanete" });
        await Falla<InvalidOperationException>(() => servicio.ActualizarAsync(otro.Id, new AntecedenteActualizarDto { Tipo = "ALERGIA", Descripcion = "Penicilina" }));
        Assert.True(await servicio.ActualizarAsync(otro.Id, new AntecedenteActualizarDto { Tipo = "CIRUGIA", Descripcion = "Juanete pie derecho" }));
        Assert.True(await servicio.EliminarAsync(creado.Id));
        Assert.True(await servicio.EliminarAsync(creado.Id));
        await Falla<InvalidOperationException>(() => servicio.ActualizarAsync(creado.Id, new AntecedenteActualizarDto { Tipo = "ALERGIA", Descripcion = "x" }));
        Assert.Equal(creado.Id, (await servicio.CrearAsync(dto)).Id);
        await servicio.EliminarAsync(creado.Id);
        Assert.True(await servicio.ReactivarAsync(creado.Id));
        Assert.True(await servicio.ReactivarAsync(creado.Id));
        Assert.Equal(2, (await servicio.ObtenerTodosAsync(historia.HiId, false, 1, 10)).Count);
        Assert.NotNull(await servicio.ObtenerPorIdAsync(creado.Id));
    }

    [Fact]
    public async Task Rechaza_datos_invalidos_e_historias_bloqueadas()
    {
        var servicio = Servicio<IAntecedenteServicio>();
        var bloqueada = await NuevaHistoriaAsync((await NuevoPacienteAsync()).PaId, "BLOQUEADA");
        var activa = await NuevaHistoriaAsync((await NuevoPacienteAsync()).PaId);

        await Falla<InvalidOperationException>(() => servicio.CrearAsync(new AntecedenteCrearDto { HistoriaId = bloqueada.HiId, Tipo = "X", Descripcion = "Y" }));
        await Falla<ArgumentException>(() => servicio.CrearAsync(new AntecedenteCrearDto { HistoriaId = Semilla.IdInexistente, Tipo = "X", Descripcion = "Y" }));
        await Falla<ArgumentException>(() => servicio.CrearAsync(new AntecedenteCrearDto { HistoriaId = activa.HiId, Tipo = " ", Descripcion = "Y" }));
        await Falla<ArgumentException>(() => servicio.CrearAsync(new AntecedenteCrearDto { HistoriaId = activa.HiId, Tipo = "X", Descripcion = " " }));
        await Falla<ArgumentException>(() => servicio.CrearAsync(new AntecedenteCrearDto { HistoriaId = activa.HiId, UsuarioId = Semilla.UsuarioInactivo, Tipo = "X", Descripcion = "Y" }));
        await Falla<ArgumentException>(() => servicio.ObtenerTodosAsync(null, false, 0, 10));
        await Falla<ArgumentException>(() => servicio.ObtenerTodosAsync(-1, false, 1, 10));
        Assert.Null(await servicio.ObtenerPorIdAsync(Semilla.IdInexistente));
        Assert.False(await servicio.ActualizarAsync(Semilla.IdInexistente, new AntecedenteActualizarDto { Tipo = "X", Descripcion = "Y" }));
        Assert.False(await servicio.EliminarAsync(Semilla.IdInexistente));
        Assert.False(await servicio.ReactivarAsync(Semilla.IdInexistente));
    }
}

public class AtencionYEvolucionServicioPruebas : PruebaServicioBase
{
    public AtencionYEvolucionServicioPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    private IAtencionServicio Atenciones => Servicio<IAtencionServicio>();

    private IEvolucionServicio Evoluciones => Servicio<IEvolucionServicio>();

    [Fact]
    public async Task Registra_la_atencion_vinculada_a_una_cita_atendida()
    {
        var paciente = await NuevoPacienteAsync();
        var historia = await NuevaHistoriaAsync(paciente.PaId);
        var cita = await NuevaCitaAsync(paciente.PaId, DateTime.UtcNow.AddDays(-1), "ATENDIDA");

        var creada = await Atenciones.CrearAsync(new AtencionCrearDto
        {
            HistoriaId = historia.HiId,
            CitaId = cita.CiId,
            UsuarioId = Semilla.UsuarioAdmin,
            MotivoConsulta = " Dolor en talón ",
            Diagnostico = " Fascitis plantar "
        });

        Assert.Equal("ABIERTA", creada.Estado);
        Assert.Equal("Fascitis plantar", creada.Diagnostico);
        await Falla<InvalidOperationException>(() => Atenciones.CrearAsync(new AtencionCrearDto { HistoriaId = historia.HiId, CitaId = cita.CiId, UsuarioId = Semilla.UsuarioAdmin }));
        Assert.Single(await Atenciones.ObtenerTodosAsync(historia.HiId, "abierta", 1, 10));
    }

    [Fact]
    public async Task Rechaza_citas_no_atendidas_de_otro_paciente_o_inexistentes()
    {
        var paciente = await NuevoPacienteAsync();
        var historia = await NuevaHistoriaAsync(paciente.PaId);
        var pendiente = await NuevaCitaAsync(paciente.PaId, DateTime.UtcNow.AddDays(3));
        var ajena = await NuevaCitaAsync((await NuevoPacienteAsync()).PaId, DateTime.UtcNow.AddDays(-1), "ATENDIDA");

        await Falla<InvalidOperationException>(() => Atenciones.CrearAsync(new AtencionCrearDto { HistoriaId = historia.HiId, CitaId = pendiente.CiId, UsuarioId = Semilla.UsuarioAdmin }));
        await Falla<InvalidOperationException>(() => Atenciones.CrearAsync(new AtencionCrearDto { HistoriaId = historia.HiId, CitaId = ajena.CiId, UsuarioId = Semilla.UsuarioAdmin }));
        await Falla<ArgumentException>(() => Atenciones.CrearAsync(new AtencionCrearDto { HistoriaId = historia.HiId, CitaId = Semilla.IdInexistente, UsuarioId = Semilla.UsuarioAdmin }));
        await Falla<ArgumentException>(() => Atenciones.CrearAsync(new AtencionCrearDto { HistoriaId = historia.HiId, UsuarioId = Semilla.UsuarioInactivo }));
        await Falla<ArgumentException>(() => Atenciones.CrearAsync(new AtencionCrearDto { HistoriaId = 0, UsuarioId = Semilla.UsuarioAdmin }));
        await Falla<ArgumentException>(() => Atenciones.CrearAsync(new AtencionCrearDto { HistoriaId = historia.HiId, CitaId = -1, UsuarioId = Semilla.UsuarioAdmin }));
        await Falla<ArgumentException>(() => Atenciones.CrearAsync(new AtencionCrearDto { HistoriaId = historia.HiId, UsuarioId = Semilla.UsuarioAdmin, Diagnostico = new string('x', 2001) }));
        await Falla<ArgumentException>(() => Atenciones.CrearAsync(new AtencionCrearDto { HistoriaId = Semilla.IdInexistente, UsuarioId = Semilla.UsuarioAdmin }));
    }

    [Fact]
    public async Task Edita_y_cierra_la_atencion_y_luego_la_protege()
    {
        var historia = await NuevaHistoriaAsync((await NuevoPacienteAsync()).PaId);
        var creada = await Atenciones.CrearAsync(new AtencionCrearDto { HistoriaId = historia.HiId, UsuarioId = Semilla.UsuarioAdmin });

        Assert.True(await Atenciones.ActualizarAsync(creada.Id, new AtencionActualizarDto { Tratamiento = "Plantillas", Version = creada.Version }));
        var editada = await Atenciones.ObtenerPorIdAsync(creada.Id);
        await Falla<ArgumentException>(() => Atenciones.CambiarEstadoAsync(creada.Id, new AtencionCambiarEstadoDto { NuevoEstado = "ABIERTA", Motivo = "x", Version = editada!.Version }));
        await Falla<ArgumentException>(() => Atenciones.CambiarEstadoAsync(creada.Id, new AtencionCambiarEstadoDto { NuevoEstado = "CERRADA", Motivo = " ", Version = editada!.Version }));
        Assert.True(await Atenciones.CambiarEstadoAsync(creada.Id, new AtencionCambiarEstadoDto { NuevoEstado = "cerrada", Motivo = "Fin de consulta", Version = editada!.Version }));

        var cerrada = await Atenciones.ObtenerPorIdAsync(creada.Id);
        await Falla<InvalidOperationException>(() => Atenciones.ActualizarAsync(creada.Id, new AtencionActualizarDto { Version = cerrada!.Version }));
        await Falla<InvalidOperationException>(() => Atenciones.CambiarEstadoAsync(creada.Id, new AtencionCambiarEstadoDto { NuevoEstado = "BLOQUEADA", Motivo = "x", Version = cerrada!.Version }));
        await Falla<InvalidOperationException>(() => Evoluciones.CrearAsync(new EvolucionCrearDto { AtencionId = creada.Id, UsuarioId = Semilla.UsuarioAdmin, Descripcion = "x" }));
        await Falla<ArgumentException>(() => Atenciones.ActualizarAsync(creada.Id, new AtencionActualizarDto { Version = "malo" }));
        await Falla<ArgumentException>(() => Atenciones.ObtenerTodosAsync(null, "RARA", 1, 10));
        await Falla<ArgumentException>(() => Atenciones.ObtenerTodosAsync(-1, null, 1, 10));
        await Falla<ArgumentException>(() => Atenciones.ObtenerTodosAsync(null, null, 1, 101));
        Assert.Null(await Atenciones.ObtenerPorIdAsync(Semilla.IdInexistente));
        Assert.False(await Atenciones.ActualizarAsync(Semilla.IdInexistente, new AtencionActualizarDto { Version = Convert.ToBase64String(new byte[8]) }));
        Assert.False(await Atenciones.CambiarEstadoAsync(Semilla.IdInexistente, new AtencionCambiarEstadoDto { NuevoEstado = "CERRADA", Motivo = "x", Version = Convert.ToBase64String(new byte[8]) }));
    }

    [Fact]
    public async Task Registra_evoluciones_sin_duplicados_mientras_la_atencion_esta_abierta()
    {
        var historia = await NuevaHistoriaAsync((await NuevoPacienteAsync()).PaId);
        var atencion = await NuevaAtencionAsync(historia.HiId);
        var dto = new EvolucionCrearDto { AtencionId = atencion.AtId, UsuarioId = Semilla.UsuarioAdmin, Descripcion = " Mejora del dolor " };

        var creada = await Evoluciones.CrearAsync(dto);
        var otra = await Evoluciones.CrearAsync(new EvolucionCrearDto { AtencionId = atencion.AtId, UsuarioId = Semilla.UsuarioAdmin, Descripcion = "Control" });

        Assert.Equal("Mejora del dolor", creada.Descripcion);
        await Falla<InvalidOperationException>(() => Evoluciones.CrearAsync(dto));
        await Falla<InvalidOperationException>(() => Evoluciones.ActualizarAsync(otra.Id, new EvolucionActualizarDto { Descripcion = "Mejora del dolor" }));
        Assert.True(await Evoluciones.ActualizarAsync(otra.Id, new EvolucionActualizarDto { Descripcion = "Control semanal" }));
        Assert.True(await Evoluciones.EliminarAsync(creada.Id));
        Assert.True(await Evoluciones.EliminarAsync(creada.Id));
        await Falla<InvalidOperationException>(() => Evoluciones.ActualizarAsync(creada.Id, new EvolucionActualizarDto { Descripcion = "x" }));
        Assert.Equal(creada.Id, (await Evoluciones.CrearAsync(dto)).Id);
        await Evoluciones.EliminarAsync(creada.Id);
        Assert.True(await Evoluciones.ReactivarAsync(creada.Id));
        Assert.True(await Evoluciones.ReactivarAsync(creada.Id));
        Assert.Equal(2, (await Evoluciones.ObtenerTodosAsync(atencion.AtId, false, 1, 10)).Count);
        Assert.NotNull(await Evoluciones.ObtenerPorIdAsync(creada.Id));
    }

    [Fact]
    public async Task Valida_los_datos_de_la_evolucion()
    {
        var historia = await NuevaHistoriaAsync((await NuevoPacienteAsync()).PaId);
        var atencion = await NuevaAtencionAsync(historia.HiId);

        await Falla<ArgumentException>(() => Evoluciones.CrearAsync(new EvolucionCrearDto { AtencionId = atencion.AtId, UsuarioId = Semilla.UsuarioAdmin, Descripcion = " " }));
        await Falla<ArgumentException>(() => Evoluciones.CrearAsync(new EvolucionCrearDto { AtencionId = atencion.AtId, UsuarioId = 0, Descripcion = "x" }));
        await Falla<ArgumentException>(() => Evoluciones.CrearAsync(new EvolucionCrearDto { AtencionId = atencion.AtId, UsuarioId = Semilla.UsuarioInactivo, Descripcion = "x" }));
        await Falla<ArgumentException>(() => Evoluciones.CrearAsync(new EvolucionCrearDto { AtencionId = Semilla.IdInexistente, UsuarioId = Semilla.UsuarioAdmin, Descripcion = "x" }));
        await Falla<ArgumentException>(() => Evoluciones.ObtenerTodosAsync(null, false, 0, 10));
        await Falla<ArgumentException>(() => Evoluciones.ObtenerTodosAsync(-1, false, 1, 10));
        Assert.Null(await Evoluciones.ObtenerPorIdAsync(Semilla.IdInexistente));
        Assert.False(await Evoluciones.ActualizarAsync(Semilla.IdInexistente, new EvolucionActualizarDto { Descripcion = "x" }));
        Assert.False(await Evoluciones.EliminarAsync(Semilla.IdInexistente));
        Assert.False(await Evoluciones.ReactivarAsync(Semilla.IdInexistente));
    }
}

public class AdjuntoServicioPruebas : PruebaServicioBase
{
    private static readonly byte[] Png = [137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82];

    public AdjuntoServicioPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    private IAdjuntoServicio Servicio => Servicio<IAdjuntoServicio>();

    private static FormFile Archivo(byte[] contenido, string nombre, string tipo)
        => new(new MemoryStream(contenido), 0, contenido.Length, "Archivo", nombre)
        {
            Headers = new HeaderDictionary(),
            ContentType = tipo
        };

    private async Task<Atencion> AtencionAbiertaAsync()
        => await NuevaAtencionAsync((await NuevaHistoriaAsync((await NuevoPacienteAsync()).PaId)).HiId);

    [Fact]
    public async Task Sube_el_archivo_a_la_carpeta_temporal_y_lo_descarga_verificado()
    {
        var atencion = await AtencionAbiertaAsync();

        var subido = await Servicio.SubirAsync(atencion.AtId, new AdjuntoSubirDto { UsuarioId = Semilla.UsuarioAdmin, Archivo = Archivo(Png, "radiografia.png", "image/png") }, CancellationToken.None);

        Assert.StartsWith("adjuntos/", subido.RutaArchivo);
        Assert.Equal(Png.Length, subido.TamanoBytes);
        Assert.True(File.Exists(Path.Combine(Fabrica.CarpetaAdjuntos, subido.RutaArchivo["adjuntos/".Length..])));

        var archivo = await Servicio.AbrirArchivoAsync(subido.Id, CancellationToken.None);
        await using (archivo!.Contenido)
        {
            Assert.Equal("image/png", archivo.TipoMime);
            Assert.Equal("radiografia.png", archivo.Nombre);
        }
    }

    [Fact]
    public async Task Detecta_un_archivo_alterado_en_disco()
    {
        var atencion = await AtencionAbiertaAsync();
        var subido = await Servicio.SubirAsync(atencion.AtId, new AdjuntoSubirDto { UsuarioId = Semilla.UsuarioAdmin, Nombre = "informe", Archivo = Archivo("%PDF-1.4 prueba"u8.ToArray(), "informe.pdf", "application/pdf") }, CancellationToken.None);
        var ruta = Path.Combine(Fabrica.CarpetaAdjuntos, subido.RutaArchivo["adjuntos/".Length..]);
        await File.WriteAllTextAsync(ruta, "%PDF-1.4 alterado");

        await Assert.ThrowsAsync<InvalidDataException>(() => Servicio.AbrirArchivoAsync(subido.Id, CancellationToken.None));
    }

    [Fact]
    public async Task Rechaza_archivos_con_extension_tipo_o_firma_incorrectos()
    {
        var atencion = await AtencionAbiertaAsync();

        await Falla<ArgumentException>(() => Servicio.SubirAsync(atencion.AtId, new AdjuntoSubirDto { UsuarioId = Semilla.UsuarioAdmin, Archivo = Archivo(Png, "foto.gif", "image/gif") }, CancellationToken.None));
        await Falla<ArgumentException>(() => Servicio.SubirAsync(atencion.AtId, new AdjuntoSubirDto { UsuarioId = Semilla.UsuarioAdmin, Archivo = Archivo(Png, "foto.png", "image/jpeg") }, CancellationToken.None));
        await Falla<ArgumentException>(() => Servicio.SubirAsync(atencion.AtId, new AdjuntoSubirDto { UsuarioId = Semilla.UsuarioAdmin, Archivo = Archivo("hola"u8.ToArray(), "foto.jpg", "image/jpeg") }, CancellationToken.None));
        await Falla<ArgumentException>(() => Servicio.SubirAsync(atencion.AtId, new AdjuntoSubirDto { UsuarioId = Semilla.UsuarioAdmin, Archivo = null }, CancellationToken.None));
        await Falla<ArgumentException>(() => Servicio.SubirAsync(atencion.AtId, new AdjuntoSubirDto { UsuarioId = Semilla.UsuarioInactivo, Archivo = Archivo(Png, "a.png", "image/png") }, CancellationToken.None));
        await Falla<ArgumentException>(() => Servicio.SubirAsync(0, new AdjuntoSubirDto { UsuarioId = Semilla.UsuarioAdmin, Archivo = Archivo(Png, "a.png", "image/png") }, CancellationToken.None));
        Assert.Empty(Directory.GetFiles(Fabrica.CarpetaAdjuntos, "*.tmp"));
    }

    [Fact]
    public async Task Registra_metadatos_y_protege_ruta_huella_y_tamano()
    {
        var atencion = await AtencionAbiertaAsync();
        var dto = new AdjuntoCrearDto
        {
            AtencionId = atencion.AtId,
            UsuarioId = Semilla.UsuarioAdmin,
            Nombre = " Foto ",
            RutaArchivo = "adjuntos/" + Guid.NewGuid().ToString("N") + ".jpg",
            TipoMime = "image/jpeg",
            Huella = "ABC",
            TamanoBytes = 10
        };

        var creado = await Servicio.CrearAsync(dto);

        Assert.Equal("abc", creado.Huella);
        await Falla<InvalidOperationException>(() => Servicio.CrearAsync(dto));
        await Falla<InvalidOperationException>(() => Servicio.ActualizarAsync(creado.Id, new AdjuntoActualizarDto { Nombre = "x", RutaArchivo = "adjuntos/otra.jpg", TipoMime = "image/jpeg", Huella = "abc", TamanoBytes = 10 }));
        await Falla<InvalidOperationException>(() => Servicio.ActualizarAsync(creado.Id, new AdjuntoActualizarDto { Nombre = "x", RutaArchivo = dto.RutaArchivo, TipoMime = "image/jpeg", Huella = "zzz", TamanoBytes = 10 }));
        Assert.True(await Servicio.ActualizarAsync(creado.Id, new AdjuntoActualizarDto { Nombre = "Foto lateral", RutaArchivo = dto.RutaArchivo, TipoMime = "image/jpeg", Huella = "abc", TamanoBytes = 10 }));
        Assert.True(await Servicio.EliminarAsync(creado.Id));
        Assert.True(await Servicio.EliminarAsync(creado.Id));
        Assert.Null(await Servicio.AbrirArchivoAsync(creado.Id, CancellationToken.None));
        await Falla<InvalidOperationException>(() => Servicio.ActualizarAsync(creado.Id, new AdjuntoActualizarDto { Nombre = "x", RutaArchivo = dto.RutaArchivo, TipoMime = "image/jpeg", Huella = "abc", TamanoBytes = 10 }));

        dto.TamanoBytes = 11;
        await Falla<InvalidOperationException>(() => Servicio.CrearAsync(dto));
        dto.TamanoBytes = 10;
        Assert.Equal(creado.Id, (await Servicio.CrearAsync(dto)).Id);
        await Servicio.EliminarAsync(creado.Id);
        Assert.True(await Servicio.ReactivarAsync(creado.Id));
        Assert.True(await Servicio.ReactivarAsync(creado.Id));
        Assert.Single(await Servicio.ObtenerTodosAsync(atencion.AtId, false, 1, 10));
    }

    [Fact]
    public async Task Rechaza_rutas_peligrosas_y_paginacion_invalida()
    {
        var atencion = await AtencionAbiertaAsync();

        foreach (var ruta in new[] { "../secreto.pdf", "adjuntos/../x.pdf", "https://x.com/a.pdf", "adjuntos\\a.pdf", " " })
            await Falla<ArgumentException>(() => Servicio.CrearAsync(new AdjuntoCrearDto { AtencionId = atencion.AtId, UsuarioId = Semilla.UsuarioAdmin, Nombre = "x", RutaArchivo = ruta }));

        await Falla<ArgumentException>(() => Servicio.CrearAsync(new AdjuntoCrearDto { AtencionId = atencion.AtId, UsuarioId = Semilla.UsuarioAdmin, Nombre = " ", RutaArchivo = "adjuntos/a.pdf" }));
        await Falla<ArgumentException>(() => Servicio.CrearAsync(new AdjuntoCrearDto { AtencionId = atencion.AtId, UsuarioId = Semilla.UsuarioAdmin, Nombre = "x", RutaArchivo = "adjuntos/a.pdf", TamanoBytes = -1 }));
        await Falla<ArgumentException>(() => Servicio.ObtenerTodosAsync(null, false, 0, 10));
        await Falla<ArgumentException>(() => Servicio.ObtenerTodosAsync(-1, false, 1, 10));
        Assert.Null(await Servicio.ObtenerPorIdAsync(Semilla.IdInexistente));
        Assert.Null(await Servicio.AbrirArchivoAsync(Semilla.IdInexistente, CancellationToken.None));
        Assert.False(await Servicio.EliminarAsync(Semilla.IdInexistente));
        Assert.False(await Servicio.ReactivarAsync(Semilla.IdInexistente));
        Assert.False(await Servicio.ActualizarAsync(Semilla.IdInexistente, new AdjuntoActualizarDto { Nombre = "x", RutaArchivo = "adjuntos/a.pdf" }));
    }
}
