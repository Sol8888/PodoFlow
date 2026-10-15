using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Repositorios.Implementaciones;
using PodoFlow.Api.Servicios.Interfaces;
using PodoFlow.Api.Servicios.Implementaciones;
using Microsoft.AspNetCore.Authentication;
using PodoFlow.Api.Seguridad;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<PodoFlowContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("PodoFlowConnection")
    )
);

// Mujer Aqui van los servicios de repositorio y servicio (Si no pones aqui no funciona)
builder.Services.AddScoped<IServicioRepositorio, ServicioRepositorio>();
builder.Services.AddScoped<IServicioServicio, ServicioServicio>();
builder.Services.AddScoped<IFinalidadRepositorio, FinalidadRepositorio>();
builder.Services.AddScoped<IFinalidadServicio, FinalidadServicio>();
builder.Services.AddScoped<IAvisoRepositorio, AvisoRepositorio>();
builder.Services.AddScoped<IAvisoServicio, AvisoServicio>();
builder.Services.AddScoped<IConsentimientoRepositorio, ConsentimientoRepositorio>();
builder.Services.AddScoped<IConsentimientoServicio, ConsentimientoServicio>();
builder.Services.AddScoped<IExportacionRepositorio, ExportacionRepositorio>();
builder.Services.AddScoped<IExportacionServicio, ExportacionServicio>();
builder.Services.AddScoped<ICategoriaDatoRepositorio, CategoriaDatoRepositorio>();
builder.Services.AddScoped<ICategoriaDatoServicio, CategoriaDatoServicio>();
builder.Services.AddScoped<IEncargadoRepositorio, EncargadoRepositorio>();
builder.Services.AddScoped<IEncargadoServicio, EncargadoServicio>();
builder.Services.AddScoped<ITratamientoDatosRepositorio, TratamientoDatosRepositorio>();
builder.Services.AddScoped<ITratamientoDatosServicio, TratamientoDatosServicio>();
builder.Services.AddScoped<ITratamientoCategoriaRepositorio, TratamientoCategoriaRepositorio>();
builder.Services.AddScoped<ITratamientoCategoriaServicio, TratamientoCategoriaServicio>();
builder.Services.AddScoped<ITratamientoEncargadoRepositorio, TratamientoEncargadoRepositorio>();
builder.Services.AddScoped<ITratamientoEncargadoServicio, TratamientoEncargadoServicio>();
builder.Services.AddScoped<IRiesgoRepositorio, RiesgoRepositorio>();
builder.Services.AddScoped<IRiesgoServicio, RiesgoServicio>();
builder.Services.AddScoped<IEvaluacionImpactoRepositorio, EvaluacionImpactoRepositorio>();
builder.Services.AddScoped<IEvaluacionImpactoServicio, EvaluacionImpactoServicio>();
builder.Services.AddScoped<IEvaluacionRiesgoRepositorio, EvaluacionRiesgoRepositorio>();
builder.Services.AddScoped<IEvaluacionRiesgoServicio, EvaluacionRiesgoServicio>();
builder.Services.AddScoped<IDocumentoRepositorio, DocumentoRepositorio>();
builder.Services.AddScoped<IDocumentoServicio, DocumentoServicio>();
builder.Services.AddScoped<IIncidenteRepositorio, IncidenteRepositorio>();
builder.Services.AddScoped<IIncidenteServicio, IncidenteServicio>();
builder.Services.AddScoped<IPlantillaRepositorio, PlantillaRepositorio>();
builder.Services.AddScoped<IPlantillaServicio, PlantillaServicio>();
builder.Services.AddScoped<IOpcionComunicacionRepositorio, OpcionComunicacionRepositorio>();
builder.Services.AddScoped<IOpcionComunicacionServicio, OpcionComunicacionServicio>();
builder.Services.AddScoped<INotificacionRepositorio, NotificacionRepositorio>();
builder.Services.AddScoped<INotificacionServicio, NotificacionServicio>();


builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
builder.Services.AddScoped<IUsuarioServicio, UsuarioServicio>();
builder.Services.AddScoped<ISolicitudRepositorio, SolicitudRepositorio>();
builder.Services.AddScoped<ISolicitudServicio, SolicitudServicio>();
builder.Services.AddScoped<IEvidenciaSolicitudRepositorio, EvidenciaSolicitudRepositorio>();
builder.Services.AddScoped<IEvidenciaSolicitudServicio, EvidenciaSolicitudServicio>();



builder.Services.AddScoped<IRolRepositorio, RolRepositorio>();
builder.Services.AddScoped<IRolServicio, RolServicio>();


builder.Services.AddScoped<IPermisoRepositorio, PermisoRepositorio>();
builder.Services.AddScoped<IPermisoServicio, PermisoServicio>();


builder.Services.AddScoped<IUsuarioRolRepositorio, UsuarioRolRepositorio>();
builder.Services.AddScoped<IUsuarioRolServicio, UsuarioRolServicio>();


builder.Services.AddScoped<IRolPermisoRepositorio, RolPermisoRepositorio>();
builder.Services.AddScoped<IRolPermisoServicio, RolPermisoServicio>();


builder.Services.AddScoped<IIntentoAccesoRepositorio, IntentoAccesoRepositorio>();
builder.Services.AddScoped<IIntentoAccesoServicio, IntentoAccesoServicio>();


builder.Services.AddScoped<IPacienteRepositorio, PacienteRepositorio>();
builder.Services.AddScoped<IPacienteServicio, PacienteServicio>();


builder.Services.AddScoped<IPacienteUsuarioRepositorio, PacienteUsuarioRepositorio>();
builder.Services.AddScoped<IPacienteUsuarioServicio, PacienteUsuarioServicio>();


builder.Services.AddScoped<IDatoContactoRepositorio, DatoContactoRepositorio>();
builder.Services.AddScoped<IDatoContactoServicio, DatoContactoServicio>();


builder.Services.AddScoped<IEventoAuditoriaRepositorio, EventoAuditoriaRepositorio>();
builder.Services.AddScoped<IEventoAuditoriaServicio, EventoAuditoriaServicio>();


builder.Services.AddScoped<IHorarioAtencionRepositorio, HorarioAtencionRepositorio>();
builder.Services.AddScoped<IHorarioAtencionServicio, HorarioAtencionServicio>();


builder.Services.AddScoped<IBloqueoAgendaRepositorio, BloqueoAgendaRepositorio>();
builder.Services.AddScoped<IBloqueoAgendaServicio, BloqueoAgendaServicio>();


builder.Services.AddScoped<ICambioCitaRepositorio, CambioCitaRepositorio>();
builder.Services.AddScoped<ICambioCitaServicio, CambioCitaServicio>();


builder.Services.AddScoped<ICitaRepositorio, CitaRepositorio>();
builder.Services.AddScoped<ICitaServicio, CitaServicio>();


builder.Services.AddScoped<IHistoriaRepositorio, HistoriaRepositorio>();
builder.Services.AddScoped<IHistoriaServicio, HistoriaServicio>();

// Esto de aqui esta porque necesito hacer mock de la seguridad para probar algunas cosas :) la carpeta de seguridad esta conectada a esto y es por hacer mock a la Autenticacion
if (builder.Environment.IsDevelopment())
{
    builder.Services
        .AddAuthentication("Desarrollo")
        .AddScheme<AuthenticationSchemeOptions,
            AutenticacionDesarrolloHandler>(
            "Desarrollo", options => { });
}

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("HistoriaClinicaGestionar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim(
            "permiso", "HISTORIA_GESTIONAR");
    });

    options.AddPolicy("AuditoriaLeer", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim(
            "permiso", "AUDITORIA_VER");
    });
});



// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSwaggerGen(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.AddSecurityDefinition(
            "DevToken",
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Description = "Token para pruebas locales",
                Name = "X-Dev-Token",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
            });

        options.AddSecurityRequirement(
            new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "DevToken"
                        }
                    },
                    Array.Empty<string>()
                }
            });
    }
});



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseAuthentication();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
