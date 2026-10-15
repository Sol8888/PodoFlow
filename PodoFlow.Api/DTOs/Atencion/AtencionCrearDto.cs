
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Atencion;

public class AtencionCrearDto
{
    [Range(1, long.MaxValue)]
    public long HistoriaId { get; set; }

    public long? CitaId { get; set; }

    [Range(1, long.MaxValue)]
    public long UsuarioId { get; set; }

    [MaxLength(1000)]
    public string? MotivoConsulta { get; set; }

    [MaxLength(3000)]
    public string? ValoracionClinica { get; set; }

    [MaxLength(2000)]
    public string? Diagnostico { get; set; }

    [MaxLength(3000)]
    public string? Tratamiento { get; set; }

    [MaxLength(3000)]
    public string? Observaciones { get; set; }
}
