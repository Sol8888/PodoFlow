using PodoFlow.Api.DTOs.Cobro;
using PodoFlow.Api.DTOs.FormaPago;
using PodoFlow.Api.DTOs.Prestacion;

namespace PodoFlow.Api.Tests.Servicios.Finanzas;

public class PrestacionYCobroServicioPruebas : PruebaServicioBase
{
    private const int Efectivo = 1;
    private const int Transferencia = 2;

    public PrestacionYCobroServicioPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    private IPrestacionServicio Prestaciones => Servicio<IPrestacionServicio>();

    private ICobroServicio Cobros => Servicio<ICobroServicio>();

    private async Task<PrestacionRespuestaDto> NuevaPrestacionAsync(decimal precio = 25m, decimal cantidad = 2m)
    {
        var paciente = await NuevoPacienteAsync();
        return await Prestaciones.CrearAsync(new PrestacionCrearDto
        {
            PacienteId = paciente.PaId,
            ServicioId = Semilla.Servicio,
            UsuarioId = Semilla.UsuarioAdmin,
            PrecioAplicado = precio,
            Cantidad = cantidad,
            Observaciones = " Consulta inicial "
        });
    }

    private Task<CobroRespuestaDto> CobrarAsync(long prestacionId, decimal valor, int formaPago = Efectivo)
        => Cobros.CrearAsync(new CobroCrearDto { PrestacionId = prestacionId, FormaPagoId = formaPago, UsuarioId = Semilla.UsuarioAdmin, Valor = valor, Referencia = " REF-1 " });

    [Fact]
    public async Task Calcula_el_total_de_la_prestacion()
    {
        var creada = await NuevaPrestacionAsync();

        Assert.Equal(50m, creada.Total);
        Assert.Equal("PENDIENTE", creada.Estado);
        Assert.Contains(await Prestaciones.ObtenerTodosAsync(creada.PacienteId, "pendiente", 1, 10), p => p.Id == creada.Id);
        Assert.NotNull(await Prestaciones.ObtenerPorIdAsync(creada.Id));
    }

    [Fact]
    public async Task Los_cobros_parciales_y_totales_cambian_el_estado_de_la_prestacion()
    {
        var prestacion = await NuevaPrestacionAsync();

        var parcial = await CobrarAsync(prestacion.Id, 20m);
        Assert.Equal(30m, parcial.SaldoPendiente);
        Assert.Equal("PENDIENTE", parcial.EstadoPrestacion);

        var total = await CobrarAsync(prestacion.Id, 30m, Transferencia);
        Assert.Equal(0m, total.SaldoPendiente);
        Assert.Equal("PAGADA", total.EstadoPrestacion);

        await Falla<InvalidOperationException>(() => CobrarAsync(prestacion.Id, 1m));
        await Falla<InvalidOperationException>(() => Prestaciones.CambiarEstadoAsync(prestacion.Id, new PrestacionCambiarEstadoDto { NuevoEstado = "ANULADA", Motivo = "x" }));
        await Falla<InvalidOperationException>(() => Prestaciones.ActualizarAsync(prestacion.Id, new PrestacionActualizarDto { PrecioAplicado = 10m, Cantidad = 1m }));

        Assert.True(await Cobros.CambiarEstadoAsync(total.Id, new CobroCambiarEstadoDto { NuevoEstado = "anulado", Motivo = "Pago duplicado" }));
        var reabierta = await Cobros.ObtenerPorIdAsync(parcial.Id);
        Assert.Equal("PENDIENTE", reabierta!.EstadoPrestacion);
        Assert.True(await Cobros.CambiarEstadoAsync(total.Id, new CobroCambiarEstadoDto { NuevoEstado = "ANULADO", Motivo = "Otra vez" }));
        Assert.Equal(2, (await Cobros.ObtenerTodosAsync(prestacion.Id, null, 1, 10)).Count);
        Assert.Single(await Cobros.ObtenerTodosAsync(prestacion.Id, "ANULADO", 1, 10));
    }

