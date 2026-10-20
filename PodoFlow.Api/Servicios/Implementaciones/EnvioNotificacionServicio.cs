using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.DTOs.Notificacion;
using PodoFlow.Api.Integraciones.Interfaces;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Servicios.Interfaces;
using System.Net;
using System.Text.RegularExpressions;

namespace PodoFlow.Api.Servicios.Implementaciones
{
    public class EnvioNotificacionServicio : IEnvioNotificacionServicio
    {
        private const string Correo = "CORREO";
        private const string WhatsApp = "WHATSAPP";
        private const string Celular = "CELULAR";

        private static readonly Regex Marcadores = new(@"\{\{(\d+)\}\}", RegexOptions.Compiled);

        private readonly INotificacionServicio _notificaciones;
        private readonly IProveedorCorreo _correo;
        private readonly IProveedorWhatsApp _whatsApp;
        private readonly PodoFlowContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EnvioNotificacionServicio> _logger;

        public EnvioNotificacionServicio(
            INotificacionServicio notificaciones,
            IProveedorCorreo correo,
            IProveedorWhatsApp whatsApp,
            PodoFlowContext context,
            IConfiguration configuration,
            ILogger<EnvioNotificacionServicio> logger)
        {
            _notificaciones = notificaciones;
            _correo = correo;
            _whatsApp = whatsApp;
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task ProcesarAsync(NotificacionRespuestaDto notificacion, CancellationToken cancelacion)
        {
            NotificacionRespuestaDto? tomada;

            try
            {
                tomada = await _notificaciones.CambiarEstadoAsync(notificacion.Id, new NotificacionCambiarEstadoDto
                {
                    Estado = "PROCESANDO",
                    Version = notificacion.Version
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                return;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Notificación {Id} cancelada antes de enviarse: {Motivo}", notificacion.Id, ex.Message);
                await CancelarAsync(notificacion.Id);
                return;
            }

            if (tomada == null)
                return;

            ResultadoProveedorDto resultado;

            try
            {
                resultado = await EnviarAsync(tomada);
            }
            catch (Exception ex) when (!cancelacion.IsCancellationRequested)
            {
                resultado = Error("EXCEPCION", ex.Message);
            }

            await _notificaciones.RegistrarEnvioAsync(tomada.Id, new EnvioCrearDto
            {
                Resultado = resultado.Exitoso ? "ENVIADO" : "ERROR",
                IdProveedor = Recortar(resultado.IdProveedor, 300),
                CodigoError = Recortar(resultado.CodigoError, 100),
                DetalleError = Recortar(resultado.DetalleError, 2000)
            });

            if (resultado.Exitoso)
                _logger.LogInformation("Notificación {Id} enviada por {Canal}.", tomada.Id, tomada.Canal);
            else
                _logger.LogWarning("Notificación {Id} con error {Codigo}: {Detalle}", tomada.Id, resultado.CodigoError, resultado.DetalleError);
        }

        private async Task<ResultadoProveedorDto> EnviarAsync(NotificacionRespuestaDto notificacion)
        {
            var plantilla = await _context.Plantillas.AsNoTracking()
                .FirstOrDefaultAsync(x => x.PlId == notificacion.PlantillaId);

            var paciente = await _context.Pacientes.AsNoTracking()
                .FirstOrDefaultAsync(x => x.PaId == notificacion.PacienteId);

            if (plantilla == null || paciente == null)
                return Error("DATOS", "No se encontró la plantilla o el paciente.");

            Cita? cita = null;

            if (notificacion.CitaId.HasValue)
                cita = await _context.Citas.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.CiId == notificacion.CitaId.Value);

            var contacto = await ObtenerContactoAsync(notificacion.PacienteId, notificacion.Canal);

            if (contacto == null)
                return Error("SIN_DESTINO", $"El paciente no tiene un contacto activo de tipo {notificacion.Canal}.");

            var valores = ArmarValores(paciente, cita);

            if (notificacion.Canal == Correo)
            {
                var asunto = Reemplazar(plantilla.PlAsunto ?? string.Empty, valores, false);
                var html = Reemplazar(plantilla.PlConte, valores, true);
                var nombre = $"{paciente.PaNombres} {paciente.PaApelli}".Trim();

                return await _correo.EnviarAsync(contacto.DaValor.Trim(), nombre, asunto, html);
            }

            if (notificacion.Canal == WhatsApp)
            {
                var telefono = NormalizarTelefono(contacto.DaValor);

                if (telefono == null)
                    return Error("TELEFONO_INVALIDO", $"El número '{contacto.DaValor}' no tiene un formato válido.");

                var cantidad = Marcadores.Matches(plantilla.PlConte)
                    .Select(m => int.Parse(m.Groups[1].Value))
                    .DefaultIfEmpty(0)
                    .Max();

                if (cantidad > valores.Count)
                    return Error("PLANTILLA", $"La plantilla usa {{{{{cantidad}}}}} y solo hay {valores.Count} valores disponibles.");

                var parametros = valores.Take(cantidad).ToList();

                return await _whatsApp.EnviarPlantillaAsync(telefono, NombrePlantillaMeta(plantilla.PlNombre), parametros);
            }

            return Error("CANAL", $"Canal {notificacion.Canal} no soportado.");
        }

        private async Task<DatoContacto?> ObtenerContactoAsync(long pacienteId, string canal)
        {
            var tipos = canal == WhatsApp ? new[] { WhatsApp, Celular } : new[] { canal };

            return await _context.DatosContacto.AsNoTracking()
                .Where(x => x.PaId == pacienteId && x.DaEstatus && tipos.Contains(x.DaTipo))
                .OrderByDescending(x => x.DaTipo == canal)
                .ThenByDescending(x => x.DaPrinc)
                .ThenByDescending(x => x.DaVerif)
                .ThenByDescending(x => x.DaFechaCrea)
                .FirstOrDefaultAsync();
        }

        private List<string> ArmarValores(Paciente paciente, Cita? cita)
        {
            var valores = new List<string> { paciente.PaNombres.Trim() };

            if (cita != null)
            {
                var local = TimeZoneInfo.ConvertTimeFromUtc(
                    DateTime.SpecifyKind(cita.CiFechaIni, DateTimeKind.Utc), ObtenerZona());

                valores.Add(local.ToString("dd/MM/yyyy"));
                valores.Add(local.ToString("HH:mm"));
            }

            return valores;
        }

        private static string Reemplazar(string texto, List<string> valores, bool html)
        {
            var nombres = new[] { "paciente", "fecha", "hora" };

            for (var i = 0; i < valores.Count; i++)
            {
                var valor = html ? WebUtility.HtmlEncode(valores[i]) : valores[i];

                texto = texto.Replace($"{{{{{i + 1}}}}}", valor)
                             .Replace($"{{{{{nombres[i]}}}}}", valor);
            }

            return texto;
        }

        private TimeZoneInfo ObtenerZona()
        {
            var id = _configuration["Envio:ZonaHoraria"] ?? "America/Guayaquil";

            if (TimeZoneInfo.TryFindSystemTimeZoneById(id, out var zona))
                return zona;

            if (TimeZoneInfo.TryFindSystemTimeZoneById("SA Pacific Standard Time", out zona))
                return zona;

            return TimeZoneInfo.CreateCustomTimeZone("Ecuador", TimeSpan.FromHours(-5), "Ecuador", "Ecuador");
        }

        private static string? NormalizarTelefono(string valor)
        {
            var digitos = new string(valor.Where(char.IsDigit).ToArray());

            if (digitos.Length == 10 && digitos.StartsWith('0'))
                return "593" + digitos[1..];

            if (digitos.Length == 12 && digitos.StartsWith("593"))
                return digitos;

            if (digitos.Length >= 8 && digitos.Length <= 15 && valor.TrimStart().StartsWith('+'))
                return digitos;

            return null;
        }

        private static string NombrePlantillaMeta(string nombre) =>
            Regex.Replace(nombre.Trim().ToLowerInvariant(), @"[^a-z0-9_]+", "_");

        private async Task CancelarAsync(long id)
        {
            try
            {
                var actual = await _notificaciones.ObtenerPorIdAsync(id);

                if (actual == null || actual.Estado != "PENDIENTE")
                    return;

                await _notificaciones.CambiarEstadoAsync(id, new NotificacionCambiarEstadoDto
                {
                    Estado = "CANCELADA",
                    Version = actual.Version
                });
            }
            catch (Exception ex) when (ex is InvalidOperationException or DbUpdateConcurrencyException)
            {
                _logger.LogWarning("No se pudo cancelar la notificación {Id}: {Motivo}", id, ex.Message);
            }
        }

        private static ResultadoProveedorDto Error(string codigo, string detalle) =>
            new() { Exitoso = false, CodigoError = codigo, DetalleError = detalle };

        private static string? Recortar(string? texto, int maximo) =>
            texto == null || texto.Length <= maximo ? texto : texto[..maximo];
    }
}