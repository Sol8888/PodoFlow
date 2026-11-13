using PodoFlow.Api.DTOs.IntentoAcceso;
using PodoFlow.Api.DTOs.PacienteUsuario;
using PodoFlow.Api.DTOs.Permiso;
using PodoFlow.Api.DTOs.Rol;
using PodoFlow.Api.DTOs.RolPermiso;
using PodoFlow.Api.DTOs.Servicio;
using PodoFlow.Api.DTOs.Usuario;
using PodoFlow.Api.DTOs.UsuarioRol;

namespace PodoFlow.Api.Tests.Servicios.Seguridad;

public class UsuarioServicioPruebas : PruebaServicioBase
{
    public UsuarioServicioPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    private IUsuarioServicio Servicio => Servicio<IUsuarioServicio>();

    private static string Correo() => $"nuevo{Identificaciones.Sufijo().ToLowerInvariant()}@pruebas.podoflow.test";

    [Fact]
    public async Task Crea_usuarios_con_correo_unico()
    {
        var correo = Correo();
        var creado = await Servicio.CrearAsync(new UsuarioCrearDto { Nombre = " Dra. Pruebas ", Correo = correo });

        Assert.True(creado.Estatus);
        await Falla<InvalidOperationException>(() => Servicio.CrearAsync(new UsuarioCrearDto { Nombre = "Otra", Correo = correo }));
        await Falla<ArgumentException>(() => Servicio.CrearAsync(new UsuarioCrearDto { Nombre = " ", Correo = Correo() }));
        Assert.NotNull(await Servicio.ObtenerPorIdAsync(creado.Id));
        Assert.NotEmpty(await Servicio.ObtenerTodosAsync(false, 1, 10));
    }

    [Fact]
    public async Task Actualiza_con_version_y_evita_correos_ajenos()
    {
        var creado = await Servicio.CrearAsync(new UsuarioCrearDto { Nombre = "Usuario", Correo = Correo() });

        Assert.True(await Servicio.ActualizarAsync(creado.Id, new UsuarioActualizarDto { Nombre = "Usuario editado", Correo = creado.Correo, Version = creado.Version }));
        var actual = await Servicio.ObtenerPorIdAsync(creado.Id);
        await Falla<InvalidOperationException>(() => Servicio.ActualizarAsync(creado.Id, new UsuarioActualizarDto { Nombre = "x", Correo = "admin@pruebas.podoflow.test", Version = actual!.Version }));
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => Servicio.ActualizarAsync(creado.Id, new UsuarioActualizarDto { Nombre = "x", Correo = creado.Correo, Version = Convert.ToBase64String(new byte[8]) }));
        await Falla<ArgumentException>(() => Servicio.ActualizarAsync(creado.Id, new UsuarioActualizarDto { Nombre = "x", Correo = creado.Correo, Version = "@@" }));
        await Falla<ArgumentException>(() => Servicio.ActualizarAsync(creado.Id, new UsuarioActualizarDto { Nombre = " ", Correo = creado.Correo, Version = actual!.Version }));
    }

    [Fact]
    public async Task Da_de_baja_y_reactiva()
    {
        var creado = await Servicio.CrearAsync(new UsuarioCrearDto { Nombre = "Usuario", Correo = Correo() });

        Assert.True(await Servicio.EliminarAsync(creado.Id));
        Assert.True(await Servicio.EliminarAsync(creado.Id));
        Assert.Equal(creado.Id, (await Servicio.CrearAsync(new UsuarioCrearDto { Nombre = "Vuelve", Correo = creado.Correo })).Id);
        await Servicio.EliminarAsync(creado.Id);
        Assert.True(await Servicio.ReactivarAsync(creado.Id));
        Assert.True(await Servicio.ReactivarAsync(creado.Id));
        Assert.Null(await Servicio.ObtenerPorIdAsync(Semilla.IdInexistente));
        Assert.False(await Servicio.EliminarAsync(Semilla.IdInexistente));
        Assert.False(await Servicio.ReactivarAsync(Semilla.IdInexistente));
        Assert.False(await Servicio.ActualizarAsync(Semilla.IdInexistente, new UsuarioActualizarDto { Nombre = "x", Correo = Correo(), Version = Convert.ToBase64String(new byte[8]) }));
    }
}

