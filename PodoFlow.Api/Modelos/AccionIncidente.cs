namespace PodoFlow.Api.Modelos
{
    public class AccionIncidente
    {
        public long AcId { get; set; }

        public long InId { get; set; }

        public long UsId { get; set; }

        public DateTime AcFechaHora { get; set; }

        public string AcTipo { get; set; } = string.Empty;

        public string AcAccion { get; set; } = string.Empty;

        public string? AcObser { get; set; }

        // Permite insertar la acción en el mismo SaveChanges que un incidente nuevo.
        public Incidente? Incidente { get; set; }
    }
}
