using PodoFlow.Api.DTOs.Documento;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones
{
    public class DocumentoServicio : IDocumentoServicio
    {
        // Mismos valores que CK_DOCUMENTO_tipo.
        private static readonly HashSet<string> Tipos =
        [
            "RAT", "EIPD", "MATRIZ_RIESGO", "MATRIZ_ENCARGADO",
            "EVIDENCIA_CONTROL", "AVISO_PRIVACIDAD", "OTRO"
        ];

        private readonly IDocumentoRepositorio _repositorio;

        public DocumentoServicio(IDocumentoRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public async Task<List<DocumentoRespuestaDto>> ObtenerTodosAsync(
            string? tipo, bool incluirInactivos, int pagina, int tamano)
        {
            var filtroTipo = string.IsNullOrWhiteSpace(tipo) ? null : NormalizarTipo(tipo);

            var documentos = await _repositorio.ObtenerTodosAsync(filtroTipo, incluirInactivos, pagina, tamano);

            return documentos.Select(MapearRespuesta).ToList();
        }

        public async Task<DocumentoRespuestaDto?> ObtenerPorIdAsync(long id)
        {
            var documento = await _repositorio.ObtenerPorIdAsync(id);

            return documento == null ? null : MapearRespuesta(documento);
        }

        public async Task<DocumentoRespuestaDto> CrearAsync(
            DocumentoCrearDto dto)
        {
            var tipo = NormalizarTipo(dto.Tipo);
            var nombre = dto.Nombre.Trim();
            var version = Limpiar(dto.Version);

            if (!await _repositorio.ExisteUsuarioActivoAsync(dto.UsuarioId))
                throw new ArgumentException($"No existe un usuario activo con id {dto.UsuarioId}.");

            await ValidarDuplicadoAsync(tipo, nombre, version, null);

            var documento = new Documento
            {
                UsId = dto.UsuarioId,
                DoTipo = tipo,
                DoVersi = version,
                DoNombre = nombre,
                DoRutaArch = dto.RutaArchivo.Trim(),
                DoHuella = Limpiar(dto.Huella),
                DoFechaCarg = DateTime.UtcNow,
                DoEstatus = true
            };

            var creado = await _repositorio.CrearAsync(documento);

            return MapearRespuesta(creado);
        }

        public async Task<bool> ActualizarAsync(
            long id,
            DocumentoActualizarDto dto)
        {
            var documento = await _repositorio.ObtenerPorIdAsync(id);

            if (documento == null)
                return false;

            var tipo = NormalizarTipo(dto.Tipo);
            var nombre = dto.Nombre.Trim();
            var version = Limpiar(dto.Version);

            await ValidarDuplicadoAsync(tipo, nombre, version, id);

            documento.DoTipo = tipo;
            documento.DoNombre = nombre;
            documento.DoVersi = version;

            await _repositorio.GuardarCambiosAsync();

            return true;
        }

        public async Task<bool> EliminarAsync(long id)
        {
            var documento = await _repositorio.ObtenerPorIdAsync(id);

            if (documento == null)
                return false;

            if (!documento.DoEstatus)
                return true;

            documento.DoEstatus = false;

            await _repositorio.GuardarCambiosAsync();

            return true;
        }

        public async Task<bool> ReactivarAsync(long id)
        {
            var documento = await _repositorio.ObtenerPorIdAsync(id);

            if (documento == null)
                return false;

            // Si ya estaba activo no hay nada que hacer.
            if (documento.DoEstatus)
                return true;

            // Al reactivarlo no puede quedar duplicado con otro que ya esté activo.
            var duplicado = await _repositorio.ObtenerDuplicadoAsync(
                documento.DoTipo, documento.DoNombre, documento.DoVersi, id);

            if (duplicado != null && duplicado.DoEstatus)
                throw new InvalidOperationException(
                    $"Ya hay un documento activo igual (id {duplicado.DoId}). Dé de baja ese antes de reactivar este.");

            documento.DoEstatus = true;

            await _repositorio.GuardarCambiosAsync();

            return true;
        }
        private async Task ValidarDuplicadoAsync(string tipo, string nombre, string? version, long? excluirId)
        {
            var duplicado = await _repositorio.ObtenerDuplicadoAsync(tipo, nombre, version, excluirId);

            if (duplicado == null)
                return;

            var descripcion = $"'{duplicado.DoNombre}' ({duplicado.DoTipo}{(duplicado.DoVersi == null ? "" : " v" + duplicado.DoVersi)})";

            if (duplicado.DoEstatus)
                throw new InvalidOperationException(
                    $"Ya existe el documento {descripcion} (id {duplicado.DoId}). Si es un archivo nuevo, use otra versión.");

            throw new InvalidOperationException(
                $"Ya existe el documento {descripcion} dado de baja (id {duplicado.DoId}). " +
                $"Reactívelo con PATCH api/Documento/{duplicado.DoId}/reactivar o use otra versión.");
        }

        private static string NormalizarTipo(string tipo)
        {
            var normalizado = tipo.Trim().ToUpperInvariant();

            if (!Tipos.Contains(normalizado))
                throw new ArgumentException(
                    $"Tipo de documento inválido. Valores permitidos: {string.Join(", ", Tipos)}.");

            return normalizado;
        }
        private static string? Limpiar(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }

        private static DocumentoRespuestaDto MapearRespuesta(
            Documento documento)
        {
            return new DocumentoRespuestaDto
            {
                Id = documento.DoId,
                UsuarioId = documento.UsId,
                Tipo = documento.DoTipo,
                Version = documento.DoVersi,
                Nombre = documento.DoNombre,
                RutaArchivo = documento.DoRutaArch,
                Huella = documento.DoHuella,
                FechaCarga = documento.DoFechaCarg,
                Estatus = documento.DoEstatus
            };
        }
    }
}
