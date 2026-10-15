using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Incidente
{
    // Acción de gestión que no cambia el estado (contener, notificar, investigar...).
    // Los cambios de estado se registran solos desde PATCH api/Incidente/{id}/estado.
    public class AccionIncidenteCrearDto
    {
        [Range(1, long.MaxValue)]
        public long UsuarioId { get; set; }

        // Ej.: CONTENCION, INVESTIGACION, NOTIFICACION_AUTORIDAD, NOTIFICACION_TITULAR,
        // RECUPERACION, OTRO. Se guarda en mayúsculas. REGISTRO y CAMBIO_ESTADO están reservados.
        [Required, MaxLength(50)]
        public string Tipo { get; set; } = string.Empty;

        [Required, MaxLength(2000)]
        public string Accion { get; set; } = string.Empty;

        [MaxLength(1500)]
        public string? Observacion { get; set; }
    }
}