public class RolYPermisoServicioPruebas : PruebaServicioBase
{
    public RolYPermisoServicioPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    private IRolServicio Roles => Servicio<IRolServicio>();

    private IPermisoServicio Permisos => Servicio<IPermisoServicio>();

    [Fact]
    public async Task Gestiona_roles_con_codigo_unico()
    {
        var codigo = "ROL_" + Identificaciones.Sufijo();
        var creado = await Roles.CrearAsync(new RolCrearDto { Codigo = codigo, Nombre = "Recepción" });

        await Falla<InvalidOperationException>(() => Roles.CrearAsync(new RolCrearDto { Codigo = codigo, Nombre = "Otro" }));
        await Falla<InvalidOperationException>(() => Roles.ActualizarAsync(creado.Id, new RolActualizarDto { Codigo = "ADMIN_PODOLOGA", Nombre = "x" }));
        await Falla<ArgumentException>(() => Roles.CrearAsync(new RolCrearDto { Codigo = " ", Nombre = "x" }));
        await Falla<ArgumentException>(() => Roles.CrearAsync(new RolCrearDto { Codigo = "X" + Identificaciones.Sufijo(), Nombre = " " }));
        Assert.True(await Roles.ActualizarAsync(creado.Id, new RolActualizarDto { Codigo = codigo, Nombre = "Recepción y caja", Descripcion = "d" }));
        Assert.True(await Roles.EliminarAsync(creado.Id));
        Assert.True(await Roles.EliminarAsync(creado.Id));
        Assert.Equal(creado.Id, (await Roles.CrearAsync(new RolCrearDto { Codigo = codigo, Nombre = "Recepción" })).Id);
        await Roles.EliminarAsync(creado.Id);
        Assert.True(await Roles.ReactivarAsync(creado.Id));
        Assert.True(await Roles.ReactivarAsync(creado.Id));
        Assert.NotEmpty(await Roles.ObtenerTodosAsync(false, 1, 10));
        Assert.NotNull(await Roles.ObtenerPorIdAsync(creado.Id));
        Assert.Null(await Roles.ObtenerPorIdAsync(int.MaxValue));
        Assert.False(await Roles.ActualizarAsync(int.MaxValue, new RolActualizarDto { Codigo = "X", Nombre = "Y" }));
        Assert.False(await Roles.EliminarAsync(int.MaxValue));
        Assert.False(await Roles.ReactivarAsync(int.MaxValue));
    }

    [Fact]
    public async Task Gestiona_permisos_con_codigo_unico()
    {
        var codigo = "PERMISO_" + Identificaciones.Sufijo();
        var creado = await Permisos.CrearAsync(new PermisoCrearDto { Codigo = codigo, Nombre = "Ver reportes", Modulo = "ANA" });

        await Falla<InvalidOperationException>(() => Permisos.CrearAsync(new PermisoCrearDto { Codigo = codigo, Nombre = "x", Modulo = "x" }));
        await Falla<InvalidOperationException>(() => Permisos.ActualizarAsync(creado.Id, new PermisoActualizarDto { Codigo = "PACIENTE_VER", Nombre = "x", Modulo = "x" }));
        await Falla<ArgumentException>(() => Permisos.CrearAsync(new PermisoCrearDto { Codigo = " ", Nombre = "x", Modulo = "x" }));
        await Falla<ArgumentException>(() => Permisos.CrearAsync(new PermisoCrearDto { Codigo = "Y" + Identificaciones.Sufijo(), Nombre = " ", Modulo = "x" }));
        await Falla<ArgumentException>(() => Permisos.CrearAsync(new PermisoCrearDto { Codigo = "Z" + Identificaciones.Sufijo(), Nombre = "x", Modulo = " " }));
        Assert.True(await Permisos.ActualizarAsync(creado.Id, new PermisoActualizarDto { Codigo = codigo, Nombre = "Ver reportes analíticos", Modulo = "ANA" }));
        Assert.True(await Permisos.EliminarAsync(creado.Id));
        Assert.True(await Permisos.EliminarAsync(creado.Id));
        Assert.Equal(creado.Id, (await Permisos.CrearAsync(new PermisoCrearDto { Codigo = codigo, Nombre = "Ver", Modulo = "ANA" })).Id);
        await Permisos.EliminarAsync(creado.Id);
        Assert.True(await Permisos.ReactivarAsync(creado.Id));
        Assert.True(await Permisos.ReactivarAsync(creado.Id));
        Assert.NotEmpty(await Permisos.ObtenerTodosAsync(false, 1, 100));
        Assert.Null(await Permisos.ObtenerPorIdAsync(int.MaxValue));
        Assert.False(await Permisos.ActualizarAsync(int.MaxValue, new PermisoActualizarDto { Codigo = "X", Nombre = "Y", Modulo = "Z" }));
        Assert.False(await Permisos.EliminarAsync(int.MaxValue));
        Assert.False(await Permisos.ReactivarAsync(int.MaxValue));
    }

