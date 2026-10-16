using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Plantilla
{
    // Sin Estatus: la baja y la reactivación tienen sus propios endpoints.
    // Solo se puede editar mientras ninguna notificación use la plantilla;
    // después hay que crear una versión nueva para no cambiar lo que ya se envió.
    public class PlantillaActualizarDto
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
