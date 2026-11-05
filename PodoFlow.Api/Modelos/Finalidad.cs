namespace PodoFlow.Api.Modelos
{
    public class Finalidad
    {
        public int FiId { get; set; }
        public string FiCodigo { get; set; } = string.Empty;
        public string FiNombre { get; set; } = string.Empty;
        public string? FiDescri { get; set; }
        public bool FiRequiCons { get; set; }
        public bool FiEstatus { get; set; }


    }
}
