
namespace PodoFlow.Api.DTOs.Prestacion;

public class PrestacionRespuestaDto
{
    public long Id { get; set; }

    public long PacienteId { get; set; }

    public long? CitaId { get; set; }

    public long? AtencionId { get; set; }

    public int ServicioId { get; set; }

    public long UsuarioId { get; set; }

    public DateTime FechaPrestacion { get; set; }

    public decimal PrecioAplicado { get; set; }

    public decimal Cantidad { get; set; }

    public decimal Total { get; set; }

    public string Estado { get; set; } = "";

    public string? Observaciones { get; set; }
}
