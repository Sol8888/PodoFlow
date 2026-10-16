using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Notificacion
{
    public class NotificacionCambiarEstadoDto
    {
        [Required, MaxLength(20)]
        public string Estado { get; set; } = string.Empty;

        [Required]
        public byte[] Version { get; set; } = [];
    }
}
