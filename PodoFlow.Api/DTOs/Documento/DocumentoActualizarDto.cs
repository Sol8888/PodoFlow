using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Documento
{
    // Solo datos descriptivos. Sin RutaArchivo ni Huella: si cambia el archivo,
    // se carga como documento nuevo (otra versión), para no perder el original.
    // Sin Estatus: la baja y la reactivación tienen sus propios endpoints.
    public class DocumentoActualizarDto
    {
        [Required, MaxLength(50)]
        public string Tipo { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? Version { get; set; }

        [Required, MaxLength(255)]
        public string Nombre { get; set; } = string.Empty;
    }
}
