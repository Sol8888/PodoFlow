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

builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
builder.Services.AddScoped<IUsuarioServicio, UsuarioServicio>();

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

builder.Services.AddScoped<IAntecedenteRepositorio, AntecedenteRepositorio>();
builder.Services.AddScoped<IAntecedenteServicio, AntecedenteServicio>();

builder.Services.AddScoped<IAtencionRepositorio, AtencionRepositorio>();
builder.Services.AddScoped<IAtencionServicio, AtencionServicio>();

builder.Services.AddScoped<IEvolucionRepositorio, EvolucionRepositorio>();
builder.Services.AddScoped<IEvolucionServicio, EvolucionServicio>();

builder.Services.AddScoped<IAdjuntoRepositorio, AdjuntoRepositorio>();
builder.Services.AddScoped<IAdjuntoServicio, AdjuntoServicio>();

builder.Services.AddScoped<IMovimientoRepositorio, MovimientoRepositorio>();
builder.Services.AddScoped<IMovimientoServicio, MovimientoServicio>();

builder.Services.AddScoped<IFormaPagoRepositorio, FormaPagoRepositorio>();
builder.Services.AddScoped<IFormaPagoServicio, FormaPagoServicio>();

builder.Services.AddScoped<IPrestacionRepositorio, PrestacionRepositorio>();
builder.Services.AddScoped<IPrestacionServicio, PrestacionServicio>();


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

    options.AddPolicy("InventarioGestionar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim(
            "permiso", "INVENTARIO_GESTIONAR");
    });

    options.AddPolicy("FinanzasGestionar", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("permiso", "FINANZAS_GESTIONAR");
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
