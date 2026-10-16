using PodoFlow.Api.DTOs.Ejecucion;
using PodoFlow.Api.Modelos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Servicios.Interfaces;

namespace PodoFlow.Api.Servicios.Implementaciones
{
    public class EjecucionServicio : IEjecucionServicio
    {
        private const string Pendiente = "PENDIENTE";
        private const string Procesando = "PROCESANDO";
        private const string Completada = "COMPLETADA";
        private const string Error = "ERROR";

        private const string CodigoAnalitica = "ANALITICA";
        private const string NivelSeudonimizado = "SEUDONIMIZADO";

        private const string MensajeEstadoInvalido =
            "El estado debe ser PENDIENTE, PROCESANDO, COMPLETADA o ERROR.";

        private static readonly Dictionary<string, string[]> TransicionesPermitidas = new()
        {
            [Pendiente] = [Procesando, Error],
            [Procesando] = [Completada, Error],
            [Completada] = [],
            [Error] = []
        };

        private static readonly HashSet<string> NivelesPrivacidad = ["AGREGADO", NivelSeudonimizado];

        private readonly IEjecucionRepositorio _repositorio;
        private readonly IFinalidadRepositorio _finalidadRepositorio;
        private readonly IEncargadoRepositorio _encargadoRepositorio;

        public EjecucionServicio(
            IEjecucionRepositorio repositorio,
            IFinalidadRepositorio finalidadRepositorio,
            IEncargadoRepositorio encargadoRepositorio)
        {
            _repositorio = repositorio;
            _finalidadRepositorio = finalidadRepositorio;
            _encargadoRepositorio = encargadoRepositorio;
        }

        public async Task<List<EjecucionRespuestaDto>> ObtenerTodosAsync(string? estado, int pagina, int tamano)
        {
            string? filtroEstado = null;

            if (!string.IsNullOrWhiteSpace(estado))
            {
                filtroEstado = Normalizar(estado);

                if (!TransicionesPermitidas.ContainsKey(filtroEstado))
                    throw new ArgumentException(MensajeEstadoInvalido);
            }

            var ejecuciones = await _repositorio.ObtenerTodosAsync(filtroEstado, pagina, tamano);

            return ejecuciones.Select(MapearRespuesta).ToList();
        }

        public async Task<EjecucionRespuestaDto?> ObtenerPorIdAsync(long id)
        {
            var ejecucion = await _repositorio.ObtenerPorIdAsync(id);

            return ejecucion == null ? null : MapearRespuesta(ejecucion);
        }

        public async Task<List<DatoAnaliticoRespuestaDto>?> ObtenerDatosAsync(long id, int pagina, int tamano)
        {
            var ejecucion = await _repositorio.ObtenerPorIdAsync(id);

            if (ejecucion == null)
                return null;

            var datos = await _repositorio.ObtenerDatosAsync(id, pagina, tamano);

            return datos.Select(MapearDato).ToList();
        }

        public async Task<List<RecomendacionRespuestaDto>?> ObtenerRecomendacionesAsync(long id)
        {
            var ejecucion = await _repositorio.ObtenerPorIdAsync(id);

            if (ejecucion == null)
                return null;

            var recomendaciones = await _repositorio.ObtenerRecomendacionesAsync(id);

            return recomendaciones.Select(MapearRecomendacion).ToList();
        }

        public async Task<EjecucionRespuestaDto> CrearAsync(
            EjecucionCrearDto dto)
        {
            if (!await _repositorio.ExisteUsuarioActivoAsync(dto.UsuarioId))
                throw new ArgumentException($"No existe un usuario activo con id {dto.UsuarioId}.");

            await ValidarEncargadoAsync(dto.EncargadoId);

            var finalidad = await ObtenerFinalidadAnaliticaAsync();

            var ejecucion = new Ejecucion
            {
                UsId = dto.UsuarioId,
                EnId = dto.EncargadoId,
                FiId = finalidad.FiId,
                EjFechaHora = DateTime.UtcNow,
                EjEstad = Pendiente,
                EjCantReg = 0,
                EjObser = Limpiar(dto.Observacion)
            };

            var creada = await _repositorio.CrearAsync(ejecucion);

            return MapearRespuesta(creada);
        }

        public async Task<bool> ActualizarAsync(
            long id,
            EjecucionActualizarDto dto)
        {
            var ejecucion = await _repositorio.ObtenerPorIdAsync(id);

            if (ejecucion == null)
                return false;

            if (ejecucion.EjEstad != Pendiente)
                throw new InvalidOperationException(
                    $"La ejecución está {ejecucion.EjEstad}; solo se puede modificar mientras está PENDIENTE.");

            await ValidarEncargadoAsync(dto.EncargadoId);

            ejecucion.EnId = dto.EncargadoId;
            ejecucion.EjObser = Limpiar(dto.Observacion);

            await _repositorio.GuardarCambiosAsync();

            return true;
        }

