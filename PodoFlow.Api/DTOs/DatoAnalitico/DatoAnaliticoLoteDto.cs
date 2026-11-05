using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Ejecucion
{
    public class DatoAnaliticoLoteDto
    {
        [Required, MinLength(1), MaxLength(1000)]
        public List<DatoAnaliticoCrearDto> Datos { get; set; } = [];
    }
}
