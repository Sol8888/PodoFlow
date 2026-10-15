
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Insumo;

public class InsumoActualizarDto
{
    [Required]
    [MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string Categoria { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string Unidad { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "9999999999.99")]
    public decimal StockMinimo { get; set; }

    [Required]
    public string Version { get; set; } = string.Empty;
}
