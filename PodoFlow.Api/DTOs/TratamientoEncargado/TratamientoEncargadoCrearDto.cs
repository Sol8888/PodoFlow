using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.TratamientoEncargado
{
    // No hay ActualizarDto: la fila solo tiene las dos llaves, la fecha y el estatus.
    // Cambiar una llave sería otra asignación distinta.
    public class TratamientoEncargadoCrearDto
    {
        [Required]
        public int TratamientoId { get; set; }

        [Required]
        public int EncargadoId { get; set; }
    }
}
