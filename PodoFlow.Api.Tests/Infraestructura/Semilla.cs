namespace PodoFlow.Api.Tests.Infraestructura;

public static class Semilla
{
    public const long UsuarioAdmin = 900001;
    public const long UsuarioInactivo = 900002;

    public const long PacienteAdulto = 900001;
    public const long PacienteMenor = 900002;
    public const long PacienteInactivo = 900003;

    public const string CedulaPacienteAdulto = "1710034065";

    public const int Servicio = 900001;

    public const long CitaFutura = 900001;
    public const long CitaPasada = 900002;

    public const int Aviso = 900001;
    public const string VersionAviso = "PRUEBAS-1.0";

    public const int PlantillaCorreo = 900001;
    public const int PlantillaWhatsApp = 900002;

    public const long ConsentimientoFidelizacion = 900001;

    public const string FinalidadAtencion = "ATENCION_CLINICA";
    public const string FinalidadCitas = "GESTION_CITAS";
    public const string FinalidadRecordatorios = "RECORDATORIOS";
    public const string FinalidadFidelizacion = "FIDELIZACION";
    public const string FinalidadAnalitica = "ANALITICA";

    public const long IdInexistente = 999_999_999;
}
