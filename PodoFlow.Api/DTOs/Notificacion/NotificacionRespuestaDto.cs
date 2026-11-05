namespace PodoFlow.Api.DTOs.Notificacion
{
    public class NotificacionRespuestaDto
    {
        public long Id { get; set; }

        public long PacienteId { get; set; }

        public long? CitaId { get; set; }

        public int PlantillaId { get; set; }

        public long? OpcionId { get; set; }

        public long? UsuarioId { get; set; }

        public string Tipo { get; set; } = string.Empty;

        public string Canal { get; set; } = string.Empty;

        public DateTime FechaProgramada { get; set; }

        public string Estado { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; }

        public byte[] Version { get; set; } = [];
    }
}
