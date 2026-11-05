using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Solicitud
{
    public class SolicitudCrearDto
    {
        [Required]
        public long PacienteId { get; set; }
        public long? UsuarioId { get; set; }

        // INFORMACION, ACCESO, CONSULTA, RECTIFICACION, ACTUALIZACION,
        // ELIMINACION, OPOSICION, SUSPENSION, PORTABILIDAD
        [Required, MaxLength(25)]
        public string TipoDerecho { get; set; } = string.Empty;

        // TITULAR o REPRESENTANTE
        [MaxLength(20)]
        public string TipoSolicitante { get; set; } = "TITULAR";

        [Required, MaxLength(200)]
        public string NombreSolicitante { get; set; } = string.Empty;
        [MaxLength(20)]
        public string? TipoIdentificacion { get; set; }

        [Required, MaxLength(50)]
        public string IdentificacionSolicitante { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? MedioRespuesta { get; set; }

        [MaxLength(3000)]
        public string? Detalle { get; set; }
    }
}
