
namespace PodoFlow.Api.Modelos;

public class Paciente
{
    public long PaId { get; set; }
    public string PaTipoIdent { get; set; } = "";
    public string PaIdent { get; set; } = "";
    public string PaNombres { get; set; } = "";
    public string PaApelli { get; set; } = "";
    public DateTime? PaFechaNaci { get; set; }
    public string? PaGenero { get; set; }
    public string? PaDirec { get; set; }
    public DateTime PaFechaCrea { get; set; }
    public DateTime? PaFechaModi { get; set; }
    public DateTime? PaFechaBaja { get; set; }
    public string? PaMotivBaja { get; set; }
    public bool PaEstatus { get; set; } = true;
    public byte[] PaVersion { get; set; } = [];
}
