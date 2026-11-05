
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace PodoFlow.Api.Servicios.Almacenamiento;

public sealed class AlmacenamientoAdjuntos
{
    public const long MaximoBytes = 10 * 1024 * 1024;

    private static readonly Regex PatronClave = new(
        @"\Aadjuntos/[a-f0-9]{32}\.(jpg|png|pdf)\z",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly string _directorio;

    public AlmacenamientoAdjuntos(
        IConfiguration configuracion,
        IWebHostEnvironment entorno)
    {
        var configurado =
            configuracion["AlmacenamientoClinico:Directorio"];

        if (string.IsNullOrWhiteSpace(configurado) ||
            !Path.IsPathFullyQualified(configurado))
        {
            throw new InvalidOperationException(
                "Configura una ruta absoluta para el almacenamiento clínico.");
        }

        _directorio = Path.GetFullPath(configurado);

        var webRoot = Path.GetFullPath(
            entorno.WebRootPath ??
            Path.Combine(entorno.ContentRootPath, "wwwroot"));

        var contentRoot = Path.GetFullPath(entorno.ContentRootPath);

        if (DentroDe(_directorio, webRoot) ||
            DentroDe(_directorio, contentRoot) ||
            DentroDe(contentRoot, _directorio))
        {
            throw new InvalidOperationException(
                "Los adjuntos deben almacenarse fuera del proyecto y de wwwroot.");
        }
    }

    public async Task<ArchivoClinicoGuardado> GuardarAsync(
        IFormFile archivo,
        CancellationToken ct)
    {
        if (archivo.Length <= 0 ||
            archivo.Length > MaximoBytes)
        {
            throw new ArgumentException(
                "El archivo debe tener entre 1 byte y 10 MB.");
        }

        var nombreOriginal = Path.GetFileName(
            archivo.FileName.Replace('\\', '/'));

        var extension = Path.GetExtension(nombreOriginal)
            .ToLowerInvariant();

        var tipo = extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".pdf" => "application/pdf",
            _ => throw new ArgumentException(
                "Solo se permiten archivos JPG, PNG y PDF.")
        };

        if (!string.Equals(
            archivo.ContentType,
            tipo,
            StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "El tipo de archivo no coincide con su extensión.");
        }

        Directory.CreateDirectory(_directorio);

        var extensionInterna =
            extension == ".jpeg" ? ".jpg" : extension;

        var nombreInterno =
            Guid.NewGuid().ToString("N") + extensionInterna;

        var clave = "adjuntos/" + nombreInterno;

        var temporal = Path.Combine(
            _directorio,
            Guid.NewGuid().ToString("N") + ".tmp");

        var destino = Path.Combine(_directorio, nombreInterno);

        var cabecera = new byte[8];
        int bytesCabecera = 0;
        long total = 0;

        using var sha = IncrementalHash.CreateHash(
            HashAlgorithmName.SHA256);

        try
        {
            await using (var origen = archivo.OpenReadStream())
            await using (var salida = new FileStream(
                temporal,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                81920,
                useAsync: true))
            {
                var buffer = new byte[81920];
                int cantidad;

                while ((cantidad = await origen.ReadAsync(
                    buffer.AsMemory(), ct)) > 0)
                {
                    total += cantidad;

                    if (total > MaximoBytes)
                    {
                        throw new ArgumentException(
                            "El archivo supera el límite de 10 MB.");
                    }

                    if (bytesCabecera < cabecera.Length)
                    {
                        var copiar = Math.Min(
                            cantidad,
                            cabecera.Length - bytesCabecera);

                        Array.Copy(
                            buffer, 0,
                            cabecera, bytesCabecera,
                            copiar);

                        bytesCabecera += copiar;
                    }

                    sha.AppendData(buffer, 0, cantidad);

                    await salida.WriteAsync(
                        buffer.AsMemory(0, cantidad), ct);
                }
            }

            if (total == 0 ||
                !FirmaValida(tipo, cabecera, bytesCabecera))
            {
                throw new ArgumentException(
                    "El contenido no corresponde al formato seleccionado.");
            }

            var huella = Convert.ToHexString(
                sha.GetHashAndReset()).ToLowerInvariant();

            File.Move(temporal, destino);

            return new ArchivoClinicoGuardado(
                clave,
                tipo,
                huella,
                total);
        }
        catch
        {
            if (File.Exists(temporal))
                File.Delete(temporal);

            throw;
        }
    }

    public async Task<FileStream?> AbrirVerificadoAsync(
        string clave,
        string? huella,
        long? tamano,
        CancellationToken ct)
    {
        var ruta = Resolver(clave);

        if (!File.Exists(ruta))
            return null;

        var archivo = new FileStream(
            ruta,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            81920,
            useAsync: true);

        try
        {
            if (!tamano.HasValue ||
                tamano.Value != archivo.Length ||
                string.IsNullOrWhiteSpace(huella))
            {
                throw new InvalidDataException(
                    "Los metadatos no permiten verificar el archivo.");
            }

            var calculada =
                await SHA256.HashDataAsync(archivo, ct);

            byte[] esperada;

            try
            {
                esperada = Convert.FromHexString(huella);
            }
            catch (FormatException)
            {
                throw new InvalidDataException(
                    "La huella almacenada es inválida.");
            }

            if (esperada.Length != calculada.Length ||
                !CryptographicOperations.FixedTimeEquals(
                    esperada, calculada))
            {
                throw new InvalidDataException(
                    "El archivo no supera la verificación de integridad.");
            }

            archivo.Position = 0;
            return archivo;
        }
        catch
        {
            await archivo.DisposeAsync();
            throw;
        }
    }

    private string Resolver(string clave)
    {
        if (!PatronClave.IsMatch(clave))
        {
            throw new InvalidDataException(
                "La clave interna del adjunto no es válida.");
        }

        return Path.Combine(
            _directorio,
            clave["adjuntos/".Length..]);
    }

    private static bool FirmaValida(
        string tipo, byte[] datos, int longitud)
    {
        if (tipo == "image/jpeg")
        {
            return longitud >= 3 &&
                   datos[0] == 0xFF &&
                   datos[1] == 0xD8 &&
                   datos[2] == 0xFF;
        }

        if (tipo == "image/png")
        {
            byte[] png =
                [137, 80, 78, 71, 13, 10, 26, 10];

            return longitud >= 8 &&
                   datos.AsSpan(0, 8).SequenceEqual(png);
        }

        return longitud >= 5 &&
               datos.AsSpan(0, 5).SequenceEqual("%PDF-"u8);
    }

    private static bool DentroDe(
        string ruta,
        string directorioPadre)
    {
        var padre = Path.TrimEndingDirectorySeparator(
            directorioPadre);

        var normalizada = Path.TrimEndingDirectorySeparator(
            ruta);

        return normalizada.Equals(
                   padre,
                   StringComparison.OrdinalIgnoreCase) ||
               normalizada.StartsWith(
                   padre + Path.DirectorySeparatorChar,
                   StringComparison.OrdinalIgnoreCase);
    }
}

public sealed record ArchivoClinicoGuardado(
    string Clave,
    string TipoMime,
    string Huella,
    long TamanoBytes);
