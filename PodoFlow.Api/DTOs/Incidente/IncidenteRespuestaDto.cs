namespace PodoFlow.Api.DTOs.Incidente
{
    public class IncidenteRespuestaDto
    {
        public long Id { get; set; }

        public long UsuarioId { get; set; }

        public DateTime FechaDeteccion { get; set; }

        public string Tipo { get; set; } = string.Empty;

        public string Modulo { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public string? DatosAfectados { get; set; }

        public string? ImpactoPreliminar { get; set; }

        public string Estado { get; set; } = string.Empty;

        public DateTime? FechaCierre { get; set; }

        public byte[] Version { get; set; } = [];
    }
}
