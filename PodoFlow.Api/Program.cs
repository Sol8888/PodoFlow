using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Datos;
using PodoFlow.Api.Repositorios.Interfaces;
using PodoFlow.Api.Repositorios.Implementaciones;
using PodoFlow.Api.Servicios.Interfaces;
using PodoFlow.Api.Servicios.Implementaciones;

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



// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
