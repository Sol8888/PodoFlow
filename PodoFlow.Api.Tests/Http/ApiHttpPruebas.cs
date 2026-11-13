using System.Net;
using System.Net.Http.Json;
using Microsoft.Data.SqlClient;

namespace PodoFlow.Api.Tests.Http;

public class AutenticacionHttpPruebas : PruebaHttpBase
{
    public AutenticacionHttpPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    [Theory]
    [InlineData("/api/Finalidad")]
    [InlineData("/api/Consentimiento")]
    [InlineData("/api/Notificacion")]
    [InlineData("/api/pacientes")]
    [InlineData("/api/citas")]
    [InlineData("/api/insumos")]
    public async Task Sin_token_responde_401(string ruta)
    {
        using var anonimo = Fabrica.CrearCliente(conToken: false);

        var respuesta = await anonimo.GetAsync(ruta);

        Estado(HttpStatusCode.Unauthorized, respuesta);
    }

    [Fact]
    public async Task Con_un_token_distinto_responde_401()
    {
        using var cliente = Fabrica.CrearCliente(conToken: false);
        cliente.DefaultRequestHeaders.Add("X-Dev-Token", "token-equivocado");

        var respuesta = await cliente.GetAsync("/api/Finalidad");

        Estado(HttpStatusCode.Unauthorized, respuesta);
    }
}

public class LecturaHttpPruebas : PruebaHttpBase
{
    public LecturaHttpPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    [Theory]
    [InlineData("/api/Finalidad")]
    [InlineData("/api/Aviso")]
    [InlineData("/api/Consentimiento")]
    [InlineData("/api/Consentimiento/paciente/900001")]
    [InlineData("/api/Plantilla?canal=WHATSAPP")]
    [InlineData("/api/OpcionComunicacion")]
    [InlineData("/api/Notificacion")]
    [InlineData("/api/Solicitud")]
    [InlineData("/api/Exportacion")]
    [InlineData("/api/Incidente")]
    [InlineData("/api/Riesgo")]
    [InlineData("/api/EvaluacionImpacto")]
    [InlineData("/api/Documento")]
    [InlineData("/api/TratamientoDatos")]
    [InlineData("/api/CategoriaDato")]
    [InlineData("/api/Encargado")]
    [InlineData("/api/Seudonimo")]
    [InlineData("/api/Ejecucion")]
    [InlineData("/api/pacientes")]
    [InlineData("/api/datos-contacto?pacienteId=900001")]
    [InlineData("/api/citas")]
    [InlineData("/api/horarios-atencion")]
    [InlineData("/api/bloqueos-agenda")]
    [InlineData("/api/historias")]
    [InlineData("/api/atenciones")]
    [InlineData("/api/evoluciones")]
    [InlineData("/api/adjuntos")]
    [InlineData("/api/insumos")]
    [InlineData("/api/movimientos")]
    [InlineData("/api/prestaciones")]
    [InlineData("/api/cobros")]
    [InlineData("/api/formas-pago")]
    [InlineData("/api/usuarios")]
    [InlineData("/api/roles")]
    [InlineData("/api/permisos")]
    [InlineData("/api/servicios")]
    public async Task Con_token_los_listados_responden_200(string ruta)
    {
        var respuesta = await Cliente.GetAsync(ruta);

        Estado(HttpStatusCode.OK, respuesta);
    }

    [Theory]
    [InlineData("/api/Finalidad/1")]
    [InlineData("/api/Aviso/900001")]
    [InlineData("/api/Consentimiento/900001")]
    [InlineData("/api/Plantilla/900001")]
    [InlineData("/api/pacientes/900001")]
    [InlineData("/api/citas/900001")]
    [InlineData("/api/usuarios/900001")]
    public async Task Con_token_los_registros_sembrados_responden_200(string ruta)
    {
        var respuesta = await Cliente.GetAsync(ruta);

        Estado(HttpStatusCode.OK, respuesta);
    }

    [Fact]
    public async Task La_disponibilidad_de_citas_responde_200()
    {
        var fecha = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)).ToString("yyyy-MM-dd");

        var respuesta = await Cliente.GetAsync($"/api/citas/disponibilidad?servicioId={Semilla.Servicio}&fecha={fecha}");

        Estado(HttpStatusCode.OK, respuesta);
    }

    [Fact]
    public async Task El_listado_de_plantillas_trae_los_datos_sembrados()
    {
        var plantillas = await Cliente.GetFromJsonAsync<List<Dictionary<string, object>>>("/api/Plantilla?tamano=100");

        Assert.Contains(plantillas!, p => p["id"].ToString() == Semilla.PlantillaWhatsApp.ToString());
    }

    [Theory]
    [InlineData("/api/Finalidad/999999999")]
    [InlineData("/api/Aviso/999999999")]
    [InlineData("/api/Consentimiento/999999999")]
    [InlineData("/api/Consentimiento/999999999/eventos")]
    [InlineData("/api/Notificacion/999999999")]
    [InlineData("/api/Solicitud/999999999")]
    [InlineData("/api/Incidente/999999999")]
    [InlineData("/api/Plantilla/999999999")]
    [InlineData("/api/pacientes/999999999")]
    [InlineData("/api/citas/999999999")]
    [InlineData("/api/historias/999999999")]
    [InlineData("/api/insumos/999999999")]
    [InlineData("/api/usuarios/999999999")]
    public async Task Un_id_inexistente_responde_404(string ruta)
    {
        var respuesta = await Cliente.GetAsync(ruta);

        Estado(HttpStatusCode.NotFound, respuesta);
    }

    [Fact]
    public async Task Cambiar_el_estado_de_un_incidente_inexistente_responde_404()
    {
        var respuesta = await EnviarAsync(HttpMethod.Patch, "/api/Incidente/999999999/estado", new
        {
            estado = "ANALISIS",
            usuarioId = Semilla.UsuarioAdmin,
            version = Convert.ToBase64String(new byte[8])
        });

        Estado(HttpStatusCode.NotFound, respuesta);
    }
}

