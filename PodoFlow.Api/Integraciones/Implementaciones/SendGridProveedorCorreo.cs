using PodoFlow.Api.DTOs.Notificacion;
using PodoFlow.Api.Integraciones.Interfaces;
using System.Net.Http.Headers;

namespace PodoFlow.Api.Integraciones.Implementaciones
{
    public class SendGridProveedorCorreo : IProveedorCorreo
    {
        private const string Url = "https://api.sendgrid.com/v3/mail/send";

        private readonly HttpClient _http;
        private readonly IConfiguration _configuration;

        public SendGridProveedorCorreo(HttpClient http, IConfiguration configuration)
        {
            _http = http;
            _configuration = configuration;
        }

        public async Task<ResultadoProveedorDto> EnviarAsync(string destino, string nombreDestino, string asunto, string contenidoHtml)
        {
            var apiKey = _configuration["SendGrid:ApiKey"];
            var remitente = _configuration["SendGrid:Remitente"];
            var nombreRemitente = _configuration["SendGrid:NombreRemitente"] ?? "PodoFlow";

            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(remitente))
                return Error("CONFIGURACION", "Falta SendGrid:ApiKey o SendGrid:Remitente.");

            var cuerpo = new
            {
                personalizations = new[]
                {
                    new { to = new[] { new { email = destino, name = nombreDestino } } }
                },
                from = new { email = remitente, name = nombreRemitente },
                subject = asunto,
                content = new[] { new { type = "text/html", value = contenidoHtml } }
            };

            using var solicitud = new HttpRequestMessage(HttpMethod.Post, Url)
            {
                Content = JsonContent.Create(cuerpo)
            };

            solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            using var respuesta = await _http.SendAsync(solicitud);

            if (respuesta.IsSuccessStatusCode)
            {
                var id = respuesta.Headers.TryGetValues("X-Message-Id", out var valores)
                    ? valores.FirstOrDefault()
                    : null;

                return new ResultadoProveedorDto { Exitoso = true, IdProveedor = id };
            }

            var detalle = await respuesta.Content.ReadAsStringAsync();

            return Error(((int)respuesta.StatusCode).ToString(), Recortar(detalle, 2000));
        }

        private static ResultadoProveedorDto Error(string codigo, string detalle) =>
            new() { Exitoso = false, CodigoError = codigo, DetalleError = detalle };

        private static string Recortar(string texto, int maximo) =>
            texto.Length <= maximo ? texto : texto[..maximo];
    }
}
 
    

