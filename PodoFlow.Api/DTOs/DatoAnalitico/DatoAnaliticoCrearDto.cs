using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Ejecucion
{
    public class DatoAnaliticoCrearDto
    {
        [Required]
        public Guid? SeudonimoId { get; set; }

        [Range(0, int.MaxValue)]
        public int FrecuenciaAtenciones { get; set; }

        [MaxLength(100)]
        public string? CategoriaServicio { get; set; }

        public DateOnly? FechaUltimaAtencion { get; set; }

        [Range(0, int.MaxValue)]
        public int? DiasInactividad { get; set; }
    }
}
