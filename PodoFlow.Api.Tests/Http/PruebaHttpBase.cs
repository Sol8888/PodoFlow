using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace PodoFlow.Api.Tests.Http;

[Collection(ColeccionApi.Nombre)]
public abstract class PruebaHttpBase
{
    protected PruebaHttpBase(FabricaApi fabrica)
    {
        Fabrica = fabrica;
        Cliente = fabrica.CrearCliente();
    }

    protected FabricaApi Fabrica { get; }

    protected HttpClient Cliente { get; }

    protected static async Task<string> MensajeAsync(HttpResponseMessage respuesta)
    {
        var texto = await respuesta.Content.ReadAsStringAsync();

        using var documento = JsonDocument.Parse(texto);

        Assert.Equal(JsonValueKind.Object, documento.RootElement.ValueKind);
        Assert.True(documento.RootElement.TryGetProperty("mensaje", out var mensaje), $"El cuerpo no tiene la propiedad 'mensaje': {texto}");

        return mensaje.GetString() ?? string.Empty;
    }

    protected static void Estado(HttpStatusCode esperado, HttpResponseMessage respuesta)
        => Assert.True(esperado == respuesta.StatusCode, $"Se esperaba {(int)esperado} y llegó {(int)respuesta.StatusCode}.");

    protected Task<HttpResponseMessage> EnviarAsync(HttpMethod metodo, string ruta, object cuerpo)
        => Cliente.SendAsync(new HttpRequestMessage(metodo, ruta) { Content = JsonContent.Create(cuerpo) });
}
