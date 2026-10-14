using PodoFlow.Api.DTOs.EvidenciaSolicitud;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones
{
    public class EvidenciaSolicitudServicio : IEvidenciaSolicitudServicio
    {
        private readonly IEvidenciaSolicitudRepositorio _repositorio;
        private readonly ISolicitudRepositorio _solicitudRepositorio;

        public EvidenciaSolicitudServicio(
            IEvidenciaSolicitudRepositorio repositorio,
            ISolicitudRepositorio solicitudRepositorio)
        {
            _repositorio = repositorio;
            _solicitudRepositorio = solicitudRepositorio;
        }

        // null si la solicitud no existe (→ 404); lista vacía si existe pero no tiene evidencias.
        public async Task<List<EvidenciaSolicitudRespuestaDto>?> ObtenerPorSolicitudAsync(long solicitudId)
        {
            var solicitud = await _solicitudRepositorio.ObtenerPorIdAsync(solicitudId);

            if (solicitud == null)
                return null;

            var evidencias = await _repositorio.ObtenerPorSolicitudAsync(solicitudId);

            return evidencias.Select(MapearRespuesta).ToList();
        }

        public async Task<EvidenciaSolicitudRespuestaDto?> ObtenerPorIdAsync(long id)
        {
            var evidencia = await _repositorio.ObtenerPorIdAsync(id);

            if (evidencia == null)
                return null;

            return MapearRespuesta(evidencia);
        }

        public async Task<EvidenciaSolicitudRespuestaDto> CrearAsync(
            EvidenciaSolicitudCrearDto dto)
        {
            var solicitud = await _solicitudRepositorio.ObtenerPorIdAsync(dto.SolicitudId);

            if (solicitud == null)
                throw new ArgumentException($"No existe la solicitud {dto.SolicitudId}.");

            // Una solicitud terminada ya no recibe documentos: su expediente queda cerrado.
            if (solicitud.SoEstad == "CERRADA" || solicitud.SoEstad == "RECHAZADA")
                throw new InvalidOperationException(
                    $"La solicitud está {solicitud.SoEstad} y ya no admite evidencias.");

            if (!await _solicitudRepositorio.ExisteUsuarioActivoAsync(dto.UsuarioId))
                throw new ArgumentException($"No existe un usuario activo con id {dto.UsuarioId}.");

            var tipo = dto.Tipo.Trim().ToUpperInvariant();
            var ahora = DateTime.UtcNow;

            var evidencia = new EvidenciaSolicitud
            {
                SoId = dto.SolicitudId,
                UsId = dto.UsuarioId,
                EvTipo = tipo,
                EvRutaArch = dto.RutaArchivo.Trim(),
                EvHuella = string.IsNullOrWhiteSpace(dto.Huella) ? null : dto.Huella.Trim(),
                EvFechaCarg = ahora
            };

            // Cargar un documento es una gestión de la solicitud, así que queda en su bitácora.
            // ac_estad_ante y ac_estad_nuev admiten NULL justamente para acciones sin cambio de estado.
            var accion = new AccionSolicitud
            {
                SoId = dto.SolicitudId,
                UsId = dto.UsuarioId,
                AcFechaHora = ahora,
                AcEstadAnte = null,
                AcEstadNuev = null,
                AcAccion = $"Evidencia cargada: {tipo}",
                AcObser = dto.Observacion?.Trim()
            };

            var creada = await _repositorio.CrearAsync(evidencia, accion);

            return MapearRespuesta(creada);
        }

        private static EvidenciaSolicitudRespuestaDto MapearRespuesta(
            EvidenciaSolicitud evidencia)
        {
            return new EvidenciaSolicitudRespuestaDto
            {
                Id = evidencia.EvId,
                SolicitudId = evidencia.SoId,
                UsuarioId = evidencia.UsId,
                Tipo = evidencia.EvTipo,
                RutaArchivo = evidencia.EvRutaArch,
                Huella = evidencia.EvHuella,
                FechaCarga = evidencia.EvFechaCarg
            };
        }
    }
}