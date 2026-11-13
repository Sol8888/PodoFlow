using PodoFlow.Api.DTOs.CategoriaDato;
using PodoFlow.Api.DTOs.Encargado;
using PodoFlow.Api.DTOs.TratamientoCategoria;
using PodoFlow.Api.DTOs.TratamientoDatos;
using PodoFlow.Api.DTOs.TratamientoEncargado;

namespace PodoFlow.Api.Tests.Servicios.Privacidad;

public class TratamientoDatosServicioPruebas : PruebaServicioBase
{
    public TratamientoDatosServicioPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    private ITratamientoDatosServicio Servicio => Servicio<ITratamientoDatosServicio>();

    private async Task<TratamientoDatosCrearDto> NuevoAsync(string? codigo = null)
        => new()
        {
            Codigo = codigo ?? " rat_" + Identificaciones.Sufijo().ToLowerInvariant(),
            FinalidadId = await FinalidadIdAsync(Semilla.FinalidadAtencion),
            UsuarioId = Semilla.UsuarioAdmin,
            Nombre = " Gestión de historias clínicas ",
            Modulo = " cli ",
            BaseLegitimacion = " Art. 7 LOPDP ",
            PlazoConservacion = "  ",
            FechaRevision = new DateOnly(2027, 1, 1)
        };

    [Fact]
    public async Task Registra_la_actividad_de_tratamiento_normalizada()
    {
        var creado = await Servicio.CrearAsync(await NuevoAsync());

        Assert.StartsWith("RAT_", creado.Codigo);
        Assert.Equal("CLI", creado.Modulo);
        Assert.Equal("Art. 7 LOPDP", creado.BaseLegitimacion);
        Assert.Null(creado.PlazoConservacion);
        Assert.Contains(await Servicio.ObtenerTodosAsync(false, 1, 100), t => t.Id == creado.Id);
    }

    [Fact]
    public async Task Valida_finalidad_y_usuario_activos()
    {
        var dto = await NuevoAsync();
        dto.FinalidadId = int.MaxValue;
        await Falla<ArgumentException>(() => Servicio.CrearAsync(dto));

        dto = await NuevoAsync();
        dto.UsuarioId = Semilla.UsuarioInactivo;
        await Falla<ArgumentException>(() => Servicio.CrearAsync(dto));
    }

    [Fact]
    public async Task No_duplica_codigo_activo_y_reutiliza_el_dado_de_baja()
    {
        var dto = await NuevoAsync();
        var creado = await Servicio.CrearAsync(dto);
        await Falla<InvalidOperationException>(() => Servicio.CrearAsync(dto));

        Assert.True(await Servicio.EliminarAsync(creado.Id));
        Assert.True(await Servicio.EliminarAsync(creado.Id));
        var reactivado = await Servicio.CrearAsync(dto);

        Assert.Equal(creado.Id, reactivado.Id);
        Assert.True(reactivado.Estatus);
    }

    [Fact]
    public async Task Actualiza_y_reactiva()
    {
        var creado = await Servicio.CrearAsync(await NuevoAsync());

        Assert.True(await Servicio.ActualizarAsync(creado.Id, new TratamientoDatosActualizarDto
        {
            FinalidadId = await FinalidadIdAsync(Semilla.FinalidadCitas),
            Nombre = "Agenda",
            Modulo = "cit"
        }));
        Assert.Equal("CIT", (await Servicio.ObtenerPorIdAsync(creado.Id))!.Modulo);
        await Servicio.EliminarAsync(creado.Id);
        Assert.True(await Servicio.ReactivarAsync(creado.Id));
        Assert.True(await Servicio.ReactivarAsync(creado.Id));
        Assert.Null(await Servicio.ObtenerPorIdAsync(int.MaxValue));
        Assert.False(await Servicio.ActualizarAsync(int.MaxValue, new TratamientoDatosActualizarDto { FinalidadId = 1, Nombre = "x", Modulo = "x" }));
        Assert.False(await Servicio.EliminarAsync(int.MaxValue));
        Assert.False(await Servicio.ReactivarAsync(int.MaxValue));
    }

