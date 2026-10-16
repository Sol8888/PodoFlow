namespace PodoFlow.Api.DTOs.Consentimiento
{
    public class EventoConsentimientoRespuestaDto
    {
        public long Id { get; set; }

        public long ConsentimientoId { get; set; }

        public long? UsuarioId { get; set; }

        public string Estado { get; set; } = string.Empty;

        public DateTime FechaHora { get; set; }

        public string Medio { get; set; } = string.Empty;

        public string? RutaEvidencia { get; set; }

        public string? Observacion { get; set; }
    }
}