
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.FormaPago;

public class FormaPagoCrearDto
{
    [Required, MaxLength(30)]
    public string Codigo { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;
}
