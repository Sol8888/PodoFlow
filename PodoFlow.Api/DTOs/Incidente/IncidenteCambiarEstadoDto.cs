using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Incidente
{
    public class IncidenteCambiarEstadoDto
    {
        // ABIERTO, ANALISIS, CONTENIDO, RECUPERADO o CERRADO.
        [Required, MaxLength(20)]
        public string Estado { get; set; } = string.Empty;

        // Obligatorio: ACCION_INCIDENTE.us_id es NOT NULL (todo cambio tiene un responsable).
        [Range(1, long.MaxValue)]
        public long UsuarioId { get; set; }

        // Descripción de lo hecho. Si no se envía, se genera "Cambio de estado de X a Y".
        [MaxLength(2000)]
        public string? Accion { get; set; }

        // Obligatoria al pasar a CERRADO (resumen del cierre o motivo si se descartó).
        [MaxLength(1500)]
        public string? Observacion { get; set; }

        [Required]
        public byte[] Version { get; set; } = [];
    }
}
