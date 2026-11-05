using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.TratamientoEncargado
{
    public class TratamientoEncargadoCrearDto
    {
        [Required]
        public int TratamientoId { get; set; }

        [Required]
        public int EncargadoId { get; set; }
    }
}
