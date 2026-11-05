using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Plantilla
{
    public class PlantillaCrearDto
    {
        [Required, MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        // CORREO o WHATSAPP (CK_PLANTILLA_canal).
        [Required, MaxLength(20)]
        public string Canal { get; set; } = string.Empty;

        // Ej.: RECORDATORIO_CITA, CONFIRMACION_CITA, FIDELIZACION. Se guarda en mayúsculas.
        [Required, MaxLength(50)]
        public string Tipo { get; set; } = string.Empty;

        // Obligatorio para CORREO. En WHATSAPP no se usa y se guarda NULL.
        [MaxLength(250)]
        public string? Asunto { get; set; }

        // NVARCHAR(MAX). En WHATSAPP, máximo 4096 caracteres.
        [Required]
        public string Contenido { get; set; } = string.Empty;

        // Si no se envía, "1.0" (mismo valor que DF_PLANTILLA_versi).
        [MaxLength(20)]
        public string? Version { get; set; }
    }
}
