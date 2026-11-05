namespace PodoFlow.Api.DTOs.Plantilla
{
    public class PlantillaRespuestaDto
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Canal { get; set; } = string.Empty;

        public string Tipo { get; set; } = string.Empty;

        public string? Asunto { get; set; }

        public string Contenido { get; set; } = string.Empty;

        public string Version { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; }

        public bool Estatus { get; set; }
    }
}
