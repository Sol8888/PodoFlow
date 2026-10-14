
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Paciente;

public class PacienteCrearDto
{
    [Required, MaxLength(20)]
    public string TipoIdentificacion { get; set; } = "";

    [Required, MaxLength(30)]
    public string Identificacion { get; set; } = "";

    [Required, MaxLength(100)]
    public string Nombres { get; set; } = "";

    [Required, MaxLength(100)]
    public string Apellidos { get; set; } = "";

    public DateTime? FechaNacimiento { get; set; }

    [MaxLength(30)]
    public string? Genero { get; set; }

    [MaxLength(300)]
    public string? Direccion { get; set; }
}
