using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.TratamientoDatos
{
    public class TratamientoDatosCrearDto
    {
        [Required, MaxLength(50)]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        public int FinalidadId { get; set; }

        public long? UsuarioId { get; set; }

        [Required, MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [Required, MaxLength(80)]
        public string Modulo { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Descripcion { get; set; }

        [MaxLength(500)]
        public string? CategoriasTitulares { get; set; }

        [MaxLength(1000)]
        public string? BaseLegitimacion { get; set; }

        [MaxLength(500)]
        public string? PlazoConservacion { get; set; }

        [MaxLength(2000)]
        public string? MedidasSeguridad { get; set; }
        public DateOnly? FechaRevision { get; set; }
    }
}
