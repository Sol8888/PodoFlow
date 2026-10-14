
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Movimiento;

public class MovimientoCrearDto
{
    [Range(1, int.MaxValue)]
    public int InsumoId { get; set; }

    public long? AtencionId { get; set; }

    [Range(1, long.MaxValue)]
    public long UsuarioId { get; set; }

    [Required]
    [MaxLength(20)]
    public string Tipo { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "9999999999.99")]
    public decimal Cantidad { get; set; }

    [Required]
    [MaxLength(500)]
    public string Motivo { get; set; } = string.Empty;
}
