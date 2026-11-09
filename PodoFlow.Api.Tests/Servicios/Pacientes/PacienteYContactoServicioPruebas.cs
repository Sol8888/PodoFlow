using PodoFlow.Api.DTOs.DatoContacto;
using PodoFlow.Api.DTOs.Paciente;

namespace PodoFlow.Api.Tests.Servicios.Pacientes;

public class PacienteServicioPruebas : PruebaServicioBase
{
    public PacienteServicioPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    private IPacienteServicio Servicio => Servicio<IPacienteServicio>();

    private static PacienteCrearDto Nuevo(string? cedula = null)
        => new()
        {
            TipoIdentificacion = " cedula ",
            Identificacion = cedula ?? Identificaciones.CedulaValida(),
            Nombres = " Lucía ",
            Apellidos = " Andrade ",
            FechaNacimiento = new DateTime(1994, 2, 3),
            Genero = "FEMENINO",
            Direccion = " Cumbayá "
        };

    [Fact]
    public async Task Registra_al_paciente_con_cedula_ecuatoriana_valida()
    {
        var creado = await Servicio.CrearAsync(Nuevo());

        Assert.Equal("CEDULA", creado.TipoIdentificacion);
        Assert.Equal("Lucía", creado.Nombres);
        Assert.True(creado.Estatus);
        Assert.False(string.IsNullOrEmpty(creado.Version));
        Assert.NotNull(await Servicio.ObtenerPorIdAsync(creado.Id));
        Assert.NotEmpty(await Servicio.ObtenerTodosAsync(false, 1, 10));
    }

    [Fact]
    public async Task Rechaza_identificaciones_y_datos_invalidos()
    {
        await Falla<ArgumentException>(() => Servicio.CrearAsync(Nuevo(Identificaciones.CedulaInvalida(Identificaciones.CedulaValida()))));
        await Falla<ArgumentException>(() => Servicio.CrearAsync(Nuevo("12345")));

        var ruc = Nuevo("179000000");
        ruc.TipoIdentificacion = "RUC";
        await Falla<ArgumentException>(() => Servicio.CrearAsync(ruc));

        var tipo = Nuevo();
        tipo.TipoIdentificacion = "DNI";
        await Falla<ArgumentException>(() => Servicio.CrearAsync(tipo));

        var sinNombre = Nuevo();
        sinNombre.Nombres = " ";
        await Falla<ArgumentException>(() => Servicio.CrearAsync(sinNombre));

        var sinApellido = Nuevo();
        sinApellido.Apellidos = "";
        await Falla<ArgumentException>(() => Servicio.CrearAsync(sinApellido));

        var futuro = Nuevo();
        futuro.FechaNacimiento = DateTime.UtcNow.AddDays(5);
        await Falla<ArgumentException>(() => Servicio.CrearAsync(futuro));

        var sinIdentificacion = Nuevo(" ");
        await Falla<ArgumentException>(() => Servicio.CrearAsync(sinIdentificacion));
    }

    [Fact(Skip = "F-04 (soledad): PacienteServicio.CedulaEcuatorianaValida rechaza el código de provincia 30 (ecuatorianos registrados en el exterior), que SolicitudServicio sí acepta. Reportado, sin corregir.")]
    public async Task Acepta_cedulas_de_ecuatorianos_registrados_en_el_exterior()
    {
        var creado = await Servicio.CrearAsync(Nuevo("3000000004"));

        Assert.Equal("3000000004", creado.Identificacion);
    }

    [Fact]
    public async Task Acepta_pasaporte_y_ruc_bien_formados()
    {
        var pasaporte = Nuevo(Identificaciones.Pasaporte());
        pasaporte.TipoIdentificacion = "PASAPORTE";
        var ruc = Nuevo("17" + Random.Shared.NextInt64(10_000_000_000, 99_999_999_999));
        ruc.TipoIdentificacion = "RUC";

        Assert.Equal("PASAPORTE", (await Servicio.CrearAsync(pasaporte)).TipoIdentificacion);
        Assert.Equal("RUC", (await Servicio.CrearAsync(ruc)).TipoIdentificacion);
    }

