namespace PodoFlow.Api.DTOs.Ejecucion
{
    public class DatoAnaliticoRespuestaDto
    {
        public long Id { get; set; }

        public long EjecucionId { get; set; }

        public Guid SeudonimoId { get; set; }

        public int FrecuenciaAtenciones { get; set; }

        public string? CategoriaServicio { get; set; }

        public DateOnly? FechaUltimaAtencion { get; set; }

        public int? DiasInactividad { get; set; }
    }
}
