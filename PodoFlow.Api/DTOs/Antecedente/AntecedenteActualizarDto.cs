
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Antecedente;

public class AntecedenteActualizarDto
{
    [Required, MaxLength(80)]
    public string Tipo { get; set; } = string.Empty;

    [Required, MaxLength(2000)]
    public string Descripcion { get; set; } = string.Empty;
}
