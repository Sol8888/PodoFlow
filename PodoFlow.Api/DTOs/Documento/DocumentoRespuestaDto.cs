namespace PodoFlow.Api.DTOs.Documento
{
    public class DocumentoRespuestaDto
    {
        public long Id { get; set; }

        public long UsuarioId { get; set; }

        public string Tipo { get; set; } = string.Empty;

        public string? Version { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string RutaArchivo { get; set; } = string.Empty;

        public string? Huella { get; set; }

        public DateTime FechaCarga { get; set; }

        public bool Estatus { get; set; }
    }
}
