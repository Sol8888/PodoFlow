using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Aviso
{
    public class AvisoActualizarDto
    {
        [Required]
        public DateOnly FechaDesde { get; set; }

        public DateOnly? FechaHasta { get; set; }

        [MaxLength(1000)]
        public string? RutaArchivo { get; set; }

        [MaxLength(128)]
        public string? Huella { get; set; }

        public bool Estatus { get; set; }
    }
}
