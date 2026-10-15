using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Notificacion
{
    // Cambios manuales: PROCESANDO (el proceso de envío la toma), CANCELADA (la baja)
    // y PENDIENTE (reintentar después de un ERROR).
    // ENVIADA y ERROR no se ponen aquí: salen de registrar el intento en POST {id}/envios.
    public class NotificacionCambiarEstadoDto
    {
        [Required, MaxLength(20)]
        public string Estado { get; set; } = string.Empty;

        [Required]
        public byte[] Version { get; set; } = [];
    }
}
