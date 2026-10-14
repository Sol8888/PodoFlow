namespace PodoFlow.Api.DTOs.Exportacion
{
    public class ExportacionRespuestaDto
    {
        public long Id { get; set; }

        public long SolicitudId { get; set; }

        public long UsuarioId { get; set; }

        public string Formato { get; set; } = string.Empty;

        public string RutaArchivo { get; set; } = string.Empty;

        public string? Huella { get; set; }

        public DateTime FechaGeneracion { get; set; }

        public DateTime? FechaEntrega { get; set; }

        public string Estado { get; set; } = string.Empty;
    }
}