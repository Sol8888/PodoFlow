using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Ejecucion
{
    public class EjecucionCrearDto
    {
        [Range(1, long.MaxValue)]
        public long UsuarioId { get; set; }

        [Range(1, int.MaxValue)]
        public int? EncargadoId { get; set; }

        [MaxLength(1500)]
        public string? Observacion { get; set; }
    }
}
