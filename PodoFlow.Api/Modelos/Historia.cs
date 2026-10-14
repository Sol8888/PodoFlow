
namespace PodoFlow.Api.Modelos;

public class Historia
{
    public long HiId { get; set; }

    public long PaId { get; set; }

    public long? UsId { get; set; }

    public DateTime HiFechaAper { get; set; }

    public string HiEstad { get; set; } = "ACTIVA";

    public DateTime? HiFechaBloq { get; set; }

    public string? HiMotivBloq { get; set; }

    public byte[] HiVersion { get; set; } = [];
}
