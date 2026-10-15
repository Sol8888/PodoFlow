
namespace PodoFlow.Api.DTOs.Movimiento;

public class MovimientoRespuestaDto
{
    public long Id { get; set; }

    public int InsumoId { get; set; }

    public long? AtencionId { get; set; }

    public long UsuarioId { get; set; }

    public string Tipo { get; set; } = string.Empty;

    public decimal Cantidad { get; set; }

    public decimal StockAnterior { get; set; }

    public decimal StockNuevo { get; set; }

    public DateTime FechaHora { get; set; }

    public string Motivo { get; set; } = string.Empty;
}
