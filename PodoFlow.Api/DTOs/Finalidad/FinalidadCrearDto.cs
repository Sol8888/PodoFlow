using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Finalidad
{
    public class FinalidadCrearDto
    {
        [Required, MaxLength(50)]
        public string Codigo { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Descripcion { get; set; }

        public bool RequiereConsentimiento { get; set; }

    }
}
