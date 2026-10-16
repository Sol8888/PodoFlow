using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.Seudonimo
{
    public class SeudonimoCrearDto
    {
        [Range(1, long.MaxValue)]
        public long PacienteId { get; set; }
    }
}
