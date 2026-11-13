using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.OpcionComunicacion
{
    public class OpcionComunicacionCrearDto
    {
        [Range(1, long.MaxValue)]
        public long PacienteId { get; set; }

        [Range(1, int.MaxValue)]
        public int FinalidadId { get; set; }

        [Required, MaxLength(20)]
        public string Canal { get; set; } = string.Empty;

        [Required]
        public bool? Permitido { get; set; }
    }
}