    [Fact]
    public async Task Edita_un_cobro_sin_exceder_el_total()
    {
        var prestacion = await NuevaPrestacionAsync();
        var cobro = await CobrarAsync(prestacion.Id, 10m);

        Assert.True(await Cobros.ActualizarAsync(cobro.Id, new CobroActualizarDto { FormaPagoId = Transferencia, Valor = 50m, Referencia = "TRX" }));
        Assert.Equal("PAGADA", (await Cobros.ObtenerPorIdAsync(cobro.Id))!.EstadoPrestacion);
        await Falla<InvalidOperationException>(() => Cobros.ActualizarAsync(cobro.Id, new CobroActualizarDto { FormaPagoId = Transferencia, Valor = 60m }));
        await Falla<ArgumentException>(() => Cobros.ActualizarAsync(cobro.Id, new CobroActualizarDto { FormaPagoId = int.MaxValue, Valor = 5m }));

        await Cobros.CambiarEstadoAsync(cobro.Id, new CobroCambiarEstadoDto { NuevoEstado = "ANULADO", Motivo = "Error" });
        await Falla<InvalidOperationException>(() => Cobros.ActualizarAsync(cobro.Id, new CobroActualizarDto { FormaPagoId = Efectivo, Valor = 5m }));
    }

    [Fact]
    public async Task Anula_prestaciones_pendientes_sin_cobros_y_bloquea_sus_cobros()
    {
        var prestacion = await NuevaPrestacionAsync();

        await Falla<InvalidOperationException>(() => Prestaciones.CambiarEstadoAsync(prestacion.Id, new PrestacionCambiarEstadoDto { NuevoEstado = "PAGADA", Motivo = "x" }));
        Assert.True(await Prestaciones.ActualizarAsync(prestacion.Id, new PrestacionActualizarDto { PrecioAplicado = 30m, Cantidad = 1m, Observaciones = "Ajuste" }));
        Assert.Equal(30m, (await Prestaciones.ObtenerPorIdAsync(prestacion.Id))!.Total);
        Assert.True(await Prestaciones.CambiarEstadoAsync(prestacion.Id, new PrestacionCambiarEstadoDto { NuevoEstado = "anulada", Motivo = "Error de registro" }));
        Assert.True(await Prestaciones.CambiarEstadoAsync(prestacion.Id, new PrestacionCambiarEstadoDto { NuevoEstado = "ANULADA", Motivo = "Repetido" }));
        Assert.Contains("[ANULACION]", (await Prestaciones.ObtenerPorIdAsync(prestacion.Id))!.Observaciones);

        await Falla<InvalidOperationException>(() => CobrarAsync(prestacion.Id, 5m));
        await Falla<InvalidOperationException>(() => Prestaciones.ActualizarAsync(prestacion.Id, new PrestacionActualizarDto { PrecioAplicado = 1m, Cantidad = 1m }));
    }

    [Fact]
    public async Task Una_prestacion_con_cobros_vigentes_no_se_anula()
    {
        var prestacion = await NuevaPrestacionAsync();
        await CobrarAsync(prestacion.Id, 5m);

        await Falla<InvalidOperationException>(() => Prestaciones.CambiarEstadoAsync(prestacion.Id, new PrestacionCambiarEstadoDto { NuevoEstado = "ANULADA", Motivo = "x" }));
        await Falla<InvalidOperationException>(() => Prestaciones.ActualizarAsync(prestacion.Id, new PrestacionActualizarDto { PrecioAplicado = 10m, Cantidad = 1m }));
    }

