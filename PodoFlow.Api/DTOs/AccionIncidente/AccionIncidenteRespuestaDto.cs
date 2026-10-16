namespace PodoFlow.Api.DTOs.Incidente
{
    public class AccionIncidenteRespuestaDto
    {
        public long Id { get; set; }

        public long IncidenteId { get; set; }

        public long UsuarioId { get; set; }

        public DateTime FechaHora { get; set; }

        public string Tipo { get; set; } = string.Empty;

        public string Accion { get; set; } = string.Empty;

        public string? Observacion { get; set; }
    }
}
