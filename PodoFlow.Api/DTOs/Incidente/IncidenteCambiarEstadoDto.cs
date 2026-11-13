using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Incidente
{
    public class IncidenteCambiarEstadoDto
    {
        [Required, MaxLength(20)]
        public string Estado { get; set; } = string.Empty;

        [Range(1, long.MaxValue)]
        public long UsuarioId { get; set; }

        [MaxLength(2000)]
        public string? Accion { get; set; }

        [MaxLength(1500)]
        public string? Observacion { get; set; }

        [Required]
        public byte[] Version { get; set; } = [];
    }
}