    [Fact]
    public async Task Asigna_permisos_a_roles_y_roles_a_usuarios()
    {
        var rol = await Roles.CrearAsync(new RolCrearDto { Codigo = "ROL_" + Identificaciones.Sufijo(), Nombre = "Caja" });
        var p1 = await Permisos.CrearAsync(new PermisoCrearDto { Codigo = "P1_" + Identificaciones.Sufijo(), Nombre = "Uno", Modulo = "FIN" });
        var p2 = await Permisos.CrearAsync(new PermisoCrearDto { Codigo = "P2_" + Identificaciones.Sufijo(), Nombre = "Dos", Modulo = "FIN" });
        var usuario = await NuevoUsuarioAsync();
        var rolesPermisos = Servicio<IRolPermisoServicio>();
        var usuariosRoles = Servicio<IUsuarioRolServicio>();

        await rolesPermisos.CrearAsync(new RolPermisoCrearDto { RolId = rol.Id, PermisoId = p1.Id });
        await Falla<InvalidOperationException>(() => rolesPermisos.CrearAsync(new RolPermisoCrearDto { RolId = rol.Id, PermisoId = p1.Id }));
        await Falla<ArgumentException>(() => rolesPermisos.CrearAsync(new RolPermisoCrearDto { RolId = int.MaxValue, PermisoId = p1.Id }));
        await Falla<ArgumentException>(() => rolesPermisos.CrearAsync(new RolPermisoCrearDto { RolId = rol.Id, PermisoId = int.MaxValue }));
        Assert.True(await rolesPermisos.ActualizarAsync(rol.Id, p1.Id, new RolPermisoActualizarDto { NuevoPermisoId = p1.Id }));
        Assert.True(await rolesPermisos.ActualizarAsync(rol.Id, p1.Id, new RolPermisoActualizarDto { NuevoPermisoId = p2.Id }));
        Assert.NotNull(await rolesPermisos.ObtenerPorIdAsync(rol.Id, p2.Id));
        Assert.True(await rolesPermisos.EliminarAsync(rol.Id, p2.Id));
        Assert.True(await rolesPermisos.EliminarAsync(rol.Id, p2.Id));
        await Falla<InvalidOperationException>(() => rolesPermisos.ActualizarAsync(rol.Id, p2.Id, new RolPermisoActualizarDto { NuevoPermisoId = p1.Id }));
        Assert.True(await rolesPermisos.ReactivarAsync(rol.Id, p2.Id));
        Assert.True(await rolesPermisos.ReactivarAsync(rol.Id, p2.Id));
        Assert.NotEmpty(await rolesPermisos.ObtenerTodosAsync(true, 1, 100));

        await usuariosRoles.CrearAsync(new UsuarioRolCrearDto { UsuarioId = usuario.UsId, RolId = rol.Id });
        await Falla<InvalidOperationException>(() => usuariosRoles.CrearAsync(new UsuarioRolCrearDto { UsuarioId = usuario.UsId, RolId = rol.Id }));
        await Falla<ArgumentException>(() => usuariosRoles.CrearAsync(new UsuarioRolCrearDto { UsuarioId = Semilla.UsuarioInactivo, RolId = rol.Id }));
        var otroRol = await Roles.CrearAsync(new RolCrearDto { Codigo = "ROL_" + Identificaciones.Sufijo(), Nombre = "Agenda" });
        Assert.True(await usuariosRoles.ActualizarAsync(usuario.UsId, rol.Id, new UsuarioRolActualizarDto { NuevoRolId = rol.Id }));
        Assert.True(await usuariosRoles.ActualizarAsync(usuario.UsId, rol.Id, new UsuarioRolActualizarDto { NuevoRolId = otroRol.Id }));
        Assert.NotNull(await usuariosRoles.ObtenerPorIdAsync(usuario.UsId, otroRol.Id));
        Assert.True(await usuariosRoles.EliminarAsync(usuario.UsId, otroRol.Id));
        Assert.True(await usuariosRoles.EliminarAsync(usuario.UsId, otroRol.Id));
        await Falla<InvalidOperationException>(() => usuariosRoles.ActualizarAsync(usuario.UsId, otroRol.Id, new UsuarioRolActualizarDto { NuevoRolId = rol.Id }));
        Assert.True(await usuariosRoles.ReactivarAsync(usuario.UsId, otroRol.Id));
        Assert.True(await usuariosRoles.ReactivarAsync(usuario.UsId, otroRol.Id));
        Assert.NotEmpty(await usuariosRoles.ObtenerTodosAsync(true, 1, 100));
        Assert.Null(await usuariosRoles.ObtenerPorIdAsync(Semilla.IdInexistente, rol.Id));
        Assert.False(await usuariosRoles.EliminarAsync(Semilla.IdInexistente, rol.Id));
        Assert.False(await usuariosRoles.ReactivarAsync(Semilla.IdInexistente, rol.Id));
        Assert.False(await usuariosRoles.ActualizarAsync(Semilla.IdInexistente, rol.Id, new UsuarioRolActualizarDto { NuevoRolId = rol.Id }));
        Assert.Null(await rolesPermisos.ObtenerPorIdAsync(int.MaxValue, p1.Id));
        Assert.False(await rolesPermisos.EliminarAsync(int.MaxValue, p1.Id));
        Assert.False(await rolesPermisos.ReactivarAsync(int.MaxValue, p1.Id));
        Assert.False(await rolesPermisos.ActualizarAsync(int.MaxValue, p1.Id, new RolPermisoActualizarDto { NuevoPermisoId = p2.Id }));
    }
}

