using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Ejecucion
{
    public class EjecucionActualizarDto
    {
        [Range(1, int.MaxValue)]
        public int? EncargadoId { get; set; }

        [MaxLength(1500)]
        public string? Observacion { get; set; }
    }
}