    [Fact]
    public async Task Valida_importes_y_relaciones_de_la_prestacion()
    {
        var paciente = await NuevoPacienteAsync();
        var otro = await NuevoPacienteAsync();
        var citaAjena = await NuevaCitaAsync(otro.PaId, DateTime.UtcNow.AddDays(-1), "ATENDIDA");
        var atencionAjena = await NuevaAtencionAsync((await NuevaHistoriaAsync(otro.PaId)).HiId);
        var historia = await NuevaHistoriaAsync(paciente.PaId);
        var citaPropia = await NuevaCitaAsync(paciente.PaId, DateTime.UtcNow.AddDays(-2), "ATENDIDA");
        var atencionPropia = await NuevaAtencionAsync(historia.HiId);
        PrestacionCrearDto Base() => new() { PacienteId = paciente.PaId, ServicioId = Semilla.Servicio, UsuarioId = Semilla.UsuarioAdmin, PrecioAplicado = 10m, Cantidad = 1m };

        var precio = Base(); precio.PrecioAplicado = -1m;
        await Falla<ArgumentException>(() => Prestaciones.CrearAsync(precio));
        var cantidad = Base(); cantidad.Cantidad = 0m;
        await Falla<ArgumentException>(() => Prestaciones.CrearAsync(cantidad));
        var cita = Base(); cita.CitaId = citaAjena.CiId;
        await Falla<InvalidOperationException>(() => Prestaciones.CrearAsync(cita));
        var atencion = Base(); atencion.AtencionId = atencionAjena.AtId;
        await Falla<InvalidOperationException>(() => Prestaciones.CrearAsync(atencion));
        var noCoincide = Base(); noCoincide.CitaId = citaPropia.CiId; noCoincide.AtencionId = atencionPropia.AtId;
        await Falla<InvalidOperationException>(() => Prestaciones.CrearAsync(noCoincide));
        var servicio = Base(); servicio.ServicioId = int.MaxValue;
        await Falla<ArgumentException>(() => Prestaciones.CrearAsync(servicio));
        var inactivo = Base(); inactivo.PacienteId = Semilla.PacienteInactivo;
        await Falla<ArgumentException>(() => Prestaciones.CrearAsync(inactivo));
        var usuario = Base(); usuario.UsuarioId = Semilla.UsuarioInactivo;
        await Falla<ArgumentException>(() => Prestaciones.CrearAsync(usuario));
        var sinCita = Base(); sinCita.CitaId = Semilla.IdInexistente;
        await Falla<ArgumentException>(() => Prestaciones.CrearAsync(sinCita));
        var sinAtencion = Base(); sinAtencion.AtencionId = Semilla.IdInexistente;
        await Falla<ArgumentException>(() => Prestaciones.CrearAsync(sinAtencion));
        await Falla<ArgumentException>(() => Prestaciones.ObtenerTodosAsync(null, "RARA", 1, 10));
        await Falla<ArgumentException>(() => Prestaciones.ObtenerTodosAsync(-1, null, 1, 10));
        await Falla<ArgumentException>(() => Prestaciones.ObtenerTodosAsync(null, null, 0, 10));
        await Falla<ArgumentException>(() => Prestaciones.ObtenerPorIdAsync(0));
        await Falla<ArgumentException>(() => Prestaciones.CambiarEstadoAsync(1, new PrestacionCambiarEstadoDto { NuevoEstado = "RARA", Motivo = "x" }));
        await Falla<ArgumentException>(() => Prestaciones.CambiarEstadoAsync(1, new PrestacionCambiarEstadoDto { NuevoEstado = "ANULADA", Motivo = " " }));
        Assert.Null(await Prestaciones.ObtenerPorIdAsync(Semilla.IdInexistente));
        Assert.False(await Prestaciones.ActualizarAsync(Semilla.IdInexistente, new PrestacionActualizarDto { PrecioAplicado = 1m, Cantidad = 1m }));
        Assert.False(await Prestaciones.CambiarEstadoAsync(Semilla.IdInexistente, new PrestacionCambiarEstadoDto { NuevoEstado = "ANULADA", Motivo = "x" }));
    }

    [Fact]
    public async Task Valida_los_datos_del_cobro()
    {
        var prestacion = await NuevaPrestacionAsync();

        await Falla<ArgumentException>(() => CobrarAsync(prestacion.Id, 0m));
        await Falla<ArgumentException>(() => CobrarAsync(prestacion.Id, 1.234m));
        await Falla<ArgumentException>(() => CobrarAsync(Semilla.IdInexistente, 1m));
        await Falla<ArgumentException>(() => CobrarAsync(prestacion.Id, 1m, int.MaxValue));
        await Falla<ArgumentException>(() => Cobros.CrearAsync(new CobroCrearDto { PrestacionId = prestacion.Id, FormaPagoId = Efectivo, UsuarioId = Semilla.UsuarioInactivo, Valor = 1m }));
        await Falla<ArgumentException>(() => Cobros.CrearAsync(new CobroCrearDto { PrestacionId = 0, FormaPagoId = Efectivo, UsuarioId = Semilla.UsuarioAdmin, Valor = 1m }));
        await Falla<ArgumentException>(() => Cobros.CrearAsync(new CobroCrearDto { PrestacionId = prestacion.Id, FormaPagoId = Efectivo, UsuarioId = Semilla.UsuarioAdmin, Valor = 1m, Referencia = new string('r', 151) }));
        await Falla<ArgumentException>(() => Cobros.CambiarEstadoAsync(1, new CobroCambiarEstadoDto { NuevoEstado = "REGISTRADO", Motivo = "x" }));
        await Falla<ArgumentException>(() => Cobros.CambiarEstadoAsync(1, new CobroCambiarEstadoDto { NuevoEstado = "ANULADO", Motivo = " " }));
        await Falla<ArgumentException>(() => Cobros.ObtenerTodosAsync(null, "RARO", 1, 10));
        await Falla<ArgumentException>(() => Cobros.ObtenerTodosAsync(-1, null, 1, 10));
        await Falla<ArgumentException>(() => Cobros.ObtenerTodosAsync(null, null, 1, 101));
        await Falla<ArgumentException>(() => Cobros.ObtenerPorIdAsync(0));
        await Falla<ArgumentException>(() => Cobros.ActualizarAsync(1, new CobroActualizarDto { FormaPagoId = 0, Valor = 1m }));
        Assert.Null(await Cobros.ObtenerPorIdAsync(Semilla.IdInexistente));
        Assert.False(await Cobros.ActualizarAsync(Semilla.IdInexistente, new CobroActualizarDto { FormaPagoId = Efectivo, Valor = 1m }));
        Assert.False(await Cobros.CambiarEstadoAsync(Semilla.IdInexistente, new CobroCambiarEstadoDto { NuevoEstado = "ANULADO", Motivo = "x" }));
    }
}

