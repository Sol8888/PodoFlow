
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.PacienteUsuario;

public class PacienteUsuarioCrearDto
{
    [Range(1, long.MaxValue)]
    public long PacienteId { get; set; }

    [Range(1, long.MaxValue)]
    public long UsuarioId { get; set; }
}
