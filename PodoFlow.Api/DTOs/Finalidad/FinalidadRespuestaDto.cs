namespace PodoFlow.Api.DTOs.Finalidad
{
    public class FinalidadRespuestaDto
    {
        public int Id { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public bool RequiereConsentimiento { get; set; }

        public bool Estatus { get; set; }
    }
}
