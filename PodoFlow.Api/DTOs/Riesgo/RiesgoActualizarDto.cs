using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Riesgo
{
    // Sin TratamientoId: el riesgo pertenece a su tratamiento y no se mueve a otro.
    // Sin Estado: se cambia por PATCH {id}/estado.
    public class RiesgoActualizarDto
    {
        [Required, MaxLength(2000)]
        public string Descripcion { get; set; } = string.Empty;

        [Range(1, 5)]
        public byte Probabilidad { get; set; }

        [Range(1, 5)]
        public byte Impacto { get; set; }

        [MaxLength(2000)]
        public string? MedidasMitigacion { get; set; }

        [Range(1, 25)]
        public byte? RiesgoResidual { get; set; }

        public DateOnly? FechaRevision { get; set; }
    }
}
