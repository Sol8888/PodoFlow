using System.Collections.Concurrent;
using PodoFlow.Api.DTOs.Notificacion;
using PodoFlow.Api.Integraciones.Interfaces;

namespace PodoFlow.Api.Tests.Infraestructura;

public sealed record CorreoEnviado(string Destino, string NombreDestino, string Asunto, string ContenidoHtml);

public sealed record WhatsAppEnviado(string Telefono, string NombrePlantilla, IReadOnlyList<string> Parametros);

public sealed class ProveedorCorreoFalso : IProveedorCorreo
{
    public ConcurrentQueue<CorreoEnviado> Enviados { get; } = new();

    public bool Fallar { get; set; }

    public Task<ResultadoProveedorDto> EnviarAsync(string destino, string nombreDestino, string asunto, string contenidoHtml)
    {
        Enviados.Enqueue(new CorreoEnviado(destino, nombreDestino, asunto, contenidoHtml));

        return Task.FromResult(Fallar
            ? new ResultadoProveedorDto { Exitoso = false, CodigoError = "FALSO", DetalleError = "Fallo simulado del proveedor de correo." }
            : new ResultadoProveedorDto { Exitoso = true, IdProveedor = "correo-falso-" + Enviados.Count });
    }

    public void Reiniciar()
    {
        Enviados.Clear();
        Fallar = false;
    }
}

public sealed class ProveedorWhatsAppFalso : IProveedorWhatsApp
{
    public ConcurrentQueue<WhatsAppEnviado> Enviados { get; } = new();

    public bool Fallar { get; set; }

    public Task<ResultadoProveedorDto> EnviarPlantillaAsync(string telefono, string nombrePlantilla, IReadOnlyList<string> parametros)
    {
        Enviados.Enqueue(new WhatsAppEnviado(telefono, nombrePlantilla, parametros));

        return Task.FromResult(Fallar
            ? new ResultadoProveedorDto { Exitoso = false, CodigoError = "FALSO", DetalleError = "Fallo simulado del proveedor de WhatsApp." }
            : new ResultadoProveedorDto { Exitoso = true, IdProveedor = "wa-falso-" + Enviados.Count });
    }

    public void Reiniciar()
    {
        Enviados.Clear();
        Fallar = false;
    }
}
