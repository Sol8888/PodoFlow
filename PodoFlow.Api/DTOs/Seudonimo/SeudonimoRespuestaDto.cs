namespace PodoFlow.Api.DTOs.Seudonimo
{
    public class SeudonimoRespuestaDto
    {
        public Guid Id { get; set; }

        public long PacienteId { get; set; }

        public DateTime FechaCreacion { get; set; }

        public bool Estatus { get; set; }
    }
}
