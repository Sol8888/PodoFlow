using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Integraciones.Interfaces;
using PodoFlow.Api.Procesos;

namespace PodoFlow.Api.Tests.Infraestructura;

public class FabricaApi : WebApplicationFactory<PodoFlowContext>
{
    private static readonly Type[] TrabajosEnSegundoPlano =
    [
        typeof(EnvioNotificacionesWorker),
        typeof(RecordatorioCitasWorker)
    ];

    public string Token { get; } = "token-pruebas-" + Guid.NewGuid().ToString("N");

    public string CarpetaTemporal { get; } = Path.Combine(
        Path.GetTempPath(), "PodoFlowPruebas", Guid.NewGuid().ToString("N"));

    public string CarpetaAdjuntos => Path.Combine(CarpetaTemporal, "adjuntos");

    public string CarpetaExportaciones => Path.Combine(CarpetaTemporal, "exportaciones");

    public ProveedorCorreoFalso Correo { get; } = new();

    public ProveedorWhatsAppFalso WhatsApp { get; } = new();

    public HttpClient CrearCliente(bool conToken = true)
    {
        var cliente = CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });

        cliente.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (conToken)
            cliente.DefaultRequestHeaders.Add("X-Dev-Token", Token);

        return cliente;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var valores = ValoresDeConfiguracion();

        builder.UseEnvironment(Environments.Development);

        foreach (var (clave, valor) in valores)
            builder.UseSetting(clave, valor);

        builder.ConfigureAppConfiguration((_, configuracion) =>
        {
            var secretos = configuracion.Sources
                .OfType<JsonConfigurationSource>()
                .Where(s => s.Path != null && s.Path.EndsWith("secrets.json", StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var fuente in secretos)
                configuracion.Sources.Remove(fuente);

            configuracion.AddInMemoryCollection(valores);
        });

        builder.ConfigureTestServices(servicios =>
        {
            var trabajos = servicios
                .Where(d => d.ServiceType == typeof(IHostedService) && TrabajosEnSegundoPlano.Contains(d.ImplementationType))
                .ToList();

            foreach (var trabajo in trabajos)
                servicios.Remove(trabajo);

            servicios.RemoveAll<DbContextOptions<PodoFlowContext>>();
            servicios.RemoveAll<DbContextOptions>();
            servicios.AddDbContext<PodoFlowContext>(opciones => opciones
                .UseSqlServer(ConfiguracionPruebas.CadenaConexion)
                .ReplaceService<ISqlServerConnection, ConexionPruebas>());

            servicios.RemoveAll<IProveedorCorreo>();
            servicios.AddSingleton<IProveedorCorreo>(Correo);

            servicios.RemoveAll<IProveedorWhatsApp>();
            servicios.AddSingleton<IProveedorWhatsApp>(WhatsApp);
        });
    }

    private Dictionary<string, string?> ValoresDeConfiguracion()
    {
        Directory.CreateDirectory(CarpetaAdjuntos);
        Directory.CreateDirectory(CarpetaExportaciones);

        return new Dictionary<string, string?>
        {
            ["ConnectionStrings:PodoFlowConnection"] = ConfiguracionPruebas.CadenaConexion,
            ["AutenticacionDesarrollo:Token"] = Token,
            ["AlmacenamientoClinico:Directorio"] = CarpetaAdjuntos,
            ["Exportacion:Carpeta"] = CarpetaExportaciones,
            ["Recordatorios:Habilitado"] = "false",
            ["Envio:Habilitado"] = "false",
            ["WhatsApp:Token"] = null,
            ["SendGrid:ApiKey"] = null,
            ["Resend:ApiKey"] = null
        };
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing && Directory.Exists(CarpetaTemporal))
        {
            try
            {
                Directory.Delete(CarpetaTemporal, recursive: true);
            }
            catch (IOException)
            {
            }
        }
    }
}

[CollectionDefinition(Nombre)]
public class ColeccionApi : ICollectionFixture<FabricaApi>
{
    public const string Nombre = "API PodoFlow";
}
