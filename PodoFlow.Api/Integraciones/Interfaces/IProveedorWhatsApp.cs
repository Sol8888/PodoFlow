using PodoFlow.Api.DTOs.Notificacion;

namespace PodoFlow.Api.Integraciones.Interfaces
{
    public interface IProveedorWhatsApp
    {
        Task<ResultadoProveedorDto> EnviarPlantillaAsync(string telefono, string nombrePlantilla, IReadOnlyList<string> parametros);
    }
}
