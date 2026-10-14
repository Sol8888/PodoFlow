using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Exportacion
{
    public class ExportacionCambiarEstadoDto
    {

        [Required, MaxLength(20)]
        public string Estado { get; set; } = string.Empty;

        [Required]
        public long UsuarioId { get; set; }

        [MaxLength(2000)]
        public string? Observacion { get; set; }
    }
}