public class PacienteUsuarioServicioPruebas : PruebaServicioBase
{
    public PacienteUsuarioServicioPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    [Fact]
    public async Task Vincula_un_paciente_con_su_cuenta_de_usuario()
    {
        var servicio = Servicio<IPacienteUsuarioServicio>();
        var paciente = await NuevoPacienteAsync();
        var usuario = await NuevoUsuarioAsync();
        var otroUsuario = await NuevoUsuarioAsync();

        var vinculo = await servicio.CrearAsync(new PacienteUsuarioCrearDto { PacienteId = paciente.PaId, UsuarioId = usuario.UsId });

        Assert.True(vinculo.Estatus);
        await Falla<InvalidOperationException>(() => servicio.CrearAsync(new PacienteUsuarioCrearDto { PacienteId = paciente.PaId, UsuarioId = usuario.UsId }));
        await Falla<InvalidOperationException>(() => servicio.CrearAsync(new PacienteUsuarioCrearDto { PacienteId = paciente.PaId, UsuarioId = otroUsuario.UsId }));
        await Falla<ArgumentException>(() => servicio.CrearAsync(new PacienteUsuarioCrearDto { PacienteId = Semilla.PacienteInactivo, UsuarioId = otroUsuario.UsId }));
        await Falla<ArgumentException>(() => servicio.CrearAsync(new PacienteUsuarioCrearDto { PacienteId = paciente.PaId, UsuarioId = Semilla.UsuarioInactivo }));
        Assert.True(await servicio.ActualizarAsync(paciente.PaId, usuario.UsId, new PacienteUsuarioActualizarDto { NuevoUsuarioId = usuario.UsId }));
        await Falla<InvalidOperationException>(() => servicio.ActualizarAsync(paciente.PaId, usuario.UsId, new PacienteUsuarioActualizarDto { NuevoUsuarioId = otroUsuario.UsId }));
        Assert.True(await servicio.EliminarAsync(paciente.PaId, usuario.UsId));
        Assert.True(await servicio.EliminarAsync(paciente.PaId, usuario.UsId));
        Assert.True((await servicio.CrearAsync(new PacienteUsuarioCrearDto { PacienteId = paciente.PaId, UsuarioId = usuario.UsId })).Estatus);
        await servicio.EliminarAsync(paciente.PaId, usuario.UsId);
        Assert.True(await servicio.ReactivarAsync(paciente.PaId, usuario.UsId));
        Assert.True(await servicio.ReactivarAsync(paciente.PaId, usuario.UsId));
        Assert.NotNull(await servicio.ObtenerAsync(paciente.PaId, usuario.UsId));
        Assert.NotEmpty(await servicio.ListarAsync(true, 1, 100));
        Assert.Null(await servicio.ObtenerAsync(Semilla.IdInexistente, usuario.UsId));
        Assert.False(await servicio.ActualizarAsync(Semilla.IdInexistente, usuario.UsId, new PacienteUsuarioActualizarDto { NuevoUsuarioId = 1 }));
        Assert.False(await servicio.EliminarAsync(Semilla.IdInexistente, usuario.UsId));
        Assert.False(await servicio.ReactivarAsync(Semilla.IdInexistente, usuario.UsId));
    }
}

