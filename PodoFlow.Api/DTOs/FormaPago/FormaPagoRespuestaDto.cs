
namespace PodoFlow.Api.DTOs.FormaPago;

public class FormaPagoRespuestaDto
{
    public int Id { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public bool Estatus { get; set; }
}
