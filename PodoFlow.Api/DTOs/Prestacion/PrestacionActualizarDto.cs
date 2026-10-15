
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Prestacion;

public class PrestacionActualizarDto
{
    [Range(typeof(decimal), "0", "9999999999.99")]
    public decimal PrecioAplicado { get; set; }

    [Range(typeof(decimal), "0.01", "99999999.99")]
    public decimal Cantidad { get; set; } = 1m;

    [MaxLength(1000)]
    public string? Observaciones { get; set; }
}
