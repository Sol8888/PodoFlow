using PodoFlow.Api.DTOs.Insumo;
using PodoFlow.Api.DTOs.Movimiento;

namespace PodoFlow.Api.Tests.Servicios.Inventario;

public class InsumoServicioPruebas : PruebaServicioBase
{
    public InsumoServicioPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    private IInsumoServicio Servicio => Servicio<IInsumoServicio>();

    private static InsumoCrearDto Nuevo()
        => new() { Nombre = " Gasa estéril " + Identificaciones.Sufijo(), Categoria = " Curaciones ", Unidad = " paquete ", StockMinimo = 5 };

    [Fact]
    public async Task Crea_el_insumo_con_stock_cero_y_alerta_de_minimo()
    {
        var creado = await Servicio.CrearAsync(Nuevo());

        Assert.Equal(0m, creado.StockActual);
        Assert.True(creado.BajoMinimo);
        Assert.Equal("Curaciones", creado.Categoria);
        Assert.NotNull(await Servicio.ObtenerPorIdAsync(creado.Id));
        Assert.NotEmpty(await Servicio.ObtenerTodosAsync(false, 1, 100));
    }

    [Fact]
    public async Task Valida_datos_y_duplicados()
    {
        var dto = Nuevo();
        await Servicio.CrearAsync(dto);
        await Falla<InvalidOperationException>(() => Servicio.CrearAsync(dto));

        var sinNombre = Nuevo();
        sinNombre.Nombre = " ";
        await Falla<ArgumentException>(() => Servicio.CrearAsync(sinNombre));

        var sinCategoria = Nuevo();
        sinCategoria.Categoria = "";
        await Falla<ArgumentException>(() => Servicio.CrearAsync(sinCategoria));

        var sinUnidad = Nuevo();
        sinUnidad.Unidad = "";
        await Falla<ArgumentException>(() => Servicio.CrearAsync(sinUnidad));

        var decimales = Nuevo();
        decimales.StockMinimo = 1.234m;
        await Falla<ArgumentException>(() => Servicio.CrearAsync(decimales));

        await Falla<ArgumentException>(() => Servicio.ObtenerTodosAsync(false, 0, 10));
        await Falla<ArgumentException>(() => Servicio.ObtenerPorIdAsync(0));
    }

    [Fact]
    public async Task Actualiza_da_de_baja_y_reactiva_con_version()
    {
        var dto = Nuevo();
        var creado = await Servicio.CrearAsync(dto);
        var otro = await Servicio.CrearAsync(Nuevo());

        await Falla<InvalidOperationException>(() => Servicio.ActualizarAsync(otro.Id, new InsumoActualizarDto { Nombre = dto.Nombre, Categoria = dto.Categoria, Unidad = dto.Unidad, Version = otro.Version }));
        Assert.True(await Servicio.ActualizarAsync(creado.Id, new InsumoActualizarDto { Nombre = dto.Nombre, Categoria = dto.Categoria, Unidad = dto.Unidad, StockMinimo = 2, Version = creado.Version }));
        var actual = await Servicio.ObtenerPorIdAsync(creado.Id);
        await Falla<ArgumentException>(() => Servicio.EliminarAsync(creado.Id, "malo"));
        Assert.True(await Servicio.EliminarAsync(creado.Id, actual!.Version));
        var inactivo = await Servicio.ObtenerPorIdAsync(creado.Id);
        Assert.True(await Servicio.EliminarAsync(creado.Id, inactivo!.Version));
        await Falla<InvalidOperationException>(() => Servicio.ActualizarAsync(creado.Id, new InsumoActualizarDto { Nombre = "x", Categoria = "x", Unidad = "x", Version = inactivo.Version }));

        var reactivado = await Servicio.CrearAsync(dto);
        Assert.Equal(creado.Id, reactivado.Id);
        Assert.True(await Servicio.ReactivarAsync(creado.Id, reactivado.Version));

        var paraBaja = await Servicio.ObtenerPorIdAsync(creado.Id);
        await Servicio.EliminarAsync(creado.Id, paraBaja!.Version);
        var dadoDeBaja = await Servicio.ObtenerPorIdAsync(creado.Id);
        Assert.True(await Servicio.ReactivarAsync(creado.Id, dadoDeBaja!.Version));
        Assert.False(await Servicio.ActualizarAsync(int.MaxValue, new InsumoActualizarDto { Nombre = "x", Categoria = "x", Unidad = "x", Version = dadoDeBaja.Version }));
        Assert.False(await Servicio.EliminarAsync(int.MaxValue, dadoDeBaja.Version));
        Assert.False(await Servicio.ReactivarAsync(int.MaxValue, dadoDeBaja.Version));
    }

    [Fact]
    public async Task Detecta_version_vencida_al_actualizar()
    {
        var creado = await Servicio.CrearAsync(Nuevo());

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => Servicio.ActualizarAsync(creado.Id, new InsumoActualizarDto
        {
            Nombre = "Nombre nuevo",
            Categoria = "Curaciones",
            Unidad = "paquete",
            Version = Convert.ToBase64String(new byte[8])
        }));
    }
}