public class AuditoriaYAccesoServicioPruebas : PruebaServicioBase
{
    public AuditoriaYAccesoServicioPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    [Fact]
    public async Task Registra_eventos_de_auditoria_y_los_filtra()
    {
        var servicio = Servicio<IEventoAuditoriaServicio>();

        var evento = await servicio.RegistrarAsync(Semilla.UsuarioAdmin, Semilla.PacienteAdulto, " pruebas ", " leer ", "PACIENTE", "900001", "correcto", "127.0.0.1", "xunit", "{\"origen\":\"pruebas\"}");

        Assert.NotNull(await servicio.ObtenerPorIdAsync(evento.Id));
        Assert.Contains(await servicio.ObtenerTodosAsync(Semilla.UsuarioAdmin, Semilla.PacienteAdulto, "PRUEBAS", "CORRECTO", DateTime.UtcNow.AddMinutes(-5), DateTime.UtcNow.AddMinutes(5), 1, 10), e => e.Id == evento.Id);
        await Falla<ArgumentException>(() => servicio.RegistrarAsync(null, null, "M", "A", null, null, "QUIZAS", null, null, null));
        await Falla<ArgumentException>(() => servicio.RegistrarAsync(null, null, " ", "A", null, null, "ERROR", null, null, null));
        await Falla<ArgumentException>(() => servicio.RegistrarAsync(null, null, "M", "A", null, null, "ERROR", null, null, "[1,2]"));
        await Falla<ArgumentException>(() => servicio.RegistrarAsync(null, null, "M", "A", null, null, "ERROR", null, null, "{no-json"));
        await Falla<ArgumentException>(() => servicio.RegistrarAsync(0, null, "M", "A", null, null, "ERROR", null, null, null));
        await Falla<ArgumentException>(() => servicio.RegistrarAsync(null, 0, "M", "A", null, null, "ERROR", null, null, null));
        await Falla<ArgumentException>(() => servicio.RegistrarAsync(null, null, new string('m', 81), "A", null, null, "ERROR", null, null, null));
        await Falla<ArgumentException>(() => servicio.ObtenerTodosAsync(null, null, null, null, DateTime.UtcNow, DateTime.UtcNow.AddDays(-1), 1, 10));
        await Falla<ArgumentException>(() => servicio.ObtenerTodosAsync(null, null, null, "QUIZAS", null, null, 1, 10));
        await Falla<ArgumentException>(() => servicio.ObtenerTodosAsync(0, null, null, null, null, null, 1, 10));
        await Falla<ArgumentException>(() => servicio.ObtenerTodosAsync(null, 0, null, null, null, null, 1, 10));
        await Falla<ArgumentException>(() => servicio.ObtenerTodosAsync(null, null, null, null, null, null, 0, 10));
        Assert.Null(await servicio.ObtenerPorIdAsync(Semilla.IdInexistente));
    }

