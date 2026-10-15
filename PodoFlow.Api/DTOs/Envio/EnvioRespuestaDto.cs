namespace PodoFlow.Api.DTOs.Notificacion
{
    public class EnvioRespuestaDto
    {
        public long Id { get; set; }

        public long NotificacionId { get; set; }

        public short NumeroIntento { get; set; }

        public DateTime FechaHora { get; set; }

        public string Resultado { get; set; } = string.Empty;

        public string? IdProveedor { get; set; }

        public string? CodigoError { get; set; }

        public string? DetalleError { get; set; }
    }
}
