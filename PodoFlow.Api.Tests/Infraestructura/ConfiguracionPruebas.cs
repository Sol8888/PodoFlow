using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace PodoFlow.Api.Tests.Infraestructura;

public static class ConfiguracionPruebas
{
    public const string BaseDatos = "PODOFLOW_PRUEBAS";

    private static readonly string[] ServidoresLocales = ["localhost", "(local)", ".", "127.0.0.1"];

    private static readonly Lazy<string> Cadena = new(Cargar);

    public static string CadenaConexion => Cadena.Value;

    private static string Cargar()
    {
        var configuracion = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.Pruebas.json", optional: false)
            .AddEnvironmentVariables("PODOFLOW_PRUEBAS_")
            .Build();

        var cadena = configuracion.GetConnectionString("PodoFlowPruebas");

        if (string.IsNullOrWhiteSpace(cadena))
            throw new InvalidOperationException("Falta ConnectionStrings:PodoFlowPruebas en appsettings.Pruebas.json.");

        var datos = new SqlConnectionStringBuilder(cadena);

        if (!string.Equals(datos.InitialCatalog, BaseDatos, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"Las pruebas solo pueden usar la base {BaseDatos}; la cadena apunta a '{datos.InitialCatalog}'.");

        var servidor = datos.DataSource.Split('\\', ',')[0].Trim().ToLowerInvariant();

        if (!ServidoresLocales.Contains(servidor))
            throw new InvalidOperationException(
                $"Las pruebas solo pueden usar un servidor SQL local; la cadena apunta a '{datos.DataSource}'.");

        return datos.ConnectionString;
    }
}