        public async Task<EjecucionRespuestaDto?> CambiarEstadoAsync(
            long id,
            EjecucionCambiarEstadoDto dto)
        {
            var nuevoEstado = Normalizar(dto.Estado);

            if (!TransicionesPermitidas.ContainsKey(nuevoEstado))
                throw new ArgumentException(MensajeEstadoInvalido);

            var ejecucion = await _repositorio.ObtenerPorIdAsync(id);

            if (ejecucion == null)
                return null;

            if (!TransicionesPermitidas[ejecucion.EjEstad].Contains(nuevoEstado))
                throw new InvalidOperationException(
                    $"No se puede pasar de {ejecucion.EjEstad} a {nuevoEstado}.");

            var huellaEntrada = Limpiar(dto.HuellaEntrada);
            var huellaSalida = Limpiar(dto.HuellaSalida);
            var observacion = Limpiar(dto.Observacion);

            if (nuevoEstado == Procesando)
            {
                if (huellaEntrada == null)
                    throw new ArgumentException("Para pasar a PROCESANDO hay que indicar la huella de los datos de entrada.");

                ejecucion.EjHuellEntr = huellaEntrada;
            }

            if (nuevoEstado == Completada)
            {
                if (huellaSalida == null)
                    throw new ArgumentException("Para pasar a COMPLETADA hay que indicar la huella de los resultados.");

                ejecucion.EjHuellSali = huellaSalida;
                ejecucion.EjCantReg = await _repositorio.ContarDatosAsync(ejecucion.EjId);
            }

            if (nuevoEstado == Error && observacion == null)
                throw new ArgumentException("Para pasar a ERROR hay que indicar el motivo en la observación.");

            if (observacion != null)
                ejecucion.EjObser = observacion;

            ejecucion.EjEstad = nuevoEstado;

            await _repositorio.GuardarCambiosAsync();

            return MapearRespuesta(ejecucion);
        }

        public async Task<int?> CargarDatosAsync(
            long id,
            DatoAnaliticoLoteDto dto)
        {
            var ejecucion = await _repositorio.ObtenerPorIdAsync(id);

            if (ejecucion == null)
                return null;

            if (ejecucion.EjEstad != Procesando)
                throw new InvalidOperationException(
                    $"La ejecución está {ejecucion.EjEstad}; solo se cargan datos mientras está PROCESANDO.");

            var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

            for (var i = 0; i < dto.Datos.Count; i++)
            {
                var dato = dto.Datos[i];

                if (!dato.SeudonimoId.HasValue || dato.SeudonimoId.Value == Guid.Empty)
                    throw new ArgumentException($"Dato {i + 1}: el seudónimo es obligatorio.");

                if (dato.FechaUltimaAtencion.HasValue && dato.FechaUltimaAtencion.Value > hoy)
                    throw new ArgumentException($"Dato {i + 1}: la fecha de la última atención no puede estar en el futuro.");
            }

            var seudonimos = dto.Datos.Select(x => x.SeudonimoId!.Value).ToList();

            var repetidos = seudonimos.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key).ToList();

            if (repetidos.Count > 0)
                throw new ArgumentException(
                    $"El lote repite seudónimos: {Resumir(repetidos)}. Va un registro por seudónimo.");

            var yaCargados = await _repositorio.ObtenerSeudonimosCargadosAsync(id, seudonimos);

            if (yaCargados.Count > 0)
                throw new InvalidOperationException(
                    $"Estos seudónimos ya tienen datos en la ejecución {id}: {Resumir(yaCargados)}.");

            var autorizados = (await _repositorio.ObtenerSeudonimosAutorizadosAsync(seudonimos, ejecucion.FiId)).ToHashSet();

            var noAutorizados = seudonimos.Where(x => !autorizados.Contains(x)).ToList();

            if (noAutorizados.Count > 0)
                throw new InvalidOperationException(
                    $"Estos seudónimos no existen, están dados de baja o su paciente no tiene el consentimiento " +
                    $"{CodigoAnalitica} aceptado: {Resumir(noAutorizados)}.");

            var datos = dto.Datos.Select(x => new DatoAnalitico
            {
                EjId = id,
                SeId = x.SeudonimoId!.Value,
                DaFrecAten = x.FrecuenciaAtenciones,
                DaCategServ = Limpiar(x.CategoriaServicio),
                DaFechaUlti = x.FechaUltimaAtencion,
                DaDiasInact = x.DiasInactividad
            }).ToList();

