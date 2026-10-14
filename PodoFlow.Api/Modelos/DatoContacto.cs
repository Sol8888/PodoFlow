
namespace PodoFlow.Api.Modelos;

public class DatoContacto
{
    public long DaId { get; set; }
    public long PaId { get; set; }
    public string DaTipo { get; set; } = "";
    public string DaValor { get; set; } = "";
    public bool DaPrinc { get; set; }
    public bool DaVerif { get; set; }
    public DateTime DaFechaCrea { get; set; }
    public bool DaEstatus { get; set; } = true;
}
