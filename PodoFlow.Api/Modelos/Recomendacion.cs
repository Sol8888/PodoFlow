namespace PodoFlow.Api.Modelos
{
    public class Recomendacion
    {
        public long ReId { get; set; }

        public long EjId { get; set; }

        public string ReSegme { get; set; } = string.Empty;

        public string ReHallaz { get; set; } = string.Empty;

        public string ReSugere { get; set; } = string.Empty;

        public int RePacieEstim { get; set; }

        public string ReNivelPriva { get; set; } = string.Empty;

        public DateTime ReFechaGene { get; set; }
    }
}
