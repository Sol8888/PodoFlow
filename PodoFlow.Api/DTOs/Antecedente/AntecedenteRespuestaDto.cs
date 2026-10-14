
namespace PodoFlow.Api.DTOs.Antecedente;

public class AntecedenteRespuestaDto
{
    public long Id { get; set; }

    public long HistoriaId { get; set; }

    public long? UsuarioId { get; set; }

    public string Tipo { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public DateTime FechaRegistro { get; set; }

    public bool Estatus { get; set; }
}
