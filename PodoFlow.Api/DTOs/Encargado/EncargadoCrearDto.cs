using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Encargado
{
    public class EncargadoCrearDto
    {
        // Proveedor externo que trata datos por cuenta de la clínica (ej. hosting, mensajería).
        [Required, MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;

        // Qué servicio presta (ej. "Alojamiento en la nube", "Envío de WhatsApp").
        [Required, MaxLength(200)]
        public string Servicio { get; set; } = string.Empty;

        // Rol en el tratamiento
        [MaxLength(100)]
        public string? RolTratamiento { get; set; }

        [MaxLength(1500)]
        public string? Descripcion { get; set; }

        // Riesgos identificados de trabajar con este proveedor.
        [MaxLength(1000)]
        public string? Riesgo { get; set; }

        // Se puede guardar: contrato de encargo, cláusulas de confidencialidad.
        [MaxLength(2000)]
        public string? MedidasContractuales { get; set; }

        // Se puede guardar: cifrado, control de acceso, copias de seguridad.
        [MaxLength(2000)]
        public string? MedidasTecnicas { get; set; }
    }
}
