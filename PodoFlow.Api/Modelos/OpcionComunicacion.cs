namespace PodoFlow.Api.Modelos
{
    public class OpcionComunicacion
    {
        public long OpId { get; set; }

        public long PaId { get; set; }

        public int FiId { get; set; }

        public long? CoId { get; set; }

        public string OpCanal { get; set; } = string.Empty;

        public bool OpPermi { get; set; }

        public DateTime OpFechaActu { get; set; }

        public bool OpEstatus { get; set; }
    }
}
