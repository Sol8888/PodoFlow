namespace PodoFlow.Api.Modelos
{
    // Tabla intermedia pri.TR_CA: qué categorías de datos incluye cada tratamiento.
    // Mismo estilo de nombre que UsuarioRol (US_RO) y PacienteUsuario (PA_US).
    public class TratamientoCategoria
    {
        public int TrId { get; set; }

        public int CaId { get; set; }

        public DateTime TcFechaAsig { get; set; }

        public bool TcEstatus { get; set; }
    }
}
