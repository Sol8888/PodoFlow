namespace PodoFlow.Api.Modelos
{
    public class Encargado
    {
        public int EnId { get; set; }

        public string EnNombre { get; set; } = string.Empty;

        public string EnServi { get; set; } = string.Empty;

        public string? EnRolTrata { get; set; }

        public string? EnDescri { get; set; }

        public string? EnRiesgo { get; set; }

        public string? EnMediContr { get; set; }

        public string? EnMediTecni { get; set; }

        public bool EnEstatus { get; set; }
    }
}
