
namespace PodoFlow.Api.DTOs.DatoContacto;

public class DatoContactoRespuestaDto
{
    public long Id { get; set; }
    public long PacienteId { get; set; }
    public string Tipo { get; set; } = "";
    public string Valor { get; set; } = "";
    public bool Principal { get; set; }
    public bool Verificado { get; set; }
    public DateTime FechaCreacion { get; set; }
    public bool Estatus { get; set; }
}
