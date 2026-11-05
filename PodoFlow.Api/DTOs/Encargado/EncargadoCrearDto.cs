using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Encargado
{
    public class EncargadoCrearDto
    {
        [Required, MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;
        [Required, MaxLength(200)]
        public string Servicio { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? RolTratamiento { get; set; }

        [MaxLength(1500)]
        public string? Descripcion { get; set; }

        [MaxLength(1000)]
        public string? Riesgo { get; set; }

        [MaxLength(2000)]
        public string? MedidasContractuales { get; set; }

        [MaxLength(2000)]
        public string? MedidasTecnicas { get; set; }
    }
}
