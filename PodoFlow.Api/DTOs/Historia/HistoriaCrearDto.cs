
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Historia;

public class HistoriaCrearDto
{
    [Range(1, long.MaxValue)]
    public long PacienteId { get; set; }

    public long? UsuarioId { get; set; }
}
