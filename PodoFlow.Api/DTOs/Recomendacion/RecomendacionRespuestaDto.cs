namespace PodoFlow.Api.DTOs.Ejecucion
{
    public class RecomendacionRespuestaDto
    {
        public long Id { get; set; }

        public long EjecucionId { get; set; }

        public string Segmento { get; set; } = string.Empty;

        public string Hallazgo { get; set; } = string.Empty;

        public string Sugerencia { get; set; } = string.Empty;

        public int PacientesEstimados { get; set; }

        public string NivelPrivacidad { get; set; } = string.Empty;

        public DateTime FechaGeneracion { get; set; }
    }
}
