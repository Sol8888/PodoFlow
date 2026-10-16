namespace PodoFlow.Api.DTOs.TratamientoCategoria
{
    public class TratamientoCategoriaRespuestaDto
    {
        public int TratamientoId { get; set; }

        public int CategoriaId { get; set; }

        public DateTime FechaAsignacion { get; set; }

        public bool Estatus { get; set; }
    }
}
