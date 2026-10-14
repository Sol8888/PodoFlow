
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.DatoContacto;

public class DatoContactoActualizarDto
{
    [Required, MaxLength(20)]
    public string Tipo { get; set; } = "";

    [Required, MaxLength(320)]
    public string Valor { get; set; } = "";

    public bool Principal { get; set; }
}
