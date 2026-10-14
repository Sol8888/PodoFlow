
namespace PodoFlow.Api.Modelos;

public class HorarioAtencion
{
    public int HoId { get; set; }

    public long? UsId { get; set; }

    public byte HoDiaSem { get; set; }

    public TimeSpan HoHoraIni { get; set; }

    public TimeSpan HoHoraFin { get; set; }

    public DateTime? HoFechaDesd { get; set; }

    public DateTime? HoFechaHast { get; set; }

    public bool HoEstatus { get; set; } = true;
}
