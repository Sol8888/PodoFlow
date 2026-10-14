namespace PodoFlow.Api.Modelos
{
    // Tabla intermedia pri.TR_EN: qué encargados (proveedores) participan en cada tratamiento.
    // Mismo estilo de nombre que UsuarioRol (US_RO) y PacienteUsuario (PA_US).
    public class TratamientoEncargado
    {
        public int TrId { get; set; }

        public int EnId { get; set; }

        public DateTime TeFechaAsig { get; set; }

        public bool TeEstatus { get; set; }
    }
}
