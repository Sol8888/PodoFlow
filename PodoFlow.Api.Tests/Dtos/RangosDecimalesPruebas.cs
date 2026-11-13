using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using PodoFlow.Api.DTOs.Cobro;

namespace PodoFlow.Api.Tests.Dtos;

public class RangosDecimalesPruebas
{
    public static TheoryData<string> Culturas => new() { "es-EC", "en-US", "" };

    private static IEnumerable<(string Campo, Func<RangeAttribute> Atributo)> RangosDecimales()
        => typeof(CobroCrearDto).Assembly.GetTypes()
            .Where(t => t.Namespace?.StartsWith("PodoFlow.Api.DTOs") == true)
            .SelectMany(t => t.GetProperties())
            .Where(p => p.GetCustomAttributes<RangeAttribute>().Any(r => r.OperandType == typeof(decimal)))
            .Select(p => (
                $"{p.DeclaringType!.Name}.{p.Name}",
                (Func<RangeAttribute>)(() => p.GetCustomAttributes<RangeAttribute>().First(r => r.OperandType == typeof(decimal)))));

    [Fact]
    public void Existen_rangos_decimales_para_revisar()
    {
        Assert.NotEmpty(RangosDecimales());
    }

    [Fact]
    public void Todos_los_rangos_decimales_usan_punto_y_cultura_invariante()
    {
        foreach (var (campo, atributo) in RangosDecimales())
        {
            var rango = atributo();

            Assert.True(rango.ParseLimitsInInvariantCulture, $"{campo} debe usar ParseLimitsInInvariantCulture = true.");
            Assert.True(rango.ConvertValueInInvariantCulture, $"{campo} debe usar ConvertValueInInvariantCulture = true.");
            Assert.DoesNotContain(",", (string)rango.Minimum);
            Assert.DoesNotContain(",", (string)rango.Maximum);
        }
    }

    [Theory]
    [MemberData(nameof(Culturas))]
    public void Los_limites_se_leen_igual_en_cualquier_idioma(string cultura)
    {
        var anterior = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultura);

            foreach (var (campo, atributo) in RangosDecimales())
            {
                var minimo = decimal.Parse((string)atributo().Minimum, CultureInfo.InvariantCulture);

                Assert.True(atributo().IsValid(minimo + 0.5m), $"{campo} rechaza {minimo + 0.5m} con la cultura '{cultura}'.");
                Assert.False(atributo().IsValid(minimo - 0.001m), $"{campo} acepta {minimo - 0.001m} con la cultura '{cultura}'.");
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = anterior;
        }
    }
}