    [Fact]
    public async Task Asigna_categorias_y_encargados_al_tratamiento()
    {
        var tratamiento = await Servicio.CrearAsync(await NuevoAsync());
        var categorias = Servicio<ICategoriaDatoServicio>();
        var encargados = Servicio<IEncargadoServicio>();
        var asignarCategoria = Servicio<ITratamientoCategoriaServicio>();
        var asignarEncargado = Servicio<ITratamientoEncargadoServicio>();
        var categoria = await categorias.CrearAsync(new CategoriaDatoCrearDto { Nombre = "Biométricos " + Identificaciones.Sufijo(), EsSensible = true });
        var encargado = await encargados.CrearAsync(new EncargadoCrearDto { Nombre = "Nube " + Identificaciones.Sufijo(), Servicio = "Hosting" });

        var tc = await asignarCategoria.CrearAsync(new TratamientoCategoriaCrearDto { TratamientoId = tratamiento.Id, CategoriaId = categoria.Id });
        var te = await asignarEncargado.CrearAsync(new TratamientoEncargadoCrearDto { TratamientoId = tratamiento.Id, EncargadoId = encargado.Id });

        Assert.True(tc.Estatus);
        Assert.True(te.Estatus);
        await Falla<InvalidOperationException>(() => asignarCategoria.CrearAsync(new TratamientoCategoriaCrearDto { TratamientoId = tratamiento.Id, CategoriaId = categoria.Id }));
        await Falla<InvalidOperationException>(() => asignarEncargado.CrearAsync(new TratamientoEncargadoCrearDto { TratamientoId = tratamiento.Id, EncargadoId = encargado.Id }));
        await Falla<ArgumentException>(() => asignarCategoria.CrearAsync(new TratamientoCategoriaCrearDto { TratamientoId = tratamiento.Id, CategoriaId = int.MaxValue }));
        await Falla<ArgumentException>(() => asignarEncargado.CrearAsync(new TratamientoEncargadoCrearDto { TratamientoId = int.MaxValue, EncargadoId = encargado.Id }));
        Assert.Single(await asignarCategoria.ObtenerTodosAsync(tratamiento.Id, null, false, 1, 10));
        Assert.Single(await asignarEncargado.ObtenerTodosAsync(tratamiento.Id, null, false, 1, 10));

        Assert.True(await asignarCategoria.EliminarAsync(tratamiento.Id, categoria.Id));
        Assert.True(await asignarCategoria.EliminarAsync(tratamiento.Id, categoria.Id));
        Assert.True(await asignarCategoria.ReactivarAsync(tratamiento.Id, categoria.Id));
        Assert.True(await asignarCategoria.ReactivarAsync(tratamiento.Id, categoria.Id));
        Assert.True(await asignarEncargado.EliminarAsync(tratamiento.Id, encargado.Id));
        var reasignado = await asignarEncargado.CrearAsync(new TratamientoEncargadoCrearDto { TratamientoId = tratamiento.Id, EncargadoId = encargado.Id });
        Assert.True(reasignado.Estatus);
        Assert.True(await asignarEncargado.EliminarAsync(tratamiento.Id, encargado.Id));
        Assert.True(await asignarEncargado.ReactivarAsync(tratamiento.Id, encargado.Id));
        Assert.NotNull(await asignarCategoria.ObtenerAsync(tratamiento.Id, categoria.Id));
        Assert.NotNull(await asignarEncargado.ObtenerAsync(tratamiento.Id, encargado.Id));
        Assert.Null(await asignarCategoria.ObtenerAsync(int.MaxValue, categoria.Id));
        Assert.Null(await asignarEncargado.ObtenerAsync(int.MaxValue, encargado.Id));
        Assert.False(await asignarCategoria.EliminarAsync(int.MaxValue, 1));
        Assert.False(await asignarEncargado.EliminarAsync(int.MaxValue, 1));
        Assert.False(await asignarCategoria.ReactivarAsync(int.MaxValue, 1));
        Assert.False(await asignarEncargado.ReactivarAsync(int.MaxValue, 1));
    }
}

