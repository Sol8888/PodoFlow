namespace PodoFlow.Api.Modelos
{
    public class Plantilla
    {
        public int PlId { get; set; }

        public string PlNombre { get; set; } = string.Empty;

        public string PlCanal { get; set; } = string.Empty;

        public string PlTipo { get; set; } = string.Empty;

        public string? PlAsunto { get; set; }

        public string PlConte { get; set; } = string.Empty;

        public string PlVersi { get; set; } = string.Empty;

        public DateTime PlFechaCrea { get; set; }

        public bool PlEstatus { get; set; }
    }
}
