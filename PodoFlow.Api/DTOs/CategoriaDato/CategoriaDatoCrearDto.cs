using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.CategoriaDato
{
    public class CategoriaDatoCrearDto
    {
        [Required, MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Descripcion { get; set; }
        public bool EsSensible { get; set; }
    }
}
