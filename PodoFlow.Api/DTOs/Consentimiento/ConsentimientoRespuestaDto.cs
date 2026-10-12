namespace PodoFlow.Api.DTOs.Consentimiento
{
    public class ConsentimientoRespuestaDto
    {
        public long Id { get; set; }

        public long PacienteId { get; set; }

        public int FinalidadId { get; set; }

        public int AvisoId { get; set; }

        public string Estado { get; set; } = string.Empty;

        public string Medio { get; set; } = string.Empty;

        public DateTime FechaObtencion { get; set; }

        public DateTime? FechaRevocacion { get; set; }

        public string? Observacion { get; set; }

        public byte[] Version { get; set; } = [];
    }
}
