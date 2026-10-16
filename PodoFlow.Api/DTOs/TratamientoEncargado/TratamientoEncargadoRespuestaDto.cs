namespace PodoFlow.Api.DTOs.TratamientoEncargado
{
    public class TratamientoEncargadoRespuestaDto
    {
        public int TratamientoId { get; set; }

        public int EncargadoId { get; set; }

        public DateTime FechaAsignacion { get; set; }

        public bool Estatus { get; set; }
    }
}
