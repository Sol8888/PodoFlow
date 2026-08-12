using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Servicio;

public class ServicioCrearDto
{
    [Required]
    [MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Descripcion { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Precio { get; set; }

    [Range(1, short.MaxValue)]
    public short DuracionMinutos { get; set; }
}
