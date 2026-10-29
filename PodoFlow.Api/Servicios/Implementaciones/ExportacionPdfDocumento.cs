using System.Globalization;
using PodoFlow.Api.DTOs.Exportacion;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PodoFlow.Api.Servicios.Implementaciones
{
    public class ExportacionPdfDocumento : IDocument
    {
        private const string Navy = "#0C2D48";
        private const string Azul = "#145DA0";
        private const string Naranja = "#FF6B35";
        private const string Hielo = "#F0F6FA";
        private const string Texto = "#1E293B";
        private const string TextoSuave = "#64748B";
        private const string TextoTenue = "#94A3B8";
        private const string Borde = "#E2E8F0";
        private const string BordeTabla = "#D6E2EE";
        private const string FondoNota = "#F8FAFC";
        private const string Verde = "#047857";
        private const string Rojo = "#B91C1C";
        private const string SinDato = "—";

        private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-EC");
        private static readonly TimeZoneInfo ZonaEcuador = ObtenerZonaEcuador();

        private static readonly Dictionary<string, string> TitulosSeccion = new()
        {
            ["PERSONALES"] = "Identificación del titular",
            ["CITAS"] = "Citas",
            ["CONSENTIMIENTOS"] = "Consentimientos otorgados",
            ["COMUNICACIONES"] = "Comunicaciones",
            ["SOLICITUDES"] = "Solicitudes de derechos",
            ["CLINICA"] = "Historia clínica podológica"
        };

        private static readonly HashSet<string> SiglasConocidas = ["RUC", "LOPDP"];

        private static readonly Dictionary<string, string> PalabrasConTilde = new()
        {
            ["cedula"] = "cédula",
            ["informacion"] = "información",
            ["rectificacion"] = "rectificación",
            ["actualizacion"] = "actualización",
            ["eliminacion"] = "eliminación",
            ["oposicion"] = "oposición",
            ["suspension"] = "suspensión",
            ["analitica"] = "analítica",
            ["gestion"] = "gestión",
            ["atencion"] = "atención",
            ["notificacion"] = "notificación",
            ["clinica"] = "clínica",
            ["clinico"] = "clínico",
            ["podologica"] = "podológica",
            ["telefono"] = "teléfono",
            ["electronico"] = "electrónico"
        };

        private readonly DatosConsultorioDto _consultorio;
        private readonly long _solicitudId;
        private readonly string _codigo;
        private readonly DateTime _fechaGeneracion;
        private readonly List<string> _secciones;
        private readonly Dictionary<string, List<Dictionary<string, object?>>> _datos;
        private readonly string _huellaDatos;

        static ExportacionPdfDocumento()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public ExportacionPdfDocumento(
            DatosConsultorioDto consultorio,
            long solicitudId,
            string codigo,
            DateTime fechaGeneracion,
            List<string> secciones,
            Dictionary<string, List<Dictionary<string, object?>>> datos,
            string huellaDatos)
        {
            _consultorio = consultorio;
            _solicitudId = solicitudId;
            _codigo = codigo;
            _fechaGeneracion = fechaGeneracion;
            _secciones = secciones;
            _datos = datos;
            _huellaDatos = huellaDatos;
        }

        public DocumentMetadata GetMetadata()
        {
            return new DocumentMetadata
            {
                Title = $"Informe de datos personales - Solicitud {_solicitudId}",
                Author = _consultorio.Nombre,
                Subject = "Derecho de acceso (LOPDP)",
                Creator = "PodoFlow"
            };
        }

        public void Compose(IDocumentContainer container)
        {
            container.Page(pagina =>
            {
                pagina.Size(PageSizes.A4);
                pagina.Margin(1.5f, Unit.Centimetre);
                pagina.DefaultTextStyle(estilo => estilo.FontSize(9.5f).FontColor(Texto));
                pagina.Content().Element(ComponerContenido);
                pagina.Footer().Element(ComponerPie);
            });
        }

        private void ComponerContenido(IContainer container)
        {
            container.Column(columna =>
            {
                columna.Spacing(6);
                columna.Item().Element(ComponerEncabezado);

                columna.Item().PaddingTop(6).Background(Navy).PaddingVertical(6).PaddingHorizontal(9)
                    .Text("INFORME DE DATOS PERSONALES DEL TITULAR · DERECHO DE ACCESO (LOPDP)")
                    .FontColor(Colors.White).Bold().FontSize(9.5f);

                columna.Item().Text(texto =>
                {
                    texto.DefaultTextStyle(estilo => estilo.FontSize(8).FontColor(TextoSuave));
                    texto.Span("Solicitud: ");
                    texto.Span($"#{_solicitudId}").Bold().FontColor(Texto);
                    texto.Span("    Fecha de emisión: ");
                    texto.Span(FechaHora(_fechaGeneracion)).Bold().FontColor(Texto);
                    texto.Span("    Secciones: ");
                    texto.Span(string.Join(", ", _secciones.Select(TituloCorto))).Bold().FontColor(Texto);
                });

                var numero = 1;

                foreach (var seccion in _secciones)
                {
                    var actual = numero++;
                    columna.Item().Element(contenedor => ComponerSeccion(contenedor, actual, seccion));
                }

                columna.Item().PaddingTop(14).Element(ComponerClausula);
                columna.Item().PaddingTop(18).Element(ComponerCierre);
            });
        }

        private void ComponerEncabezado(IContainer container)
        {
            container.BorderBottom(2).BorderColor(Azul).PaddingBottom(9).Row(fila =>
            {
                fila.ConstantItem(30).Height(30).Background(Naranja).AlignCenter().AlignMiddle()
                    .Text("P").FontColor(Colors.White).FontSize(16).Bold();

                fila.ConstantItem(9);

                fila.RelativeItem().Column(marca =>
                {
                    marca.Item().Text(_consultorio.Nombre).FontSize(15).Bold().FontColor(Navy);

                    if (!string.IsNullOrWhiteSpace(_consultorio.Subtitulo))
                        marca.Item().Text(_consultorio.Subtitulo).FontSize(12).Bold().FontColor(Naranja);

                    if (!string.IsNullOrWhiteSpace(_consultorio.Lema))
                        marca.Item().Text(_consultorio.Lema).FontSize(8).FontColor(TextoSuave);
                });

                fila.RelativeItem().AlignRight().AlignBottom().Column(contacto =>
                {
                    if (!string.IsNullOrWhiteSpace(_consultorio.Ciudad))
                        contacto.Item().AlignRight().Text(_consultorio.Ciudad).FontSize(8).Bold().FontColor(Navy);

                    if (!string.IsNullOrWhiteSpace(_consultorio.Telefono))
                        contacto.Item().AlignRight().Text($"Tel: {_consultorio.Telefono}").FontSize(7.5f).FontColor(TextoSuave);

                    if (!string.IsNullOrWhiteSpace(_consultorio.Correo))
                        contacto.Item().AlignRight().Text(_consultorio.Correo).FontSize(7.5f).FontColor(TextoSuave);

                    if (!string.IsNullOrWhiteSpace(_consultorio.Instagram))
                        contacto.Item().AlignRight().Text($"IG: {_consultorio.Instagram}").FontSize(7.5f).FontColor(TextoSuave);
                });
            });
        }

        private void ComponerSeccion(IContainer container, int numero, string seccion)
        {
            container.PaddingTop(8).Column(columna =>
            {
                columna.Spacing(4);

                columna.Item().BorderBottom(1).BorderColor(Borde).PaddingBottom(2).Text(texto =>
                {
                    texto.Span($"{numero}. ").FontColor(Naranja).Bold();
                    texto.Span(TituloSeccion(seccion).ToUpper(Cultura)).FontColor(Navy).Bold();
                });

                switch (seccion)
                {
                    case "PERSONALES":
                        ComponerPersonales(columna);
                        break;
                    case "CITAS":
                        ComponerCitas(columna);
                        break;
                    case "CONSENTIMIENTOS":
                        ComponerConsentimientos(columna);
                        break;
                    case "COMUNICACIONES":
                        ComponerComunicaciones(columna);
                        break;
                    case "SOLICITUDES":
                        ComponerSolicitudes(columna);
                        break;
                    case "CLINICA":
                        ComponerClinica(columna);
                        break;
                }
            });
        }

        private void ComponerPersonales(ColumnDescriptor columna)
        {
            var paciente = Filas("paciente").FirstOrDefault();
            var contactos = Filas("contactos");

            if (paciente == null)
            {
                SinRegistros(columna);
                return;
            }

            columna.Item().Row(fila =>
            {
                fila.RelativeItem().Column(izquierda =>
                {
                    Par(izquierda, "Paciente", $"{Valor(paciente, "nombres")} {Valor(paciente, "apellidos")}");
                    Par(izquierda, Legible(Valor(paciente, "tipoIdentificacion")), Valor(paciente, "identificacion"));
                    Par(izquierda, "Fecha de nacimiento", FechaConEdad(paciente));
                    Par(izquierda, "Género", Legible(Valor(paciente, "genero")));
                });

                fila.ConstantItem(16);

                fila.RelativeItem().Column(derecha =>
                {
                    foreach (var contacto in contactos)
                    {
                        var detalle = Valor(contacto, "valor");

                        if (Booleano(contacto, "principal"))
                            detalle += " (principal)";

                        if (!Booleano(contacto, "activo"))
                            detalle += " (dado de baja)";

                        Par(derecha, Legible(Valor(contacto, "tipo")), detalle);
                    }

                    Par(derecha, "Dirección", Valor(paciente, "direccion"));
                    Par(derecha, "Paciente desde", Fecha(paciente, "fechaRegistro"));
                });
            });
        }

        private void ComponerCitas(ColumnDescriptor columna)
        {
            var citas = Filas("citas");

            if (citas.Count == 0)
            {
                SinRegistros(columna);
                return;
            }

            Tabla(columna,
                [("Fecha y hora", 2.2f), ("Servicio", 3), ("Estado", 1.8f), ("Canal", 1.6f), ("Observación", 3)],
                citas.Select(c => new[]
                {
                    FechaHora(c, "fechaInicio"),
                    Valor(c, "servicio"),
                    Legible(Valor(c, "estado")),
                    Legible(Valor(c, "canal")),
                    Valor(c, "observacion")
                }));
        }

        private void ComponerConsentimientos(ColumnDescriptor columna)
        {
            var consentimientos = Filas("consentimientos");
            var eventos = Filas("eventosConsentimiento");

            if (consentimientos.Count == 0)
            {
                SinRegistros(columna);
                return;
            }

            Tabla(columna,
                [("Finalidad", 3), ("Estado", 1.5f), ("Otorgado", 1.6f), ("Revocado", 1.6f), ("Medio", 1.5f), ("Aviso", 1)],
                consentimientos.Select(c => new[]
                {
                    Valor(c, "finalidadNombre"),
                    Legible(Valor(c, "estado")),
                    Fecha(c, "fechaObtencion"),
                    Fecha(c, "fechaRevocatoria"),
                    Legible(Valor(c, "medio")),
                    Valor(c, "versionAviso")
                }));

            if (eventos.Count == 0)
                return;

            Subtitulo(columna, "Historial de cambios");

            var finalidades = consentimientos.ToDictionary(c => Valor(c, "id"), c => Valor(c, "finalidadNombre"));

            Tabla(columna,
                [("Fecha", 2), ("Finalidad", 3), ("Estado", 1.5f), ("Medio", 1.5f), ("Observación", 3)],
                eventos.Select(e => new[]
                {
                    FechaHora(e, "fecha"),
                    finalidades.TryGetValue(Valor(e, "consentimientoId"), out var finalidad) ? finalidad : SinDato,
                    Legible(Valor(e, "estado")),
                    Legible(Valor(e, "medio")),
                    Valor(e, "observacion")
                }));
        }

        private void ComponerComunicaciones(ColumnDescriptor columna)
        {
            var opciones = Filas("opcionesComunicacion");
            var notificaciones = Filas("notificaciones");
            var envios = Filas("envios");

            if (opciones.Count == 0 && notificaciones.Count == 0)
            {
                SinRegistros(columna);
                return;
            }

            if (opciones.Count > 0)
            {
                Subtitulo(columna, "Preferencias de contacto");

                Tabla(columna,
                    [("Canal", 2), ("Finalidad", 3), ("Permitido", 1.5f), ("Vigente", 1.5f), ("Actualizado", 2)],
                    opciones.Select(o => new[]
                    {
                        Legible(Valor(o, "canal")),
                        Legible(Valor(o, "finalidad")),
                        SiNo(o, "permitido"),
                        SiNo(o, "activo"),
                        Fecha(o, "fechaActualizacion")
                    }));
            }

            if (notificaciones.Count > 0)
            {
                Subtitulo(columna, "Mensajes programados y enviados");

                var intentos = envios
                    .GroupBy(e => Valor(e, "notificacionId"))
                    .ToDictionary(g => g.Key, g => g.Count());

                Tabla(columna,
                    [("Fecha programada", 2.2f), ("Canal", 1.5f), ("Tipo", 2.5f), ("Estado", 1.5f), ("Intentos", 1.2f)],
                    notificaciones.Select(n => new[]
                    {
                        FechaHora(n, "fechaProgramada"),
                        Legible(Valor(n, "canal")),
                        Legible(Valor(n, "tipo")),
                        Legible(Valor(n, "estado")),
                        intentos.TryGetValue(Valor(n, "id"), out var cantidad) ? cantidad.ToString(Cultura) : "0"
                    }));
            }
        }

        private void ComponerSolicitudes(ColumnDescriptor columna)
        {
            var solicitudes = Filas("solicitudes");

            if (solicitudes.Count == 0)
            {
                SinRegistros(columna);
                return;
            }

            Tabla(columna,
                [("Recibida", 1.6f), ("Derecho", 1.8f), ("Presentada por", 1.8f), ("Estado", 1.6f), ("Respuesta", 4)],
                solicitudes.Select(s => new[]
                {
                    Fecha(s, "fechaRecepcion"),
                    Legible(Valor(s, "derecho")),
                    Legible(Valor(s, "tipoSolicitante")),
                    Legible(Valor(s, "estado")),
                    Valor(s, "respuesta")
                }));
        }

        private void ComponerClinica(ColumnDescriptor columna)
        {
            var historia = Filas("historia").FirstOrDefault();
            var antecedentes = Filas("antecedentes");
            var atenciones = Filas("atenciones");
            var evoluciones = Filas("evoluciones");
            var adjuntos = Filas("adjuntos");

            if (historia == null && atenciones.Count == 0)
            {
                SinRegistros(columna);
                return;
            }

            if (historia != null)
            {
                columna.Item().Row(fila =>
                {
                    fila.RelativeItem().Column(izquierda => Par(izquierda, "Historia abierta", Fecha(historia, "fechaApertura")));
                    fila.ConstantItem(16);
                    fila.RelativeItem().Column(derecha => Par(derecha, "Estado de la historia", Legible(Valor(historia, "estado"))));
                });
            }

            Subtitulo(columna, "Antecedentes");

            if (antecedentes.Count == 0)
            {
                SinRegistros(columna);
            }
            else
            {
                foreach (var antecedente in antecedentes)
                {
                    columna.Item().PaddingLeft(6).Text(texto =>
                    {
                        texto.Span("• ").FontColor(Naranja);
                        texto.Span($"{Legible(Valor(antecedente, "tipo"))}: ").Bold();
                        texto.Span(Valor(antecedente, "descripcion"));

                        if (!Booleano(antecedente, "activo"))
                            texto.Span(" (dado de baja)").FontColor(TextoTenue);
                    });
                }
            }

            Subtitulo(columna, "Atenciones y procedimientos");

            if (atenciones.Count == 0)
            {
                SinRegistros(columna);
            }
            else
            {
                Tabla(columna,
                    [("Fecha", 1.4f), ("Motivo", 2.3f), ("Diagnóstico", 2.3f), ("Tratamiento", 3), ("Observaciones", 2.6f)],
                    atenciones.Select(a => new[]
                    {
                        Fecha(a, "fecha"),
                        Valor(a, "motivoConsulta"),
                        Valor(a, "diagnostico"),
                        Valor(a, "tratamiento"),
                        UnirTextos(Valor(a, "valoracionClinica"), Valor(a, "observacion"))
                    }));
            }

            if (evoluciones.Count > 0)
            {
                Subtitulo(columna, "Evoluciones");

                var fechasAtencion = atenciones.ToDictionary(a => Valor(a, "id"), a => Fecha(a, "fecha"));

                Tabla(columna,
                    [("Fecha", 1.6f), ("Atención del", 1.6f), ("Descripción", 7)],
                    evoluciones.Select(e => new[]
                    {
                        Fecha(e, "fecha"),
                        fechasAtencion.TryGetValue(Valor(e, "atencionId"), out var fecha) ? fecha : SinDato,
                        Valor(e, "descripcion")
                    }));
            }

            if (adjuntos.Count > 0)
            {
                Subtitulo(columna, "Archivos adjuntos a la historia");

                Tabla(columna,
                    [("Archivo", 5), ("Tipo", 2.5f), ("Fecha de carga", 2)],
                    adjuntos.Select(a => new[]
                    {
                        Valor(a, "nombre"),
                        Valor(a, "tipoMime"),
                        Fecha(a, "fechaCarga")
                    }));

                columna.Item().Text("Puede solicitar una copia de estos archivos al consultorio.")
                    .FontSize(7.5f).Italic().FontColor(TextoSuave);
            }
        }

        private void ComponerClausula(IContainer container)
        {
            container.BorderLeft(3).BorderColor(Naranja).Background(FondoNota).Padding(8).Text(texto =>
            {
                texto.DefaultTextStyle(estilo => estilo.FontSize(7.5f).FontColor("#475569").LineHeight(1.35f));
                texto.Span("CONFIDENCIALIDAD Y PROTECCIÓN DE DATOS (LOPDP). ").Bold().FontColor(Navy);
                texto.Span("Este documento contiene datos personales, incluidos datos de salud, protegidos por la Ley Orgánica de Protección de Datos Personales del Ecuador. ");
                texto.Span("Se entrega exclusivamente a su titular en ejercicio del derecho de acceso. ");
                texto.Span("El titular puede solicitar la rectificación, actualización, eliminación, oposición o suspensión del tratamiento de sus datos por los canales del consultorio. ");
                texto.Span("Queda prohibida su alteración o comunicación no autorizada a terceros.");
            });
        }

        private void ComponerCierre(IContainer container)
        {
            container.Row(fila =>
            {
                fila.RelativeItem(6).Column(verificacion =>
                {
                    verificacion.Item().Text(texto =>
                    {
                        texto.DefaultTextStyle(estilo => estilo.FontSize(7.5f).FontColor(TextoSuave));
                        texto.Span("Código de verificación: ");
                        texto.Span(_codigo).Bold().FontColor(Texto);
                    });

                    verificacion.Item().Text("Huella de los datos (SHA-256):").FontSize(7.5f).FontColor(TextoSuave);
                    verificacion.Item().Text(_huellaDatos).FontSize(7).FontColor(Texto);
                    verificacion.Item().Text("Para verificar este documento, comuníquese con el consultorio indicando el código.")
                        .FontSize(7.5f).FontColor(TextoSuave);
                });

                fila.ConstantItem(20);

                fila.RelativeItem(4).AlignBottom().BorderTop(1).BorderColor(Texto).PaddingTop(4).Column(emisor =>
                {
                    emisor.Item().AlignCenter().Text(NombreCompletoConsultorio()).FontSize(8.5f).Bold().FontColor(Navy);
                    emisor.Item().AlignCenter().Text("Responsable del tratamiento de datos").FontSize(8);
                    emisor.Item().AlignCenter().Text($"Emitido el {Fecha(_fechaGeneracion)}").FontSize(8);
                });
            });
        }

        private void ComponerPie(IContainer container)
        {
            container.PaddingTop(6).Row(fila =>
            {
                fila.RelativeItem().Text($"{NombreCompletoConsultorio()} · Documento confidencial")
                    .FontSize(7).FontColor(TextoTenue);

                fila.RelativeItem().AlignRight().Text(texto =>
                {
                    texto.DefaultTextStyle(estilo => estilo.FontSize(7).FontColor(TextoTenue));
                    texto.Span("Página ");
                    texto.CurrentPageNumber();
                    texto.Span(" de ");
                    texto.TotalPages();
                });
            });
        }

        private static void Tabla(
            ColumnDescriptor columna,
            (string Titulo, float Ancho)[] columnas,
            IEnumerable<string[]> filas)
        {
            columna.Item().Table(tabla =>
            {
                tabla.ColumnsDefinition(definicion =>
                {
                    foreach (var columnaTabla in columnas)
                        definicion.RelativeColumn(columnaTabla.Ancho);
                });

                tabla.Header(encabezado =>
                {
                    foreach (var columnaTabla in columnas)
                    {
                        encabezado.Cell().Background(Hielo).Border(0.5f).BorderColor(BordeTabla)
                            .PaddingVertical(4).PaddingHorizontal(5)
                            .Text(columnaTabla.Titulo).FontSize(8).Bold().FontColor(Navy);
                    }
                });

                foreach (var fila in filas)
                {
                    foreach (var celda in fila)
                    {
                        tabla.Cell().Border(0.5f).BorderColor(Borde).PaddingVertical(3.5f).PaddingHorizontal(5)
                            .Text(celda).FontSize(8).FontColor(ColorValor(celda));
                    }
                }
            });
        }

        private static void Par(ColumnDescriptor columna, string etiqueta, string valor)
        {
            columna.Item().BorderBottom(0.5f).BorderColor(Borde).PaddingVertical(2.5f).Row(fila =>
            {
                fila.RelativeItem(4).Text(etiqueta).FontColor(TextoSuave);
                fila.RelativeItem(6).Text(valor).Bold();
            });
        }

        private static void Subtitulo(ColumnDescriptor columna, string texto)
        {
            columna.Item().PaddingTop(4).Text(texto).FontSize(9).Bold().FontColor(Azul);
        }

        private static void SinRegistros(ColumnDescriptor columna)
        {
            columna.Item().Text("Sin registros.").FontSize(8.5f).Italic().FontColor(TextoTenue);
        }

        private List<Dictionary<string, object?>> Filas(string conjunto)
        {
            return _datos.TryGetValue(conjunto, out var filas) ? filas : [];
        }

        private string NombreCompletoConsultorio()
        {
            return string.IsNullOrWhiteSpace(_consultorio.Subtitulo)
                ? _consultorio.Nombre
                : $"{_consultorio.Nombre} {_consultorio.Subtitulo}";
        }

        private static string TituloSeccion(string seccion)
        {
            return TitulosSeccion.TryGetValue(seccion, out var titulo) ? titulo : Legible(seccion);
        }

        private static string TituloCorto(string seccion)
        {
            return seccion switch
            {
                "PERSONALES" => "Datos personales",
                "CLINICA" => "Historia clínica",
                _ => TituloSeccion(seccion)
            };
        }

        private static string Valor(Dictionary<string, object?> fila, string campo)
        {
            if (!fila.TryGetValue(campo, out var valor) || valor == null)
                return SinDato;

            return valor switch
            {
                string texto when string.IsNullOrWhiteSpace(texto) => SinDato,
                string texto => texto.Trim(),
                DateTime fecha => FechaHora(fecha),
                DateOnly dia => dia.ToString("dd/MM/yyyy", Cultura),
                bool logico => logico ? "Sí" : "No",
                IFormattable formateable => formateable.ToString(null, Cultura),
                _ => valor.ToString() ?? SinDato
            };
        }

        private static bool Booleano(Dictionary<string, object?> fila, string campo)
        {
            return fila.TryGetValue(campo, out var valor) && valor is true;
        }

        private static string SiNo(Dictionary<string, object?> fila, string campo)
        {
            return Booleano(fila, campo) ? "Sí" : "No";
        }

        private static string Fecha(Dictionary<string, object?> fila, string campo)
        {
            if (!fila.TryGetValue(campo, out var valor) || valor == null)
                return SinDato;

            return valor switch
            {
                DateTime fecha => Fecha(fecha),
                DateOnly dia => dia.ToString("dd/MM/yyyy", Cultura),
                _ => Valor(fila, campo)
            };
        }

        private static string FechaHora(Dictionary<string, object?> fila, string campo)
        {
            return fila.TryGetValue(campo, out var valor) && valor is DateTime fecha
                ? FechaHora(fecha)
                : Valor(fila, campo);
        }

        private static string Fecha(DateTime fecha)
        {
            return HoraEcuador(fecha).ToString("dd/MM/yyyy", Cultura);
        }

        private static string FechaHora(DateTime fecha)
        {
            return HoraEcuador(fecha).ToString("dd/MM/yyyy HH:mm", Cultura);
        }

        private string FechaConEdad(Dictionary<string, object?> paciente)
        {
            if (!paciente.TryGetValue("fechaNacimiento", out var valor) || valor is not DateOnly nacimiento)
                return SinDato;

            var hoy = DateOnly.FromDateTime(HoraEcuador(_fechaGeneracion));
            var edad = hoy.Year - nacimiento.Year;

            if (nacimiento > hoy.AddYears(-edad))
                edad--;

            return $"{nacimiento.ToString("dd/MM/yyyy", Cultura)} ({edad} años)";
        }

        private static DateTime HoraEcuador(DateTime fecha)
        {
            var utc = fecha.Kind == DateTimeKind.Utc ? fecha : DateTime.SpecifyKind(fecha, DateTimeKind.Utc);
            return TimeZoneInfo.ConvertTimeFromUtc(utc, ZonaEcuador);
        }

        private static string Legible(string codigo)
        {
            if (codigo == SinDato || string.IsNullOrWhiteSpace(codigo))
                return SinDato;

            if (SiglasConocidas.Contains(codigo))
                return codigo;

            var palabras = codigo.Replace('_', ' ').ToLower(Cultura)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => PalabrasConTilde.TryGetValue(p, out var conTilde) ? conTilde : p);

            var texto = string.Join(' ', palabras);
            return char.ToUpper(texto[0], Cultura) + texto[1..];
        }

        private static string UnirTextos(string primero, string segundo)
        {
            var partes = new[] { primero, segundo }.Where(p => p != SinDato).ToList();
            return partes.Count == 0 ? SinDato : string.Join(" · ", partes);
        }

        private static string ColorValor(string valor)
        {
            return valor switch
            {
                "Aceptado" or "Atendida" or "Enviada" or "Sí" or "Cerrada" or "Respondida" => Verde,
                "Revocado" or "Rechazado" or "Rechazada" or "Cancelada" or "Error" or "No asiste" => Rojo,
                _ => Texto
            };
        }

        private static TimeZoneInfo ObtenerZonaEcuador()
        {
            if (TimeZoneInfo.TryFindSystemTimeZoneById("America/Guayaquil", out var zona))
                return zona;

            if (TimeZoneInfo.TryFindSystemTimeZoneById("SA Pacific Standard Time", out zona))
                return zona;

            return TimeZoneInfo.CreateCustomTimeZone("Ecuador", TimeSpan.FromHours(-5), "Ecuador", "Ecuador");
        }
    }
}
