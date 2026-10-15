
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Cobro;

public class CobroCrearDto
{
    [Range(1, long.MaxValue)]
    public long PrestacionId { get; set; }

    [Range(1, int.MaxValue)]
    public int FormaPagoId { get; set; }

    [Range(1, long.MaxValue)]
    public long UsuarioId { get; set; }

    [Range(typeof(decimal), "0.01", "9999999999.99")]
    public decimal Valor { get; set; }

    [MaxLength(150)]
    public string? Referencia { get; set; }

    [MaxLength(1000)]
    public string? Observaciones { get; set; }
}
