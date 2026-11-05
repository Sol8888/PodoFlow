namespace PodoFlow.Api.DTOs.EvidenciaSolicitud
{
    public class EvidenciaSolicitudRespuestaDto
    {
        public long Id { get; set; }

        public long SolicitudId { get; set; }

        public long UsuarioId { get; set; }

        public string Tipo { get; set; } = string.Empty;

        public string RutaArchivo { get; set; } = string.Empty;

        public string? Huella { get; set; }

        public DateTime FechaCarga { get; set; }
    }
}
