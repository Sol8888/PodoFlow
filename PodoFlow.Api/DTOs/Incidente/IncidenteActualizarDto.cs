using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Incidente
{
    // Corrige o completa los datos del incidente mientras no esté CERRADO.
    // No cambia el usuario que lo registró, el estado (va por su endpoint) ni la fecha de cierre.
    public class IncidenteActualizarDto
    {
        [Required]
        public DateTime? FechaDeteccion { get; set; }

        [Required, MaxLength(100)]
        public string Tipo { get; set; } = string.Empty;

        [Required, MaxLength(80)]
        public string Modulo { get; set; } = string.Empty;

        [Required, MaxLength(3000)]
        public string Descripcion { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? DatosAfectados { get; set; }

        [MaxLength(1500)]
        public string? ImpactoPreliminar { get; set; }

        // in_version leída (base64 en Swagger).
        [Required]
        public byte[] Version { get; set; } = [];
    }
}
