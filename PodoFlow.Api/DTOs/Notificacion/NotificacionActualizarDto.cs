using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Notificacion
{
    // Reprogramar o cambiar de plantilla mientras esté PENDIENTE o en ERROR.
    // No cambia el paciente, la opción, la cita ni el canal: eso sería otra notificación.
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
