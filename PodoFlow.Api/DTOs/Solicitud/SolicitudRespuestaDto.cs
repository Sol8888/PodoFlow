namespace PodoFlow.Api.DTOs.Solicitud
{
    public class SolicitudRespuestaDto
    {
        public long Id { get; set; }

        public long PacienteId { get; set; }

        public long? UsuarioId { get; set; }

        public string TipoDerecho { get; set; } = string.Empty;

        public string TipoSolicitante { get; set; } = string.Empty;

        public string NombreSolicitante { get; set; } = string.Empty;

        public string IdentificacionSolicitante { get; set; } = string.Empty;

        public DateTime FechaRecepcion { get; set; }

        public string Estado { get; set; } = string.Empty;

        public string? MedioRespuesta { get; set; }

        public string? Detalle { get; set; }

        public string? Respuesta { get; set; }

        public DateTime? FechaCierre { get; set; }

        public byte[] Version { get; set; } = [];
    }
}
    
