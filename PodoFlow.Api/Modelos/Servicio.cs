namespace PodoFlow.Api.Modelos
{
    public class Servicio
    {
        public int SeId { get; set; }

        public string SeNombre { get; set; } = string.Empty;

        public string? SeDescri { get; set; }

        public decimal SePrecio { get; set; }

        public short SeDuracMin { get; set; }

        public DateTime SeFechaCrea { get; set; }

        public DateTime? SeFechaModi { get; set; }

        public bool SeEstatus { get; set; }

        public byte[] SeVersion { get; set; } = [];
    }
}
