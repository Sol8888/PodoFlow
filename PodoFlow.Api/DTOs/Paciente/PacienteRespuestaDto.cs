
namespace PodoFlow.Api.DTOs.Paciente;

public class PacienteRespuestaDto
{
    public long Id { get; set; }
    public string TipoIdentificacion { get; set; } = "";
    public string Identificacion { get; set; } = "";
    public string Nombres { get; set; } = "";
    public string Apellidos { get; set; } = "";
    public DateTime? FechaNacimiento { get; set; }
    public string? Genero { get; set; }
    public string? Direccion { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public DateTime? FechaBaja { get; set; }
    public string? MotivoBaja { get; set; }
    public bool Estatus { get; set; }
    public string Version { get; set; } = "";
}
