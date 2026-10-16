namespace PodoFlow.Api.DTOs.EvaluacionImpacto
{
    public class EvaluacionImpactoRespuestaDto
    {
        public long Id { get; set; }

        public long UsuarioId { get; set; }

        public string Version { get; set; } = string.Empty;

        public DateOnly Fecha { get; set; }

        public string Alcance { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public string? Conclusion { get; set; }

        public byte? NivelResidual { get; set; }

        public string Estado { get; set; } = string.Empty;

        public DateTime? FechaModificacion { get; set; }
    }
}
