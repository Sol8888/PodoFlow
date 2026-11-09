using PodoFlow.Api.DTOs.Notificacion;
using PodoFlow.Api.Modelos;

namespace PodoFlow.Api.Tests.Servicios.Comunicacion;

public class EnvioNotificacionServicioPruebas : PruebaServicioBase
{
    public EnvioNotificacionServicioPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    private IEnvioNotificacionServicio Envio => Servicio<IEnvioNotificacionServicio>();

    private INotificacionServicio Notificaciones => Servicio<INotificacionServicio>();

    private async Task<(Paciente Paciente, NotificacionRespuestaDto Notificacion)> PreparadaAsync(string canal, int plantillaId, string? contacto, bool conCita = true)
    {
        var paciente = await NuevoPacienteAsync();
        var consentimiento = await NuevoConsentimientoAsync(paciente.PaId, Semilla.FinalidadRecordatorios);
        var opcion = await NuevaOpcionAsync(paciente.PaId, Semilla.FinalidadRecordatorios, canal, true, consentimiento.CoId);

        if (contacto != null)
            await NuevoContactoAsync(paciente.PaId, canal == "CORREO" ? "CORREO" : "CELULAR", contacto);

        long? citaId = conCita ? (await NuevaCitaAsync(paciente.PaId, DateTime.UtcNow.AddDays(2))).CiId : null;

        var notificacion = await Notificaciones.CrearAsync(new NotificacionCrearDto
        {
            OpcionId = opcion.OpId,
            PlantillaId = plantillaId,
            CitaId = citaId,
            FechaProgramada = DateTime.UtcNow
        });

        return (paciente, notificacion);
    }

    [Fact]
    public async Task Envia_el_correo_con_los_marcadores_reemplazados()
    {
        var (paciente, notificacion) = await PreparadaAsync("CORREO", Semilla.PlantillaCorreo, "paciente@pruebas.podoflow.test");

        await Envio.ProcesarAsync(notificacion, CancellationToken.None);

        var correo = Assert.Single(Fabrica.Correo.Enviados);
        Assert.Equal("paciente@pruebas.podoflow.test", correo.Destino);
        Assert.Contains(paciente.PaNombres, correo.ContenidoHtml);
        Assert.DoesNotContain("{{", correo.ContenidoHtml);
        Assert.Equal("ENVIADA", (await Notificaciones.ObtenerPorIdAsync(notificacion.Id))!.Estado);
    }

    [Fact]
    public async Task Envia_whatsapp_normalizando_el_telefono_ecuatoriano()
    {
        var (_, notificacion) = await PreparadaAsync("WHATSAPP", Semilla.PlantillaWhatsApp, "099 123 4567");

        await Envio.ProcesarAsync(notificacion, CancellationToken.None);

        var mensaje = Assert.Single(Fabrica.WhatsApp.Enviados);
        Assert.Equal("593991234567", mensaje.Telefono);
        Assert.Equal("recordatorio_cita_pruebas", mensaje.NombrePlantilla);
        Assert.Equal(3, mensaje.Parametros.Count);
    }

    [Fact]
    public async Task Registra_error_si_el_paciente_no_tiene_contacto_o_el_telefono_es_invalido()
    {
        var (_, sinContacto) = await PreparadaAsync("CORREO", Semilla.PlantillaCorreo, null);
        await Envio.ProcesarAsync(sinContacto, CancellationToken.None);

        var (_, telefonoMalo) = await PreparadaAsync("WHATSAPP", Semilla.PlantillaWhatsApp, "12-34");
        await Envio.ProcesarAsync(telefonoMalo, CancellationToken.None);

        Assert.Equal("SIN_DESTINO", (await Notificaciones.ObtenerEnviosAsync(sinContacto.Id))!.Single().CodigoError);
        Assert.Equal("TELEFONO_INVALIDO", (await Notificaciones.ObtenerEnviosAsync(telefonoMalo.Id))!.Single().CodigoError);
        Assert.Equal("ERROR", (await Notificaciones.ObtenerPorIdAsync(telefonoMalo.Id))!.Estado);
    }

    [Fact]
    public async Task Registra_error_si_la_plantilla_pide_mas_valores_de_los_disponibles()
    {
        var plantilla = await NuevaPlantillaAsync("WHATSAPP", "AVISO_ADMINISTRATIVO", "Hola {{1}}, fecha {{2}}");
        var (_, notificacion) = await PreparadaAsync("WHATSAPP", plantilla.PlId, "+593 99 765 4321", conCita: false);

        await Envio.ProcesarAsync(notificacion, CancellationToken.None);

        Assert.Equal("PLANTILLA", (await Notificaciones.ObtenerEnviosAsync(notificacion.Id))!.Single().CodigoError);
    }

    [Fact]
    public async Task Registra_el_fallo_del_proveedor()
    {
        Fabrica.Correo.Fallar = true;
        var (_, notificacion) = await PreparadaAsync("CORREO", Semilla.PlantillaCorreo, "paciente@pruebas.podoflow.test");

        await Envio.ProcesarAsync(notificacion, CancellationToken.None);

        Assert.Equal("ERROR", (await Notificaciones.ObtenerPorIdAsync(notificacion.Id))!.Estado);
    }

    [Fact]
    public async Task Cancela_la_notificacion_si_el_paciente_revoco_el_permiso()
    {
        var (paciente, notificacion) = await PreparadaAsync("CORREO", Semilla.PlantillaCorreo, "paciente@pruebas.podoflow.test");
        var opcion = await Contexto.OpcionesComunicacion.SingleAsync(o => o.PaId == paciente.PaId);
        opcion.OpPermi = false;
        await Contexto.SaveChangesAsync();

        await Envio.ProcesarAsync(notificacion, CancellationToken.None);

        Assert.Empty(Fabrica.Correo.Enviados);
        Assert.Equal("CANCELADA", (await Notificaciones.ObtenerPorIdAsync(notificacion.Id))!.Estado);
    }
}