public class FormaPagoServicioPruebas : PruebaServicioBase
{
    public FormaPagoServicioPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    [Fact]
    public async Task Gestiona_formas_de_pago_con_codigo_unico()
    {
        var servicio = Servicio<IFormaPagoServicio>();
        var codigo = "QR_" + Identificaciones.Sufijo();

        var creada = await servicio.CrearAsync(new FormaPagoCrearDto { Codigo = codigo.ToLowerInvariant(), Nombre = " Pago QR " });

        Assert.Equal(codigo, creada.Codigo);
        await Falla<InvalidOperationException>(() => servicio.CrearAsync(new FormaPagoCrearDto { Codigo = codigo, Nombre = "Otra" }));
        await Falla<InvalidOperationException>(() => servicio.ActualizarAsync(creada.Id, new FormaPagoActualizarDto { Codigo = "EFECTIVO", Nombre = "x" }));
        Assert.True(await servicio.ActualizarAsync(creada.Id, new FormaPagoActualizarDto { Codigo = codigo, Nombre = "Pago con QR" }));
        Assert.True(await servicio.EliminarAsync(creada.Id));
        Assert.True(await servicio.EliminarAsync(creada.Id));
        await Falla<InvalidOperationException>(() => servicio.ActualizarAsync(creada.Id, new FormaPagoActualizarDto { Codigo = codigo, Nombre = "x" }));
        Assert.Equal(creada.Id, (await servicio.CrearAsync(new FormaPagoCrearDto { Codigo = codigo, Nombre = "QR" })).Id);
        await servicio.EliminarAsync(creada.Id);
        Assert.True(await servicio.ReactivarAsync(creada.Id));
        Assert.True(await servicio.ReactivarAsync(creada.Id));
        Assert.NotEmpty(await servicio.ObtenerTodosAsync(false, 1, 10));
        Assert.NotNull(await servicio.ObtenerPorIdAsync(creada.Id));
    }

    [Fact]
    public async Task Valida_datos_e_identificadores()
    {
        var servicio = Servicio<IFormaPagoServicio>();

        await Falla<ArgumentException>(() => servicio.CrearAsync(new FormaPagoCrearDto { Codigo = " ", Nombre = "x" }));
        await Falla<ArgumentException>(() => servicio.CrearAsync(new FormaPagoCrearDto { Codigo = "X", Nombre = " " }));
        await Falla<ArgumentException>(() => servicio.ObtenerTodosAsync(false, 0, 10));
        await Falla<ArgumentException>(() => servicio.ObtenerPorIdAsync(0));
        await Falla<ArgumentException>(() => servicio.ActualizarAsync(0, new FormaPagoActualizarDto { Codigo = "X", Nombre = "Y" }));
        await Falla<ArgumentException>(() => servicio.EliminarAsync(0));
        await Falla<ArgumentException>(() => servicio.ReactivarAsync(0));
        Assert.Null(await servicio.ObtenerPorIdAsync(int.MaxValue));
        Assert.False(await servicio.ActualizarAsync(int.MaxValue, new FormaPagoActualizarDto { Codigo = "X", Nombre = "Y" }));
        Assert.False(await servicio.EliminarAsync(int.MaxValue));
        Assert.False(await servicio.ReactivarAsync(int.MaxValue));
    }
}
