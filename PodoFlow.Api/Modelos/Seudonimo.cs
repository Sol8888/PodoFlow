namespace PodoFlow.Api.Modelos
{
    public class Seudonimo
    {
        public Guid SeId { get; set; }

        public long PaId { get; set; }

        public DateTime SeFechaCrea { get; set; }

        public bool SeEstatus { get; set; }
    }
}
