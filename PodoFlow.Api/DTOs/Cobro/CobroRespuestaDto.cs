
namespace PodoFlow.Api.DTOs.Cobro;

public class CobroRespuestaDto
{
    public long Id { get; set; }

    public long PrestacionId { get; set; }

    public int FormaPagoId { get; set; }

    public long UsuarioId { get; set; }

    public DateTime FechaPago { get; set; }

    public decimal Valor { get; set; }

    public string? Referencia { get; set; }

    public string? Observaciones { get; set; }

    public string Estado { get; set; } = "";

    public decimal TotalPrestacion { get; set; }

    public decimal TotalCobrado { get; set; }

    public decimal SaldoPendiente { get; set; }

    public string EstadoPrestacion { get; set; } = "";
}
