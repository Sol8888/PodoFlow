using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.TratamientoCategoria
{
    // No hay ActualizarDto: la fila solo tiene las dos llaves, la fecha y el estatus.
    // Cambiar una llave sería otra asignación distinta.
    public class TratamientoCategoriaCrearDto
    {
        [Required]
        public int TratamientoId { get; set; }

        [Required]
        public int CategoriaId { get; set; }
    }
}