public class CategoriaYEncargadoServicioPruebas : PruebaServicioBase
{
    public CategoriaYEncargadoServicioPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    [Fact]
    public async Task Gestiona_categorias_de_datos_sin_nombres_repetidos()
    {
        var servicio = Servicio<ICategoriaDatoServicio>();
        var nombre = "Categoría " + Identificaciones.Sufijo();
        var creada = await servicio.CrearAsync(new CategoriaDatoCrearDto { Nombre = " " + nombre + " ", Descripcion = " d ", EsSensible = true });

        Assert.Equal(nombre, creada.Nombre);
        await Falla<InvalidOperationException>(() => servicio.CrearAsync(new CategoriaDatoCrearDto { Nombre = nombre }));
        await Falla<InvalidOperationException>(() => servicio.ActualizarAsync(creada.Id, new CategoriaDatoActualizarDto { Nombre = "Datos de pagos internos" }));
        Assert.True(await servicio.ActualizarAsync(creada.Id, new CategoriaDatoActualizarDto { Nombre = nombre + " v2", EsSensible = false }));
        Assert.True(await servicio.EliminarAsync(creada.Id));
        Assert.True(await servicio.EliminarAsync(creada.Id));
        var reactivada = await servicio.CrearAsync(new CategoriaDatoCrearDto { Nombre = nombre + " v2", EsSensible = true });
        Assert.Equal(creada.Id, reactivada.Id);
        await servicio.EliminarAsync(creada.Id);
        Assert.True(await servicio.ReactivarAsync(creada.Id));
        Assert.True(await servicio.ReactivarAsync(creada.Id));
        Assert.NotEmpty(await servicio.ObtenerTodosAsync(true, 1, 100));
        Assert.Null(await servicio.ObtenerPorIdAsync(int.MaxValue));
        Assert.False(await servicio.ActualizarAsync(int.MaxValue, new CategoriaDatoActualizarDto { Nombre = "x" }));
        Assert.False(await servicio.EliminarAsync(int.MaxValue));
        Assert.False(await servicio.ReactivarAsync(int.MaxValue));
    }

    [Fact]
    public async Task Gestiona_encargados_sin_duplicar_nombre_y_servicio()
    {
        var servicio = Servicio<IEncargadoServicio>();
        var dto = new EncargadoCrearDto
        {
            Nombre = "Proveedor " + Identificaciones.Sufijo(),
            Servicio = "Correo transaccional",
            RolTratamiento = " Encargado ",
            MedidasTecnicas = " TLS "
        };
        var creado = await servicio.CrearAsync(dto);

        Assert.Equal("Encargado", creado.RolTratamiento);
        await Falla<InvalidOperationException>(() => servicio.CrearAsync(dto));
        Assert.True(await servicio.ActualizarAsync(creado.Id, new EncargadoActualizarDto { Nombre = dto.Nombre, Servicio = "WhatsApp", Descripcion = "Mensajería" }));
        Assert.Equal("WhatsApp", (await servicio.ObtenerPorIdAsync(creado.Id))!.Servicio);
        Assert.True(await servicio.EliminarAsync(creado.Id));
        Assert.True(await servicio.EliminarAsync(creado.Id));

        var duplicadoInactivo = await Falla<InvalidOperationException>(() => servicio.CrearAsync(new EncargadoCrearDto { Nombre = dto.Nombre, Servicio = "WhatsApp" }));
        Assert.Contains("dado de baja", duplicadoInactivo.Message);

        var otro = await servicio.CrearAsync(new EncargadoCrearDto { Nombre = dto.Nombre, Servicio = "Otro servicio" });
        await servicio.ActualizarAsync(otro.Id, new EncargadoActualizarDto { Nombre = dto.Nombre, Servicio = "Otro servicio" });
        Assert.True(await servicio.ReactivarAsync(creado.Id));
        Assert.True(await servicio.ReactivarAsync(creado.Id));
        await Falla<InvalidOperationException>(() => servicio.ActualizarAsync(otro.Id, new EncargadoActualizarDto { Nombre = dto.Nombre, Servicio = "WhatsApp" }));
        Assert.NotEmpty(await servicio.ObtenerTodosAsync(false, 1, 100));
        Assert.Null(await servicio.ObtenerPorIdAsync(int.MaxValue));
        Assert.False(await servicio.ActualizarAsync(int.MaxValue, new EncargadoActualizarDto { Nombre = "x", Servicio = "x" }));
        Assert.False(await servicio.EliminarAsync(int.MaxValue));
        Assert.False(await servicio.ReactivarAsync(int.MaxValue));
    }
}
