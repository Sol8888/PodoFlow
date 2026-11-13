using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.OpcionComunicacion
{
    public class OpcionComunicacionActualizarDto
    {
        [Required]
        public bool? Permitido { get; set; }
    }
}
