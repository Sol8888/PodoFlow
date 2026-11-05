using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Notificacion
{
    public class NotificacionCrearDto
    {
        [Range(1, long.MaxValue)]
        public long OpcionId { get; set; }

        [Range(1, int.MaxValue)]
        public int PlantillaId { get; set; }
        public long? CitaId { get; set; }
        public long? UsuarioId { get; set; }

        [MaxLength(50)]
        public string? Tipo { get; set; }

        [Required]
        public DateTime? FechaProgramada { get; set; }
    }
}
