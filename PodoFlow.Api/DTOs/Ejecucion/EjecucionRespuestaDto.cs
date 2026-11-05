namespace PodoFlow.Api.DTOs.Ejecucion
{
    public class EjecucionRespuestaDto
    {
        public long Id { get; set; }

        public long UsuarioId { get; set; }

        public int? EncargadoId { get; set; }

        public int FinalidadId { get; set; }

        public DateTime FechaHora { get; set; }

        public string Estado { get; set; } = string.Empty;

        public int CantidadRegistros { get; set; }

        public string? HuellaEntrada { get; set; }

        public string? HuellaSalida { get; set; }

        public string? Observacion { get; set; }
    }
}
