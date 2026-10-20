using System.Net.Http.Headers;
using System.Text.Json;
using PodoFlow.Api.DTOs.Notificacion;
using PodoFlow.Api.Integraciones.Interfaces;

namespace PodoFlow.Api.Integraciones.Implementaciones
{
    public class MetaProveedorWhatsApp : IProveedorWhatsApp

    {
        private readonly HttpClient _http;
        private readonly IConfiguration _configuration;

        public MetaProveedorWhatsApp(HttpClient http, IConfiguration configuration)
        {
            _http = http;
            _configuration = configuration;
        }

        public async Task<ResultadoProveedorDto> EnviarPlantillaAsync(string telefono, string nombrePlantilla, IReadOnlyList<string> parametros)
        {
            var token = _configuration["WhatsApp:Token"];
            var numeroId = _configuration["WhatsApp:NumeroId"];
            var version = _configuration["WhatsApp:Version"] ?? "v21.0";
            var idioma = _configuration["WhatsApp:Idioma"] ?? "es";

            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(numeroId))
                return Error("CONFIGURACION", "Falta WhatsApp:Token o WhatsApp:NumeroId.");

            var componentes = parametros.Count == 0
                ? Array.Empty<object>()
                : new object[]
                {
                    new
                    {
                        type = "body",
                        parameters = parametros.Select(p => new { type = "text", text = p }).ToArray()
                    }
                };

            var cuerpo = new
            {
                messaging_product = "whatsapp",
                to = telefono,
                type = "template",
                template = new
                {
                    name = nombrePlantilla,
                    language = new { code = idioma },
                    components = componentes
                }
            };

            var url = $"https://graph.facebook.com/{version}/{numeroId}/messages";

            using var solicitud = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(cuerpo)
            };

            solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var respuesta = await _http.SendAsync(solicitud);
            var texto = await respuesta.Content.ReadAsStringAsync();

            if (respuesta.IsSuccessStatusCode)
                return new ResultadoProveedorDto { Exitoso = true, IdProveedor = LeerIdMensaje(texto) };

            return LeerError(texto, (int)respuesta.StatusCode);
        }

        private static string? LeerIdMensaje(string json)
        {
            using var documento = JsonDocument.Parse(json);

            if (documento.RootElement.TryGetProperty("messages", out var mensajes) &&
                mensajes.ValueKind == JsonValueKind.Array &&
                mensajes.GetArrayLength() > 0 &&
                mensajes[0].TryGetProperty("id", out var id))
                return id.GetString();

            return null;
        }

        private static ResultadoProveedorDto LeerError(string json, int estadoHttp)
        {
            try
            {
                using var documento = JsonDocument.Parse(json);

                if (documento.RootElement.TryGetProperty("error", out var error))
                {
                    var codigo = error.TryGetProperty("code", out var c) ? c.ToString() : estadoHttp.ToString();
                    var mensaje = error.TryGetProperty("message", out var m) ? m.GetString() : null;

                    return Error(codigo, Recortar(mensaje ?? json, 2000));
                }
            }
            catch (JsonException)
            {
            }

            return Error(estadoHttp.ToString(), Recortar(json, 2000));
        }

        private static ResultadoProveedorDto Error(string codigo, string detalle) =>
            new() { Exitoso = false, CodigoError = codigo, DetalleError = detalle };

        private static string Recortar(string texto, int maximo) =>
            texto.Length <= maximo ? texto : texto[..maximo];
    }
}
