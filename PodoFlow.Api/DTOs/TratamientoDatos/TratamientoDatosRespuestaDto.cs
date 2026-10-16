namespace PodoFlow.Api.DTOs.TratamientoDatos
{
    public class TratamientoDatosRespuestaDto
    {
        public int Id { get; set; }

        public int FinalidadId { get; set; }

        public long? UsuarioId { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string Modulo { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public string? CategoriasTitulares { get; set; }

        public string? BaseLegitimacion { get; set; }

        public string? PlazoConservacion { get; set; }

        public string? MedidasSeguridad { get; set; }

        public DateOnly? FechaRevision { get; set; }

        public bool Estatus { get; set; }
    }
}
