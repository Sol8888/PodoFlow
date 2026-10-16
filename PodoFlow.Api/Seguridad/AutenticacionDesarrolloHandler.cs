
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace PodoFlow.Api.Seguridad;

public class AutenticacionDesarrolloHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IConfiguration _configuration;

    public AutenticacionDesarrolloHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IConfiguration configuration)
        : base(options, logger, encoder)
    {
        _configuration = configuration;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Context.RequestServices
            .GetRequiredService<IHostEnvironment>()
            .IsDevelopment())
        {
            return Task.FromResult(
                AuthenticateResult.NoResult());
        }

        var esperado = _configuration["AutenticacionDesarrollo:Token"];
        var recibido = Request.Headers["X-Dev-Token"].ToString();

        if (string.IsNullOrWhiteSpace(esperado) ||
            string.IsNullOrWhiteSpace(recibido))
        {
            return Task.FromResult(
                AuthenticateResult.NoResult());
        }

        var bytesEsperados = SHA256.HashData(
            Encoding.UTF8.GetBytes(esperado));

        var bytesRecibidos = SHA256.HashData(
            Encoding.UTF8.GetBytes(recibido));

        if (!CryptographicOperations.FixedTimeEquals(
            bytesEsperados, bytesRecibidos))
        {
            return Task.FromResult(
                AuthenticateResult.Fail("Token de desarrollo inválido."));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "desarrollo-local"),
            new Claim(ClaimTypes.Name, "Usuario de pruebas"),
            new Claim("permiso", "HISTORIA_GESTIONAR"),
            new Claim("permiso", "AUDITORIA_VER"),
            new Claim("permiso", "FINANZAS_GESTIONAR"),
            new Claim("permiso", "PRIVACIDAD_GESTIONAR"),
            new Claim("permiso", "COMUNICACION_GESTIONAR"),
            new Claim("permiso", "ANALITICA_VER"),

        };

        var identidad = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identidad);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(
            AuthenticateResult.Success(ticket));
    }
}
