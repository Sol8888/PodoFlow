using System.ComponentModel.DataAnnotations;

namespace PodoFlow.Api.DTOs.OpcionComunicacion
{
    // Solo cambia el permiso. Paciente, finalidad y canal forman la clave única
    // (UQ_OPCION_pac_canal_fin): si cambian, es otra opción.
    // Sin Estatus: la baja y la reactivación tienen sus propios endpoints.
    public class OpcionComunicacionActualizarDto
    {
        [Required]
        public bool? Permitido { get; set; }
    }
}