public class ValidacionHttpPruebas : PruebaHttpBase
{
    public ValidacionHttpPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    [Theory]
    [InlineData("/api/Finalidad?tamano=0")]
    [InlineData("/api/Plantilla?tamano=101")]
    [InlineData("/api/Notificacion?pagina=0")]
    [InlineData("/api/Incidente?tamano=500")]
    [InlineData("/api/Consentimiento?pagina=-1")]
    public async Task La_paginacion_invalida_responde_400_con_mensaje(string ruta)
    {
        var respuesta = await Cliente.GetAsync(ruta);

        Estado(HttpStatusCode.BadRequest, respuesta);
        Assert.Contains("Paginación", await MensajeAsync(respuesta));
    }

    [Theory]
    [InlineData("/api/pacientes?tamano=0")]
    [InlineData("/api/citas?pagina=0")]
    [InlineData("/api/insumos?tamano=101")]
    [InlineData("/api/usuarios?tamano=0")]
    public async Task La_paginacion_invalida_en_modulos_de_persona_1_responde_400(string ruta)
    {
        var respuesta = await Cliente.GetAsync(ruta);

        Estado(HttpStatusCode.BadRequest, respuesta);
    }

    [Theory(Skip = "F-03 (soledad): los controladores de pac, cit, cli, inv, fin y seg devuelven el error como texto plano y no como { mensaje }. Reportado, sin corregir.")]
    [InlineData("/api/pacientes?tamano=0")]
    [InlineData("/api/citas?pagina=0")]
    public async Task La_paginacion_invalida_en_modulos_de_persona_1_trae_mensaje(string ruta)
    {
        var respuesta = await Cliente.GetAsync(ruta);

        Assert.False(string.IsNullOrWhiteSpace(await MensajeAsync(respuesta)));
    }

    [Fact]
    public async Task Un_filtro_de_estado_desconocido_responde_400_con_mensaje()
    {
        var respuesta = await Cliente.GetAsync("/api/Incidente?estado=PERDIDO");

        Estado(HttpStatusCode.BadRequest, respuesta);
        Assert.Contains("ABIERTO", await MensajeAsync(respuesta));
    }

    [Fact]
    public async Task Consentimiento_de_menor_sin_representante_responde_400_con_mensaje()
    {
        var respuesta = await EnviarAsync(HttpMethod.Post, "/api/Consentimiento", new
        {
            pacienteId = Semilla.PacienteMenor,
            finalidadId = 3,
            avisoId = Semilla.Aviso,
            estado = "ACEPTADO",
            medio = "PRESENCIAL"
        });

        Estado(HttpStatusCode.BadRequest, respuesta);
        Assert.Contains("menor de edad", await MensajeAsync(respuesta));
    }

    [Fact]
    public async Task Plantilla_de_whatsapp_promocional_responde_400_con_mensaje()
    {
        var respuesta = await EnviarAsync(HttpMethod.Post, "/api/Plantilla", new
        {
            nombre = "promo_http",
            canal = "WHATSAPP",
            tipo = "PROMOCION",
            contenido = "Descuento"
        });

        Estado(HttpStatusCode.BadRequest, respuesta);
        Assert.Contains("operativos", await MensajeAsync(respuesta));
    }

    [Fact]
    public async Task Recordatorios_con_horas_fuera_de_rango_responde_400_con_mensaje()
    {
        var respuesta = await Cliente.PostAsync("/api/Notificacion/recordatorios?horasAntes=0", null);

        Estado(HttpStatusCode.BadRequest, respuesta);
        Assert.Contains("horas", await MensajeAsync(respuesta));
    }

    [Fact]
    public async Task Un_cuerpo_sin_campos_obligatorios_responde_400()
    {
        var respuesta = await EnviarAsync(HttpMethod.Post, "/api/Finalidad", new { descripcion = "sin código" });

        Estado(HttpStatusCode.BadRequest, respuesta);
    }

    [Fact]
    public async Task Paciente_con_cedula_invalida_responde_400()
    {
        var respuesta = await EnviarAsync(HttpMethod.Post, "/api/pacientes", new
        {
            tipoIdentificacion = "CEDULA",
            identificacion = "1710034066",
            nombres = "Cédula",
            apellidos = "Inválida"
        });

        Estado(HttpStatusCode.BadRequest, respuesta);
    }
}

