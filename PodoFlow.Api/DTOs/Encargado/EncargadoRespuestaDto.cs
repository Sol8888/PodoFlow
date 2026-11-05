namespace PodoFlow.Api.DTOs.Encargado
{
    public class EncargadoRespuestaDto
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Servicio { get; set; } = string.Empty;

        public string? RolTratamiento { get; set; }

        public string? Descripcion { get; set; }

        public string? Riesgo { get; set; }

        public string? MedidasContractuales { get; set; }

        public string? MedidasTecnicas { get; set; }

        public bool Estatus { get; set; }
    }
}
