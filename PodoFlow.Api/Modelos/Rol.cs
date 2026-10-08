namespace PodoFlow.Api.Modelos
{
    public class Rol
    {
        public int RoId { get; set; }

        public string RoCodigo { get; set; } = string.Empty;

        public string RoNombre { get; set; } = string.Empty;

        public string? RoDescri { get; set; }

        public DateTime RoFechaCrea { get; set; }

        public bool RoEstatus { get; set; } = true;
    }
}
