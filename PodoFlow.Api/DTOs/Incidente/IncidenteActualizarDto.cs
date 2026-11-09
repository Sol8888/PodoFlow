using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Incidente
{
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

        [Required]
        public byte[] Version { get; set; } = [];
    }
}