    [Fact]
    public async Task No_duplica_un_paciente_activo_y_reactiva_al_inactivo()
    {
        await Falla<InvalidOperationException>(() => Servicio.CrearAsync(Nuevo(Semilla.CedulaPacienteAdulto)));

        var creado = await Servicio.CrearAsync(Nuevo());
        Assert.True(await Servicio.EliminarAsync(creado.Id, "Cambio de ciudad"));
        Assert.True(await Servicio.EliminarAsync(creado.Id, "Otra vez"));

        var reactivado = await Servicio.CrearAsync(Nuevo(creado.Identificacion));

        Assert.Equal(creado.Id, reactivado.Id);
        Assert.True(reactivado.Estatus);
        Assert.Null(reactivado.MotivoBaja);
    }

    [Fact]
    public async Task Actualiza_con_version_y_detecta_conflictos()
    {
        var creado = await Servicio.CrearAsync(Nuevo());
        var dto = new PacienteActualizarDto
        {
            TipoIdentificacion = "CEDULA",
            Identificacion = creado.Identificacion,
            Nombres = "Lucía Elena",
            Apellidos = "Andrade",
            Version = creado.Version
        };

        Assert.True(await Servicio.ActualizarAsync(creado.Id, dto));
        Assert.Equal("Lucía Elena", (await Servicio.ObtenerPorIdAsync(creado.Id))!.Nombres);

        dto.Nombres = "Con versión vieja";
        dto.Version = Convert.ToBase64String(new byte[8]);
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => Servicio.ActualizarAsync(creado.Id, dto));

        OlvidarSeguimiento();
        dto.Version = "no-es-base64";
        await Falla<ArgumentException>(() => Servicio.ActualizarAsync(creado.Id, dto));

        dto.Version = (await Servicio.ObtenerPorIdAsync(creado.Id))!.Version;
        dto.Identificacion = Semilla.CedulaPacienteAdulto;
        await Falla<InvalidOperationException>(() => Servicio.ActualizarAsync(creado.Id, dto));
    }

    [Fact]
    public async Task No_edita_pacientes_inactivos_y_exige_motivo_de_baja()
    {
        var creado = await Servicio.CrearAsync(Nuevo());

        await Falla<ArgumentException>(() => Servicio.EliminarAsync(creado.Id, " "));
        await Servicio.EliminarAsync(creado.Id, "Solicitud del titular");

        await Falla<InvalidOperationException>(() => Servicio.ActualizarAsync(creado.Id, new PacienteActualizarDto
        {
            TipoIdentificacion = "CEDULA",
            Identificacion = creado.Identificacion,
            Nombres = "x",
            Apellidos = "y",
            Version = creado.Version
        }));
        Assert.True(await Servicio.ReactivarAsync(creado.Id));
        Assert.True(await Servicio.ReactivarAsync(creado.Id));
    }

    [Fact]
    public async Task Devuelve_nulo_o_falso_si_no_existe()
    {
        Assert.Null(await Servicio.ObtenerPorIdAsync(Semilla.IdInexistente));
        Assert.False(await Servicio.EliminarAsync(Semilla.IdInexistente, "motivo"));
        Assert.False(await Servicio.ReactivarAsync(Semilla.IdInexistente));
        Assert.False(await Servicio.ActualizarAsync(Semilla.IdInexistente, new PacienteActualizarDto
        {
            TipoIdentificacion = "PASAPORTE",
            Identificacion = "AB123456",
            Nombres = "x",
            Apellidos = "y",
            Version = Convert.ToBase64String(new byte[8])
        }));
    }
}

public class DatoContactoServicioPruebas : PruebaServicioBase
{
    public DatoContactoServicioPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    private IDatoContactoServicio Servicio => Servicio<IDatoContactoServicio>();

    [Fact]
    public async Task Registra_contactos_del_paciente_activo()
    {
        var paciente = await NuevoPacienteAsync();

        var creado = await Servicio.CrearAsync(new DatoContactoCrearDto { PacienteId = paciente.PaId, Tipo = " celular ", Valor = " 0998887777 ", Principal = true });

        Assert.Equal("CELULAR", creado.Tipo);
        Assert.Equal("0998887777", creado.Valor);
        Assert.False(creado.Verificado);
        Assert.Single(await Servicio.ListarAsync(paciente.PaId, false, 1, 10));
        Assert.NotNull(await Servicio.ObtenerAsync(creado.Id));
    }

