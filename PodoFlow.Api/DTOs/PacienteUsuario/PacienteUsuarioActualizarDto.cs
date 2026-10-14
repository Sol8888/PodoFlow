
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.PacienteUsuario;

public class PacienteUsuarioActualizarDto
{
    [Range(1, long.MaxValue)]
    public long NuevoUsuarioId { get; set; }
}
