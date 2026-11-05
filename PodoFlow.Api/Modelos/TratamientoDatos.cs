namespace PodoFlow.Api.Modelos
{
    public class TratamientoDatos
    {
        public int TrId { get; set; }

        public int FiId { get; set; }

        public long? UsId { get; set; }

        public string TrCodigo { get; set; } = string.Empty;

        public string TrNombre { get; set; } = string.Empty;

        public string TrModulo { get; set; } = string.Empty;

        public string? TrDescri { get; set; }

        public string? TrCategTitu { get; set; }

        public string? TrBaseLegit { get; set; }

        public string? TrPlazoConse { get; set; }

        public string? TrMediSegur { get; set; }

        public DateOnly? TrFechaRevi { get; set; }

        public bool TrEstatus { get; set; }
    }
}
