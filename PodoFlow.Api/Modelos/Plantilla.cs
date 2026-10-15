namespace PodoFlow.Api.Modelos
{
    // Texto base de los mensajes que se envían a los pacientes (correo o WhatsApp).
    public class Plantilla
    {
        public int PlId { get; set; }

        public string PlNombre { get; set; } = string.Empty;

        public string PlCanal { get; set; } = string.Empty;

        public string PlTipo { get; set; } = string.Empty;

        public string? PlAsunto { get; set; }

        public string PlConte { get; set; } = string.Empty;

        public string PlVersi { get; set; } = string.Empty;

        public DateTime PlFechaCrea { get; set; }

        public bool PlEstatus { get; set; }
    }
}
