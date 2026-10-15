namespace PodoFlow.Api.DTOs.OpcionComunicacion
{
    public class OpcionComunicacionRespuestaDto
    {
        public long Id { get; set; }

        public long PacienteId { get; set; }

        public int FinalidadId { get; set; }

        public long? ConsentimientoId { get; set; }

        public string Canal { get; set; } = string.Empty;

        public bool Permitido { get; set; }

        public DateTime FechaActualizacion { get; set; }

        public bool Estatus { get; set; }
    }
}
