namespace PodoFlow.Api.Modelos
{
    public class Ejecucion
    {
        public long EjId { get; set; }

        public long UsId { get; set; }

        public int? EnId { get; set; }

        public int FiId { get; set; }

        public DateTime EjFechaHora { get; set; }

        public string EjEstad { get; set; } = string.Empty;

        public int EjCantReg { get; set; }

        public string? EjHuellEntr { get; set; }

        public string? EjHuellSali { get; set; }

        public string? EjObser { get; set; }
    }
}
