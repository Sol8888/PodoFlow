using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Procesos
{
    public class EnvioNotificacionesWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopes;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EnvioNotificacionesWorker> _logger;

        public EnvioNotificacionesWorker(
            IServiceScopeFactory scopes,
            IConfiguration configuration,
            ILogger<EnvioNotificacionesWorker> logger)
        {
            _scopes = scopes;
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancelacion)
        {
            if (!_configuration.GetValue("Envio:Habilitado", false))
            {
                _logger.LogInformation("Envío de notificaciones deshabilitado (Envio:Habilitado = false).");
                return;
            }

            var intervalo = TimeSpan.FromSeconds(_configuration.GetValue("Envio:IntervaloSegundos", 60));
            var tamano = _configuration.GetValue("Envio:Lote", 20);

            using var temporizador = new PeriodicTimer(intervalo);

            do
            {
                try
                {
                    await ProcesarLoteAsync(tamano, cancelacion);
                }
                catch (Exception ex) when (!cancelacion.IsCancellationRequested)
                {
                    _logger.LogError(ex, "Error al procesar el lote de notificaciones.");
                }
            }
            while (await temporizador.WaitForNextTickAsync(cancelacion));
        }

        private async Task ProcesarLoteAsync(int tamano, CancellationToken cancelacion)
        {
            List<DTOs.Notificacion.NotificacionRespuestaDto> lote;

            using (var scope = _scopes.CreateScope())
            {
                var notificaciones = scope.ServiceProvider.GetRequiredService<INotificacionServicio>();
                lote = await notificaciones.ObtenerPorEnviarAsync(tamano);
            }

            foreach (var notificacion in lote)
            {
                if (cancelacion.IsCancellationRequested)
                    break;

                using var scope = _scopes.CreateScope();
                var envio = scope.ServiceProvider.GetRequiredService<IEnvioNotificacionServicio>();

                await envio.ProcesarAsync(notificacion, cancelacion);
            }
        }
    }
}