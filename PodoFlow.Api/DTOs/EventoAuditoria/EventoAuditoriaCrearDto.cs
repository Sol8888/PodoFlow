
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.EventoAuditoria;

public class EventoAuditoriaCrearDto
{
    public long? UsuarioId { get; set; }

    public long? PacienteId { get; set; }

    [Required]
    [MaxLength(80)]
    public string Modulo { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string Accion { get; set; } = string.Empty;

    [MaxLength(128)]
    public string? Entidad { get; set; }

    [MaxLength(100)]
    public string? EntidadId { get; set; }

    [Required]
    [RegularExpression("CORRECTO|ERROR|DENEGADO")]
    public string Resultado { get; set; } = string.Empty;

    [MaxLength(45)]
    public string? DireccionIp { get; set; }

    [MaxLength(500)]
    public string? AgenteUsuario { get; set; }

    public string? MetadatosJson { get; set; }
}
