namespace PodoFlow.Api.DTOs.Solicitud
{
    public class AccionSolicitudRespuestaDto
    {
        public long Id { get; set; }

        public long SolicitudId { get; set; }

        public long UsuarioId { get; set; }

        public DateTime FechaHora { get; set; }

        public string? EstadoAnterior { get; set; }

        public string? EstadoNuevo { get; set; }

        public string Accion { get; set; } = string.Empty;

        public string? Observacion { get; set; }
    }
}
