
namespace PodoFlow.Api.Modelos;

public class PacienteUsuario
{
    public long PaId { get; set; }
    public long UsId { get; set; }
    public DateTime PuFechaAsig { get; set; }
    public bool PuEstatus { get; set; } = true;
}
