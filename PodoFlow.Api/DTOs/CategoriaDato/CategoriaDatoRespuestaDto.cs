namespace PodoFlow.Api.DTOs.CategoriaDato
{
    public class CategoriaDatoRespuestaDto
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public bool EsSensible { get; set; }

        public bool Estatus { get; set; }
    }
}
