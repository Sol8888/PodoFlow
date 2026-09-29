using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Finalidad
{
    public class FinalidadActualizarDto
    {

        [Required, MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Descripcion { get; set; }

        public bool RequiereConsentimiento { get; set; }

        public bool Estatus { get; set; }

    }
}