    [Fact]
    public async Task Registra_intentos_de_acceso_exitosos_y_fallidos()
    {
        var servicio = Servicio<IIntentoAccesoServicio>();

        var exitoso = await servicio.CrearAsync(new IntentoAccesoCrearDto { UsuarioId = Semilla.UsuarioAdmin, IdentificadorAcceso = " admin@pruebas.podoflow.test ", Exitoso = true }, "127.0.0.1", "xunit");
        var fallido = await servicio.CrearAsync(new IntentoAccesoCrearDto { IdentificadorAcceso = "desconocido", Exitoso = false, Motivo = "Clave incorrecta" }, null, null);

        Assert.True(exitoso.Exitoso);
        Assert.False(fallido.Exitoso);
        Assert.NotNull(await servicio.ObtenerPorIdAsync(exitoso.Id));
        Assert.Contains(await servicio.ObtenerTodosAsync(Semilla.UsuarioAdmin, true, 1, 10), i => i.Id == exitoso.Id);
        await Falla<ArgumentException>(() => servicio.CrearAsync(new IntentoAccesoCrearDto { UsuarioId = 0, Exitoso = false }, null, null));
        await Falla<ArgumentException>(() => servicio.CrearAsync(new IntentoAccesoCrearDto { UsuarioId = Semilla.IdInexistente, Exitoso = false }, null, null));
        await Falla<ArgumentException>(() => servicio.CrearAsync(new IntentoAccesoCrearDto { IdentificadorAcceso = new string('x', 321), Exitoso = false }, null, null));
        await Falla<ArgumentException>(() => servicio.CrearAsync(new IntentoAccesoCrearDto { Motivo = new string('x', 301), Exitoso = false }, null, null));
        Assert.Null(await servicio.ObtenerPorIdAsync(Semilla.IdInexistente));
    }
}

public class ServicioCatalogoServicioPruebas : PruebaServicioBase
{
    public ServicioCatalogoServicioPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    [Fact]
    public async Task Gestiona_el_catalogo_de_servicios()
    {
        var servicio = Servicio<IServicioServicio>();

        var creado = await servicio.CrearAsync(new ServicioCrearDto { Nombre = "Quiropodia " + Identificaciones.Sufijo(), Precio = 30m, DuracionMinutos = 45 });

        Assert.NotNull(await servicio.ObtenerPorIdAsync(creado.Id));
        Assert.Contains(await servicio.ObtenerTodosAsync(), s => s.Id == creado.Id);
        Assert.True(await servicio.ActualizarAsync(creado.Id, new ServicioActualizarDto { Nombre = creado.Nombre, Precio = 35m, DuracionMinutos = 45, Estatus = true }));
        Assert.Equal(35m, (await servicio.ObtenerPorIdAsync(creado.Id))!.Precio);
        Assert.True(await servicio.EliminarAsync(creado.Id));
        Assert.Null(await servicio.ObtenerPorIdAsync(int.MaxValue));
        Assert.False(await servicio.ActualizarAsync(int.MaxValue, new ServicioActualizarDto { Nombre = "x", DuracionMinutos = 1 }));
        Assert.False(await servicio.EliminarAsync(int.MaxValue));
    }
}
