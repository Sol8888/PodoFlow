
namespace PodoFlow.Api.DTOs.Insumo;

public class InsumoRespuestaDto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Categoria { get; set; } = string.Empty;

    public string Unidad { get; set; } = string.Empty;

    public decimal StockActual { get; set; }

    public decimal StockMinimo { get; set; }

    public bool BajoMinimo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Estatus { get; set; }

    public string Version { get; set; } = string.Empty;
}
