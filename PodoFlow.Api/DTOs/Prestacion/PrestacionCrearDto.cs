
using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Prestacion;

public class PrestacionCrearDto
{
    [Range(1, long.MaxValue)]
    public long PacienteId { get; set; }

    public long? CitaId { get; set; }

    public long? AtencionId { get; set; }

    [Range(1, int.MaxValue)]
    public int ServicioId { get; set; }

    [Range(1, long.MaxValue)]
    public long UsuarioId { get; set; }

    [Range(typeof(decimal), "0", "9999999999.99")]
    public decimal PrecioAplicado { get; set; }

    [Range(typeof(decimal), "0.01", "99999999.99")]
    public decimal Cantidad { get; set; } = 1m;

    [MaxLength(1000)]
    public string? Observaciones { get; set; }
}
