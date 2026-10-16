namespace PodoFlow.Api.Modelos
{
    public class Solicitud
    {
        public long SoId { get; set; }

        public long PaId { get; set; }

        public long? UsId { get; set; }

        public string SoTipoDere { get; set; } = string.Empty;

        public string SoTipoSoli { get; set; } = string.Empty;

        public string SoNombrSoli { get; set; } = string.Empty;

        public string SoIdentSoli { get; set; } = string.Empty;

        public DateTime SoFechaRece { get; set; }

        public string SoEstad { get; set; } = string.Empty;

        public string? SoMedioResp { get; set; }

        public string? SoDetal { get; set; }

        public string? SoRespue { get; set; }

        public DateTime? SoFechaCier { get; set; }

        public byte[] SoVersion { get; set; } = [];
    }
}