public class ConflictoHttpPruebas : PruebaHttpBase
{
    public ConflictoHttpPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    [Fact]
    public async Task Una_finalidad_duplicada_responde_409_con_mensaje()
    {
        var respuesta = await EnviarAsync(HttpMethod.Post, "/api/Finalidad", new { codigo = "RECORDATORIOS", nombre = "Duplicada" });

        Estado(HttpStatusCode.Conflict, respuesta);
        Assert.Contains("RECORDATORIOS", await MensajeAsync(respuesta));
    }

    [Fact]
    public async Task Un_aviso_con_version_repetida_responde_409_con_mensaje()
    {
        var respuesta = await EnviarAsync(HttpMethod.Post, "/api/Aviso", new { version = Semilla.VersionAviso, fechaDesde = "2026-01-01" });

        Estado(HttpStatusCode.Conflict, respuesta);
        Assert.Contains(Semilla.VersionAviso, await MensajeAsync(respuesta));
    }

    [Fact]
    public async Task Un_consentimiento_repetido_responde_409_con_mensaje()
    {
        var respuesta = await EnviarAsync(HttpMethod.Post, "/api/Consentimiento", new
        {
            pacienteId = Semilla.PacienteAdulto,
            finalidadId = 4,
            avisoId = Semilla.Aviso,
            estado = "ACEPTADO",
            medio = "PRESENCIAL"
        });

        Estado(HttpStatusCode.Conflict, respuesta);
        Assert.Contains("ya tiene un consentimiento", await MensajeAsync(respuesta));
    }

    [Fact]
    public async Task Un_paciente_con_cedula_repetida_responde_409()
    {
        var respuesta = await EnviarAsync(HttpMethod.Post, "/api/pacientes", new
        {
            tipoIdentificacion = "CEDULA",
            identificacion = Semilla.CedulaPacienteAdulto,
            nombres = "Repetida",
            apellidos = "Pruebas"
        });

        Estado(HttpStatusCode.Conflict, respuesta);
    }

    [Fact]
    public async Task Actualizar_un_consentimiento_con_version_vencida_responde_409_con_mensaje()
    {
        var respuesta = await EnviarAsync(HttpMethod.Put, $"/api/Consentimiento/{Semilla.ConsentimientoFidelizacion}", new
        {
            medio = "CORREO",
            observacion = "Intento con versión vieja",
            version = Convert.ToBase64String(new byte[8])
        });

        Estado(HttpStatusCode.Conflict, respuesta);
        Assert.False(string.IsNullOrWhiteSpace(await MensajeAsync(respuesta)));
    }

    [Fact]
    public async Task Actualizar_un_paciente_con_version_vencida_responde_409()
    {
        var respuesta = await EnviarAsync(HttpMethod.Put, $"/api/pacientes/{Semilla.PacienteAdulto}", new
        {
            tipoIdentificacion = "CEDULA",
            identificacion = Semilla.CedulaPacienteAdulto,
            nombres = "Intento",
            apellidos = "Versión vieja",
            version = Convert.ToBase64String(new byte[8])
        });

        Estado(HttpStatusCode.Conflict, respuesta);
    }

    [Fact]
    public async Task Las_peticiones_rechazadas_no_escriben_en_la_base()
    {
        var antes = await ContarFilasAsync();

        await Una_finalidad_duplicada_responde_409_con_mensaje();
        await Un_consentimiento_repetido_responde_409_con_mensaje();
        await Actualizar_un_consentimiento_con_version_vencida_responde_409_con_mensaje();
        await Actualizar_un_paciente_con_version_vencida_responde_409();
        await EnviarAsync(HttpMethod.Post, "/api/Plantilla", new { nombre = "promo", canal = "WHATSAPP", tipo = "PROMOCION", contenido = "x" });

        Assert.Equal(antes, await ContarFilasAsync());
    }

    private static async Task<string> ContarFilasAsync()
    {
        await using var conexion = new SqlConnection(ConfiguracionPruebas.CadenaConexion);
        await conexion.OpenAsync();
        await using var comando = conexion.CreateCommand();
        comando.CommandText = """
            SELECT CONCAT(
                (SELECT COUNT(*) FROM pri.FINALIDAD), '|',
                (SELECT COUNT(*) FROM pri.CONSENTIMIENTO), '|',
                (SELECT COUNT(*) FROM pri.EVENTO_CONSENTIMIENTO), '|',
                (SELECT COUNT(*) FROM com.PLANTILLA), '|',
                (SELECT COUNT(*) FROM pac.PACIENTE), '|',
                (SELECT CONVERT(varchar(30), co_version, 1) FROM pri.CONSENTIMIENTO WHERE co_id = 900001), '|',
                (SELECT CONVERT(varchar(30), pa_version, 1) FROM pac.PACIENTE WHERE pa_id = 900001))
            """;

        return (string)(await comando.ExecuteScalarAsync())!;
    }
}
