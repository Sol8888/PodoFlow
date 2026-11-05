using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Solicitud
{
    public class SolicitudCambiarEstadoDto
    {
        [Required, MaxLength(20)]
        public string Estado { get; set; } = string.Empty;

        [Required]
        public long UsuarioId { get; set; }

        [MaxLength(1000)]
        public string? Accion { get; set; }

        [MaxLength(2000)]
        public string? Observacion { get; set; }

        [MaxLength(3000)]
        public string? Respuesta { get; set; }

        [MaxLength(50)]
        public string? MedioRespuesta { get; set; }

        [Required]
        public byte[] Version { get; set; } = [];
    }
}