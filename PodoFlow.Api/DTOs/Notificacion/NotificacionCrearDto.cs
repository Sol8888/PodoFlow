using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Notificacion
{
    // El paciente y el canal no se envían: salen de la opción de comunicación,
    // que es la que dice para qué finalidad es el mensaje y si el paciente lo permite.
    // El estado tampoco: toda notificación nace PENDIENTE.
    public class NotificacionCrearDto
    {
        // Obligatoria en la API (en la tabla admite NULL): sin ella no se sabe la finalidad
        // ni se puede verificar el permiso y el consentimiento.
        [Range(1, long.MaxValue)]
        public long OpcionId { get; set; }

        [Range(1, int.MaxValue)]
        public int PlantillaId { get; set; }

        // Para recordatorios ligados a una cita. Debe ser del mismo paciente y estar vigente.
        public long? CitaId { get; set; }

        // Quién la programó. Opcional: NULL = la programó el sistema.
        public long? UsuarioId { get; set; }

        // Si no se envía, se usa el tipo de la plantilla. Se guarda en mayúsculas.
        [MaxLength(50)]
        public string? Tipo { get; set; }

        // Cuándo debe salir. No puede estar en el pasado y, si hay cita, debe ser antes de ella.
        [Required]
        public DateTime? FechaProgramada { get; set; }
    }
}
