using PodoFlow.Api.DTOs.Aviso;
using PodoFlow.Api.DTOs.Finalidad;

namespace PodoFlow.Api.Tests.Servicios.Privacidad;

public class FinalidadServicioPruebas : PruebaServicioBase
{
    public FinalidadServicioPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    private IFinalidadServicio Servicio => Servicio<IFinalidadServicio>();

    [Fact]
    public async Task Crea_la_finalidad_con_codigo_en_mayusculas()
    {
        var creada = await Servicio.CrearAsync(new FinalidadCrearDto
        {
            Codigo = " encuestas_" + Identificaciones.Sufijo().ToLowerInvariant(),
            Nombre = " Encuestas de satisfacción ",
            RequiereConsentimiento = true
        });

        Assert.Equal(creada.Codigo.ToUpperInvariant(), creada.Codigo);
        Assert.Equal("Encuestas de satisfacción", creada.Nombre);
        Assert.True(creada.Estatus);
        Assert.Contains(await Servicio.ObtenerTodosAsync(false, 1, 100), f => f.Id == creada.Id);
    }

    [Fact]
    public async Task No_duplica_un_codigo_activo()
    {
        var error = await Falla<InvalidOperationException>(() => Servicio.CrearAsync(new FinalidadCrearDto
        {
            Codigo = "recordatorios",
            Nombre = "Duplicada"
        }));

        Assert.Contains("RECORDATORIOS", error.Message);
    }

    [Fact]
    public async Task Crear_un_codigo_dado_de_baja_lo_reactiva_con_los_datos_nuevos()
    {
        var creada = await Servicio.CrearAsync(new FinalidadCrearDto { Codigo = "TEMP_" + Identificaciones.Sufijo(), Nombre = "Temporal" });
        Assert.True(await Servicio.EliminarAsync(creada.Id));
        Assert.True(await Servicio.EliminarAsync(creada.Id));

        var reactivada = await Servicio.CrearAsync(new FinalidadCrearDto { Codigo = creada.Codigo, Nombre = "Renovada", RequiereConsentimiento = true });

        Assert.Equal(creada.Id, reactivada.Id);
        Assert.Equal("Renovada", reactivada.Nombre);
        Assert.True(reactivada.Estatus);
    }

    [Fact]
    public async Task Actualiza_da_de_baja_y_reactiva()
    {
        var creada = await Servicio.CrearAsync(new FinalidadCrearDto { Codigo = "TEMP_" + Identificaciones.Sufijo(), Nombre = "Temporal" });

        Assert.True(await Servicio.ActualizarAsync(creada.Id, new FinalidadActualizarDto { Nombre = "Cambiada", RequiereConsentimiento = true }));
        Assert.True((await Servicio.ObtenerPorIdAsync(creada.Id))!.RequiereConsentimiento);
        await Servicio.EliminarAsync(creada.Id);
        Assert.DoesNotContain(await Servicio.ObtenerTodosAsync(false, 1, 100), f => f.Id == creada.Id);
        Assert.True(await Servicio.ReactivarAsync(creada.Id));
        Assert.True(await Servicio.ReactivarAsync(creada.Id));
    }

    [Fact]
    public async Task Devuelve_nulo_o_falso_si_no_existe()
    {
        Assert.Null(await Servicio.ObtenerPorIdAsync(int.MaxValue));
        Assert.False(await Servicio.ActualizarAsync(int.MaxValue, new FinalidadActualizarDto { Nombre = "x" }));
        Assert.False(await Servicio.EliminarAsync(int.MaxValue));
        Assert.False(await Servicio.ReactivarAsync(int.MaxValue));
    }
}

public class AvisoServicioPruebas : PruebaServicioBase
{
    public AvisoServicioPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    private IAvisoServicio Servicio => Servicio<IAvisoServicio>();

    [Fact]
    public async Task Crea_un_aviso_vigente()
    {
        var creado = await Servicio.CrearAsync(new AvisoCrearDto
        {
            Version = " V-" + Identificaciones.Sufijo() + " ",
            FechaDesde = new DateOnly(2026, 1, 1),
            RutaArchivo = " avisos/v2.pdf ",
            Huella = " abc "
        });

        Assert.Equal(creado.Version.Trim(), creado.Version);
        Assert.Equal("avisos/v2.pdf", creado.RutaArchivo);
        Assert.Contains(await Servicio.ObtenerTodosAsync(false, 1, 100), a => a.Id == creado.Id);
    }

    [Fact]
    public async Task Rechaza_fecha_hasta_anterior_a_desde()
    {
        await Falla<ArgumentException>(() => Servicio.CrearAsync(new AvisoCrearDto
        {
            Version = "V-" + Identificaciones.Sufijo(),
            FechaDesde = new DateOnly(2026, 5, 1),
            FechaHasta = new DateOnly(2026, 4, 1)
        }));

        await Falla<ArgumentException>(() => Servicio.ActualizarAsync(Semilla.Aviso, new AvisoActualizarDto
        {
            FechaDesde = new DateOnly(2026, 5, 1),
            FechaHasta = new DateOnly(2026, 4, 1)
        }));
    }

    [Fact]
    public async Task No_duplica_una_version_activa_y_reactiva_la_dada_de_baja()
    {
        await Falla<InvalidOperationException>(() => Servicio.CrearAsync(new AvisoCrearDto
        {
            Version = Semilla.VersionAviso,
            FechaDesde = new DateOnly(2026, 1, 1)
        }));

        var creado = await Servicio.CrearAsync(new AvisoCrearDto { Version = "V-" + Identificaciones.Sufijo(), FechaDesde = new DateOnly(2026, 1, 1) });
        Assert.True(await Servicio.EliminarAsync(creado.Id));
        Assert.True(await Servicio.EliminarAsync(creado.Id));

        var reactivado = await Servicio.CrearAsync(new AvisoCrearDto { Version = creado.Version, FechaDesde = new DateOnly(2026, 2, 1) });

        Assert.Equal(creado.Id, reactivado.Id);
        Assert.Equal(new DateOnly(2026, 2, 1), reactivado.FechaDesde);
    }

    [Fact]
    public async Task Actualiza_y_reactiva()
    {
        var creado = await Servicio.CrearAsync(new AvisoCrearDto { Version = "V-" + Identificaciones.Sufijo(), FechaDesde = new DateOnly(2026, 1, 1) });

        Assert.True(await Servicio.ActualizarAsync(creado.Id, new AvisoActualizarDto { FechaDesde = new DateOnly(2026, 1, 1), FechaHasta = new DateOnly(2026, 12, 31) }));
        Assert.Equal(new DateOnly(2026, 12, 31), (await Servicio.ObtenerPorIdAsync(creado.Id))!.FechaHasta);
        await Servicio.EliminarAsync(creado.Id);
        Assert.True(await Servicio.ReactivarAsync(creado.Id));
        Assert.True(await Servicio.ReactivarAsync(creado.Id));
    }

    [Fact]
    public async Task Devuelve_nulo_o_falso_si_no_existe()
    {
        Assert.Null(await Servicio.ObtenerPorIdAsync(int.MaxValue));
        Assert.False(await Servicio.ActualizarAsync(int.MaxValue, new AvisoActualizarDto { FechaDesde = new DateOnly(2026, 1, 1) }));
        Assert.False(await Servicio.EliminarAsync(int.MaxValue));
        Assert.False(await Servicio.ReactivarAsync(int.MaxValue));
    }
}
