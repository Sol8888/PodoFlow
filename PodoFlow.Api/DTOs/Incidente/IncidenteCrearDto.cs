using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Incidente
{
    // El estado no se envía: todo incidente nace ABIERTO.
    public class IncidenteCrearDto
    {
        // Quién registra el incidente. Obligatorio: us_id es NOT NULL.
        [Range(1, long.MaxValue)]
        public long UsuarioId { get; set; }

        // Cuándo se detectó (puede ser antes del registro). No puede estar en el futuro.
        // Es DateTime? con [Required] para que, si no se envía, responda 400 en vez de guardar 0001-01-01.
        [Required]
        public DateTime? FechaDeteccion { get; set; }

        // Ej.: ACCESO_NO_AUTORIZADO, PERDIDA_DISPOSITIVO, ENVIO_ERRONEO, MALWARE. Se guarda en mayúsculas.
        [Required, MaxLength(100)]
        public string Tipo { get; set; } = string.Empty;

        // Módulo afectado. Ej.: PACIENTES, HISTORIA, AGENDA. Se guarda en mayúsculas.
        [Required, MaxLength(80)]
        public string Modulo { get; set; } = string.Empty;

        [Required, MaxLength(3000)]
        public string Descripcion { get; set; } = string.Empty;

        // Qué datos personales se vieron comprometidos (si ya se sabe).
        [MaxLength(2000)]
        public string? DatosAfectados { get; set; }

        [MaxLength(1500)]
        public string? ImpactoPreliminar { get; set; }
    }
}
