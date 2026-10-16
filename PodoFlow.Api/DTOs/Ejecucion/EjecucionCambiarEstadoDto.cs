using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Ejecucion
{
    public class EjecucionCambiarEstadoDto
    {
        [Required, MaxLength(20)]
        public string Estado { get; set; } = string.Empty;

        [MaxLength(128)]
        public string? HuellaEntrada { get; set; }

        [MaxLength(128)]
        public string? HuellaSalida { get; set; }

        [MaxLength(1500)]
        public string? Observacion { get; set; }
    }
}
