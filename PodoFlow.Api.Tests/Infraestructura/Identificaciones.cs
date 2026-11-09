namespace PodoFlow.Api.Tests.Infraestructura;

public static class Identificaciones
{
    public static string CedulaValida()
    {
        var digitos = new int[10];
        var provincia = Random.Shared.Next(1, 25);

        digitos[0] = provincia / 10;
        digitos[1] = provincia % 10;
        digitos[2] = Random.Shared.Next(0, 6);

        for (var i = 3; i < 9; i++)
            digitos[i] = Random.Shared.Next(0, 10);

        var suma = 0;

        for (var i = 0; i < 9; i++)
        {
            var producto = digitos[i] * (i % 2 == 0 ? 2 : 1);
            suma += producto > 9 ? producto - 9 : producto;
        }

        digitos[9] = (10 - suma % 10) % 10;

        return string.Concat(digitos);
    }

    public static string CedulaInvalida(string valida)
    {
        var ultimo = valida[9] - '0';
        return valida[..9] + ((ultimo + 1) % 10);
    }

    public static string Pasaporte()
        => "PZ" + Random.Shared.Next(100000, 999999);

    public static string Sufijo()
        => Guid.NewGuid().ToString("N")[..10].ToUpperInvariant();
}
