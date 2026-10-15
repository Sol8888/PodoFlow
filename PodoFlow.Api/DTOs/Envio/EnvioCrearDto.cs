using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Notificacion
{
    // Resultado de un intento de entrega. Lo registra el proceso de envío.
    // El número de intento no se envía: se calcula (último + 1).
    public class EnvioCrearDto
    {
        // ENVIADO o ERROR (CK_ENVIO_result).
        [Required, MaxLength(20)]
        public string Resultado { get; set; } = string.Empty;

        // Id que devuelve el proveedor (ej. el id del mensaje de WhatsApp o del correo).
        [MaxLength(300)]
        public string? IdProveedor { get; set; }

        // Si el resultado es ERROR, hay que enviar el código o el detalle.
        [MaxLength(100)]
        public string? CodigoError { get; set; }

        [MaxLength(2000)]
        public string? DetalleError { get; set; }
    }
}
