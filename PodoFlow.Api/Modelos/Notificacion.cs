namespace PodoFlow.Api.Modelos
{
    public class Notificacion
    {
        public long NoId { get; set; }

        public long PaId { get; set; }

        public long? CiId { get; set; }

        public int PlId { get; set; }

        public long? OpId { get; set; }

        public long? UsId { get; set; }

        public string NoTipo { get; set; } = string.Empty;

        public string NoCanal { get; set; } = string.Empty;

        public DateTime NoFechaProg { get; set; }

        public string NoEstad { get; set; } = string.Empty;

        public DateTime NoFechaCrea { get; set; }

        public byte[] NoVersion { get; set; } = [];
    }
}
