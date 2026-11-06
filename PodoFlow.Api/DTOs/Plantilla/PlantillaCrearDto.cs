using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Plantilla
{
    public class PlantillaCrearDto
    {
        [Required, MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Canal { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Tipo { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Asunto { get; set; }

        [Required]
        public string Contenido { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Version { get; set; }
    }
}
