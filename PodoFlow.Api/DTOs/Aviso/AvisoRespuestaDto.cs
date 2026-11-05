namespace PodoFlow.Api.DTOs.Aviso
{
    public class AvisoRespuestaDto
    {
        public int Id { get; set; }

        public string Version { get; set; } = string.Empty;

        public DateOnly FechaDesde { get; set; }

        public DateOnly? FechaHasta { get; set; }

        public string? RutaArchivo { get; set; }

        public string? Huella { get; set; }

        public bool Estatus { get; set; }
    }
}
