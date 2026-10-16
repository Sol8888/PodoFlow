namespace PodoFlow.Api.DTOs.Riesgo
{
    public class RiesgoRespuestaDto
    {
        public long Id { get; set; }

        public int TratamientoId { get; set; }

        public string Descripcion { get; set; } = string.Empty;

        public byte Probabilidad { get; set; }

        public byte Impacto { get; set; }

        public byte Nivel { get; set; }

        public string? MedidasMitigacion { get; set; }

        public byte? RiesgoResidual { get; set; }

        public string Estado { get; set; } = string.Empty;

        public DateOnly? FechaRevision { get; set; }
    }
}
