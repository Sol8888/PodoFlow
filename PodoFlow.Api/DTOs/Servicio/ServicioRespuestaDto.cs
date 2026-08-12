namespace PodoFlow.Api.DTOs.Servicio;

public class ServicioRespuestaDto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public decimal Precio { get; set; }

    public short DuracionMinutos { get; set; }

    public bool Estatus { get; set; }
}
