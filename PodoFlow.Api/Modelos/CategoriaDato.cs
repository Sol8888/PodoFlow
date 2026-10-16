namespace PodoFlow.Api.Modelos
{
    public class CategoriaDato
    {
        public int CaId { get; set; }

        public string CaNombre { get; set; } = string.Empty;

        public string? CaDescri { get; set; }

        public bool CaSensi { get; set; }

        public bool CaEstatus { get; set; }
    }
}