            await _repositorio.CrearDatosAsync(datos);

            return datos.Count;
        }

        public async Task<RecomendacionRespuestaDto?> CrearRecomendacionAsync(
            long id,
            RecomendacionCrearDto dto)
        {
            var ejecucion = await _repositorio.ObtenerPorIdAsync(id);

            if (ejecucion == null)
                return null;

            if (ejecucion.EjEstad != Completada)
                throw new InvalidOperationException(
                    $"La ejecución está {ejecucion.EjEstad}; las recomendaciones se registran cuando está COMPLETADA.");

            var nivel = string.IsNullOrWhiteSpace(dto.NivelPrivacidad)
                ? NivelSeudonimizado
                : Normalizar(dto.NivelPrivacidad);

            if (!NivelesPrivacidad.Contains(nivel))
                throw new ArgumentException("El nivel de privacidad debe ser AGREGADO o SEUDONIMIZADO.");

            if (dto.PacientesEstimados > ejecucion.EjCantReg)
                throw new ArgumentException(
                    $"Los pacientes estimados ({dto.PacientesEstimados}) no pueden superar " +
                    $"los registros analizados en la ejecución ({ejecucion.EjCantReg}).");

            var recomendacion = new Recomendacion
            {
                EjId = id,
                ReSegme = dto.Segmento.Trim(),
                ReHallaz = dto.Hallazgo.Trim(),
                ReSugere = dto.Sugerencia.Trim(),
                RePacieEstim = dto.PacientesEstimados,
                ReNivelPriva = nivel,
                ReFechaGene = DateTime.UtcNow
            };

            var creada = await _repositorio.CrearRecomendacionAsync(recomendacion);

            return MapearRecomendacion(creada);
        }

        private async Task ValidarEncargadoAsync(int? encargadoId)
        {
            if (!encargadoId.HasValue)
                return;

            var encargado = await _encargadoRepositorio.ObtenerPorIdAsync(encargadoId.Value);

            if (encargado == null || !encargado.EnEstatus)
                throw new ArgumentException($"No existe un encargado activo con id {encargadoId.Value}.");
        }

        private async Task<Finalidad> ObtenerFinalidadAnaliticaAsync()
        {
            var finalidad = await _finalidadRepositorio.ObtenerPorCodigoAsync(CodigoAnalitica);

            if (finalidad == null || !finalidad.FiEstatus)
                throw new InvalidOperationException($"La finalidad {CodigoAnalitica} no existe o está dada de baja.");

            return finalidad;
        }

        private static string Resumir(List<Guid> seudonimos)
        {
            var primeros = string.Join(", ", seudonimos.Take(5));

            return seudonimos.Count > 5 ? $"{primeros} y {seudonimos.Count - 5} más" : primeros;
        }

        private static string Normalizar(string valor)
        {
            return valor.Trim().ToUpperInvariant();
        }

        private static string? Limpiar(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }

        private static EjecucionRespuestaDto MapearRespuesta(
            Ejecucion ejecucion)
        {
            return new EjecucionRespuestaDto
            {
                Id = ejecucion.EjId,
                UsuarioId = ejecucion.UsId,
                EncargadoId = ejecucion.EnId,
                FinalidadId = ejecucion.FiId,
                FechaHora = ejecucion.EjFechaHora,
                Estado = ejecucion.EjEstad,
                CantidadRegistros = ejecucion.EjCantReg,
                HuellaEntrada = ejecucion.EjHuellEntr,
                HuellaSalida = ejecucion.EjHuellSali,
                Observacion = ejecucion.EjObser
            };
        }

        private static DatoAnaliticoRespuestaDto MapearDato(
            DatoAnalitico dato)
        {
            return new DatoAnaliticoRespuestaDto
            {
                Id = dato.DaId,
                EjecucionId = dato.EjId,
                SeudonimoId = dato.SeId,
                FrecuenciaAtenciones = dato.DaFrecAten,
                CategoriaServicio = dato.DaCategServ,
                FechaUltimaAtencion = dato.DaFechaUlti,
                DiasInactividad = dato.DaDiasInact
            };
        }

        private static RecomendacionRespuestaDto MapearRecomendacion(
            Recomendacion recomendacion)
        {
            return new RecomendacionRespuestaDto
            {
                Id = recomendacion.ReId,
                EjecucionId = recomendacion.EjId,
                Segmento = recomendacion.ReSegme,
                Hallazgo = recomendacion.ReHallaz,
                Sugerencia = recomendacion.ReSugere,
                PacientesEstimados = recomendacion.RePacieEstim,
                NivelPrivacidad = recomendacion.ReNivelPriva,
                FechaGeneracion = recomendacion.ReFechaGene
            };
        }
    }
}
