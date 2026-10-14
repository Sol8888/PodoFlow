using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Solicitud
{
    public class SolicitudActualizarDto
    {
        [Required, MaxLength(200)]
        public string NombreSolicitante { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? TipoIdentificacion { get; set; }

        [Required, MaxLength(50)]
        public string IdentificacionSolicitante { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? MedioRespuesta { get; set; }

        [MaxLength(3000)]
        public string? Detalle { get; set; }

        [Required]
        public byte[] Version { get; set; } = []; 
    }
}
