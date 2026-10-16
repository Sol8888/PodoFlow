using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Notificacion
{
    public class NotificacionActualizarDto
    {
        [Range(1, int.MaxValue)]
        public int PlantillaId { get; set; }

        [MaxLength(50)]
        public string? Tipo { get; set; }

        [Required]
        public DateTime? FechaProgramada { get; set; }

        // no_version leída (base64 en Swagger).
        [Required]
        public byte[] Version { get; set; } = [];
    }
}