public class MovimientoServicioPruebas : PruebaServicioBase
{
    public MovimientoServicioPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    private IMovimientoServicio Servicio => Servicio<IMovimientoServicio>();

    private static MovimientoCrearDto Movimiento(int insumoId, string tipo, decimal cantidad, long? atencionId = null)
        => new() { InsumoId = insumoId, UsuarioId = Semilla.UsuarioAdmin, Tipo = tipo, Cantidad = cantidad, Motivo = " Movimiento de prueba ", AtencionId = atencionId };

    [Fact]
    public async Task Ingresos_y_consumos_actualizan_el_stock()
    {
        var insumo = await NuevoInsumoAsync();
        var atencion = await NuevaAtencionAsync((await NuevaHistoriaAsync((await NuevoPacienteAsync()).PaId)).HiId);

        var ingreso = await Servicio.CrearAsync(Movimiento(insumo.InId, "ingreso", 10.5m));
        OlvidarSeguimiento();
        var consumo = await Servicio.CrearAsync(Movimiento(insumo.InId, "CONSUMO", 3m, atencion.AtId));
        OlvidarSeguimiento();
        var ajuste = await Servicio.CrearAsync(Movimiento(insumo.InId, "AJUSTE_NEG", 0.5m));

        Assert.Equal(10.5m, ingreso.StockNuevo);
        Assert.Equal(7.5m, consumo.StockNuevo);
        Assert.Equal(7m, ajuste.StockNuevo);
        OlvidarSeguimiento();
        Assert.Equal(7m, (await Contexto.Insumos.SingleAsync(i => i.InId == insumo.InId)).InStockActu);
        Assert.Equal(3, (await Servicio.ObtenerTodosAsync(insumo.InId, null, null, 1, 10)).Count);
        Assert.Single(await Servicio.ObtenerTodosAsync(null, atencion.AtId, "consumo", 1, 10));
        Assert.NotNull(await Servicio.ObtenerPorIdAsync(ingreso.Id));
    }

    [Fact]
    public async Task No_permite_stock_negativo_ni_insumos_inactivos()
    {
        var insumo = await NuevoInsumoAsync(2m);

        var error = await Falla<InvalidOperationException>(() => Servicio.CrearAsync(Movimiento(insumo.InId, "CONSUMO", 5m)));
        Assert.Contains("Stock insuficiente", error.Message);

        insumo.InEstatus = false;
        await Contexto.SaveChangesAsync();
        await Falla<InvalidOperationException>(() => Servicio.CrearAsync(Movimiento(insumo.InId, "INGRESO", 1m)));
    }

    [Fact]
    public async Task Valida_tipo_cantidad_motivo_y_referencias()
    {
        var insumo = await NuevoInsumoAsync();

        await Falla<ArgumentException>(() => Servicio.CrearAsync(Movimiento(insumo.InId, "REGALO", 1m)));
        await Falla<ArgumentException>(() => Servicio.CrearAsync(Movimiento(insumo.InId, "INGRESO", 0m)));
        await Falla<ArgumentException>(() => Servicio.CrearAsync(Movimiento(insumo.InId, "INGRESO", 1.001m)));
        await Falla<ArgumentException>(() => Servicio.CrearAsync(Movimiento(0, "INGRESO", 1m)));
        await Falla<ArgumentException>(() => Servicio.CrearAsync(Movimiento(int.MaxValue, "INGRESO", 1m)));
        await Falla<ArgumentException>(() => Servicio.CrearAsync(Movimiento(insumo.InId, "INGRESO", 1m, Semilla.IdInexistente)));

        var sinMotivo = Movimiento(insumo.InId, "INGRESO", 1m);
        sinMotivo.Motivo = " ";
        await Falla<ArgumentException>(() => Servicio.CrearAsync(sinMotivo));

        var usuario = Movimiento(insumo.InId, "INGRESO", 1m);
        usuario.UsuarioId = Semilla.UsuarioInactivo;
        await Falla<ArgumentException>(() => Servicio.CrearAsync(usuario));

        await Falla<ArgumentException>(() => Servicio.ObtenerTodosAsync(null, null, "REGALO", 1, 10));
        await Falla<ArgumentException>(() => Servicio.ObtenerTodosAsync(-1, null, null, 1, 10));
        await Falla<ArgumentException>(() => Servicio.ObtenerTodosAsync(null, -1, null, 1, 10));
        await Falla<ArgumentException>(() => Servicio.ObtenerTodosAsync(null, null, null, 1, 0));
        await Falla<ArgumentException>(() => Servicio.ObtenerPorIdAsync(0));
        Assert.Null(await Servicio.ObtenerPorIdAsync(Semilla.IdInexistente));
    }
}
