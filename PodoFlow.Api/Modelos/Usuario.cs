namespace PodoFlow.Api.Modelos
{
    public class Usuario
    {
        public long UsId { get; set; }
        public string UsNombre { get; set; } = string.Empty;
        public string UsCorreo { get; set; } = string.Empty;
        public Guid? UsEntraOid { get; set; }
        public Guid? UsEntraTid { get; set; }
        public DateTime UsFechaCrea { get; set; }
        public DateTime? UsFechaModi { get; set; }
        public DateTime? UsUltimAcces { get; set; }
        public bool UsEstatus { get; set; } = true;
        public byte[] UsVersion { get; set; } = [];
    }
}
