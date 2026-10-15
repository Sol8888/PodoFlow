
namespace PodoFlow.Api.Modelos;

public class FormaPago
{
    public int FoId { get; set; }

    public string FoCodigo { get; set; } = string.Empty;

    public string FoNombre { get; set; } = string.Empty;

    public bool FoEstatus { get; set; } = true;
}
