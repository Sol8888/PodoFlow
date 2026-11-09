using Microsoft.EntityFrameworkCore;

namespace PodoFlow.Api.Tests.Infraestructura;

public class SeguridadBaseDatosPruebas : PruebaServicioBase
{
    public SeguridadBaseDatosPruebas(FabricaApi fabrica) : base(fabrica)
    {
    }

    [Fact]
    public async Task La_api_usa_solo_la_base_de_pruebas()
    {
        var nombre = await Contexto.Database.SqlQueryRaw<string>("SELECT DB_NAME() AS [Value]").SingleAsync();

        Assert.Equal(ConfiguracionPruebas.BaseDatos, nombre);
    }

    [Fact]
    public async Task La_base_de_pruebas_tiene_los_datos_sinteticos()
    {
        Assert.True(await Contexto.Usuarios.AnyAsync(u => u.UsId == Semilla.UsuarioAdmin));
        Assert.True(await Contexto.Pacientes.AnyAsync(p => p.PaId == Semilla.PacienteMenor));
        Assert.True(await Contexto.Avisos.AnyAsync(a => a.AvId == Semilla.Aviso));
    }

    [Fact]
    public async Task Las_transacciones_internas_se_revierten_con_la_prueba()
    {
        var usuario = await NuevoUsuarioAsync();

        await using (var interna = await Contexto.Database.BeginTransactionAsync())
        {
            usuario.UsNombre = "Cambio revertido";
            await Contexto.SaveChangesAsync();
        }

        OlvidarSeguimiento();

        var guardado = await Contexto.Usuarios.SingleAsync(u => u.UsId == usuario.UsId);
        Assert.NotEqual("Cambio revertido", guardado.UsNombre);
    }
}
