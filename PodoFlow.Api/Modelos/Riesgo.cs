namespace PodoFlow.Api.Modelos
{
    public class Riesgo
    {
        public long RiId { get; set; }

        public int TrId { get; set; }

        public string RiDescri { get; set; } = string.Empty;

        public byte RiProba { get; set; }

        public byte RiImpac { get; set; }

        public byte RiNivel { get; set; }

        public string? RiMediMitig { get; set; }

        public byte? RiRiesgResid { get; set; }

        public string RiEstad { get; set; } = string.Empty;

        public DateOnly? RiFechaRevi { get; set; }
    }
}
