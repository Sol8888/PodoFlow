using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Notificacion
{
    public class EnvioCrearDto
    {
        [Required, MaxLength(20)]
        public string Resultado { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? IdProveedor { get; set; }

        [MaxLength(100)]
        public string? CodigoError { get; set; }

        [MaxLength(2000)]
        public string? DetalleError { get; set; }
    }
}
