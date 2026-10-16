using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.TratamientoCategoria
{
    public class TratamientoCategoriaCrearDto
    {
        [Required]
        public int TratamientoId { get; set; }

        [Required]
        public int CategoriaId { get; set; }
    }
}