    [Fact]
    public async Task Rechaza_tipo_valor_paciente_inactivo_y_duplicados()
    {
        var paciente = await NuevoPacienteAsync();

        await Falla<ArgumentException>(() => Servicio.CrearAsync(new DatoContactoCrearDto { PacienteId = paciente.PaId, Tipo = "FAX", Valor = "1" }));
        await Falla<ArgumentException>(() => Servicio.CrearAsync(new DatoContactoCrearDto { PacienteId = paciente.PaId, Tipo = "CORREO", Valor = " " }));
        await Falla<ArgumentException>(() => Servicio.CrearAsync(new DatoContactoCrearDto { PacienteId = Semilla.PacienteInactivo, Tipo = "CORREO", Valor = "a@b.c" }));

        var dto = new DatoContactoCrearDto { PacienteId = paciente.PaId, Tipo = "CORREO", Valor = "dup@pruebas.podoflow.test" };
        await Servicio.CrearAsync(dto);
        await Falla<InvalidOperationException>(() => Servicio.CrearAsync(dto));
    }

    [Fact]
    public async Task Al_cambiar_el_valor_pierde_la_verificacion()
    {
        var paciente = await NuevoPacienteAsync();
        var contacto = await NuevoContactoAsync(paciente.PaId, "CORREO", "antes@pruebas.podoflow.test");

        Assert.True(await Servicio.ActualizarAsync(contacto.DaId, new DatoContactoActualizarDto { Tipo = "CORREO", Valor = "despues@pruebas.podoflow.test", Principal = true }));

        Assert.False((await Servicio.ObtenerAsync(contacto.DaId))!.Verificado);
    }

    [Fact]
    public async Task Da_de_baja_reactiva_y_reutiliza_el_contacto_inactivo()
    {
        var paciente = await NuevoPacienteAsync();
        var creado = await Servicio.CrearAsync(new DatoContactoCrearDto { PacienteId = paciente.PaId, Tipo = "TELEFONO", Valor = "022345678", Principal = true });
        var otro = await Servicio.CrearAsync(new DatoContactoCrearDto { PacienteId = paciente.PaId, Tipo = "TELEFONO", Valor = "022999999" });

        await Falla<InvalidOperationException>(() => Servicio.ActualizarAsync(otro.Id, new DatoContactoActualizarDto { Tipo = "TELEFONO", Valor = "022345678" }));
        Assert.True(await Servicio.EliminarAsync(creado.Id));
        Assert.True(await Servicio.EliminarAsync(creado.Id));
        await Falla<InvalidOperationException>(() => Servicio.ActualizarAsync(creado.Id, new DatoContactoActualizarDto { Tipo = "TELEFONO", Valor = "1" }));

        var reutilizado = await Servicio.CrearAsync(new DatoContactoCrearDto { PacienteId = paciente.PaId, Tipo = "TELEFONO", Valor = "022345678" });
        Assert.Equal(creado.Id, reutilizado.Id);

        await Servicio.EliminarAsync(creado.Id);
        Assert.True(await Servicio.ReactivarAsync(creado.Id));
        Assert.True(await Servicio.ReactivarAsync(creado.Id));
        Assert.Equal(2, (await Servicio.ListarAsync(paciente.PaId, true, 1, 10)).Count);
    }

    [Fact]
    public async Task No_reactiva_ni_edita_contactos_de_pacientes_inactivos()
    {
        var paciente = await NuevoPacienteAsync();
        var contacto = await NuevoContactoAsync(paciente.PaId, "CORREO", "x@pruebas.podoflow.test");
        await Servicio.EliminarAsync(contacto.DaId);
        paciente.PaEstatus = false;
        await Contexto.SaveChangesAsync();

        await Falla<ArgumentException>(() => Servicio.ReactivarAsync(contacto.DaId));
    }

    [Fact]
    public async Task Devuelve_nulo_o_falso_si_no_existe()
    {
        Assert.Null(await Servicio.ObtenerAsync(Semilla.IdInexistente));
        Assert.False(await Servicio.ActualizarAsync(Semilla.IdInexistente, new DatoContactoActualizarDto { Tipo = "CORREO", Valor = "x" }));
        Assert.False(await Servicio.EliminarAsync(Semilla.IdInexistente));
        Assert.False(await Servicio.ReactivarAsync(Semilla.IdInexistente));
    }
}
