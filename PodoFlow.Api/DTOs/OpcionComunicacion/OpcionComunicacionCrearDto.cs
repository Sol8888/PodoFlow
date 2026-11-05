using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.OpcionComunicacion
{
    // El consentimiento no se envía: se busca solo, porque hay uno por paciente y finalidad
    // (UQ_CONSENTIMIENTO_pac_fin).
    public class OpcionComunicacionCrearDto
    {
        [Range(1, long.MaxValue)]
        public long PacienteId { get; set; }

        [Range(1, int.MaxValue)]
        public int FinalidadId { get; set; }

        // CORREO o WHATSAPP (CK_OPCION_canal).
        [Required, MaxLength(20)]
        public string Canal { get; set; } = string.Empty;

        // true = acepta recibir mensajes; false = no quiere recibirlos.
        // bool? con [Required] para que, si no se envía, responda 400 en vez de guardar false.
        [Required]
        public bool? Permitido { get; set; }
    }
}
