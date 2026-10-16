namespace PodoFlow.Api.Modelos
{
    // Preferencia del paciente: si acepta o no recibir mensajes de una finalidad por un canal.
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
