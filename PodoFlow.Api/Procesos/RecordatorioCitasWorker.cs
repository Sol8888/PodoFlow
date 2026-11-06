using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Procesos
{
    public class RecordatorioCitasWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopes;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RecordatorioCitasWorker> _logger;

        public RecordatorioCitasWorker(
            IServiceScopeFactory scopes,
            IConfiguration configuration,
            ILogger<RecordatorioCitasWorker> logger)
        {
            _scopes = scopes;
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancelacion)
        {
            if (!_configuration.GetValue("Recordatorios:Habilitado", false))
            {
                _logger.LogInformation("Recordatorios automáticos deshabilitados (Recordatorios:Habilitado = false).");
                return;
            }

            var intervalo = TimeSpan.FromMinutes(_configuration.GetValue("Recordatorios:IntervaloMinutos", 15));
            var horasAntes = _configuration.GetValue("Recordatorios:HorasAntes", 24);

            using var temporizador = new PeriodicTimer(intervalo);

            do
            {
                try
                {
                    using var scope = _scopes.CreateScope();
                    var notificaciones = scope.ServiceProvider.GetRequiredService<INotificacionServicio>();

                    var (programadas, omitidas) = await notificaciones.ProgramarRecordatoriosAsync(horasAntes);

                    if (programadas > 0 || omitidas > 0)
                        _logger.LogInformation(
                            "Recordatorios de citas: {Programadas} programados, {Omitidas} omitidos.", programadas, omitidas);
                }
                catch (Exception ex) when (!cancelacion.IsCancellationRequested)
                {
                    _logger.LogError(ex, "Error al programar los recordatorios de citas.");
                }
            }
            while (await temporizador.WaitForNextTickAsync(cancelacion));
        }
    }
}
