namespace PodoFlow.Api.DTOs.Notificacion
{
    public class ResultadoProveedorDto
    {
        public bool Exitoso { get; set; }

        public string? IdProveedor { get; set; }

        public string? CodigoError { get; set; }

        public string? DetalleError { get; set; }
    }
}
