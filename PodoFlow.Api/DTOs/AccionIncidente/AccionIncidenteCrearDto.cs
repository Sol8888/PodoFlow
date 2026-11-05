using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Incidente
{
    public class AccionIncidenteCrearDto
    {
        [Range(1, long.MaxValue)]
        public long UsuarioId { get; set; }

        [Required, MaxLength(50)]
        public string Tipo { get; set; } = string.Empty;

        [Required, MaxLength(2000)]
        public string Accion { get; set; } = string.Empty;

        [MaxLength(1500)]
        public string? Observacion { get; set; }
    }
}
