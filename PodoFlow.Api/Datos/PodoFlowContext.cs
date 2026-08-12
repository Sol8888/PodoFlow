using Microsoft.EntityFrameworkCore;
using PodoFlow.Api.Modelos;
using System.Reflection.Emit;

namespace PodoFlow.Api.Datos;

public class PodoFlowContext : DbContext
{
    public PodoFlowContext(DbContextOptions<PodoFlowContext> options)
       : base(options)
    {
    }

    public DbSet<Servicio> Servicios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Servicio>(entity =>
        {
            entity.ToTable("SERVICIO", "cit");

            entity.HasKey(e => e.SeId);

            entity.Property(e => e.SeId)
                .HasColumnName("se_id");

            entity.Property(e => e.SeNombre)
                .HasColumnName("se_nombre")
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(e => e.SeDescri)
                .HasColumnName("se_descri")
                .HasMaxLength(1000);

            entity.Property(e => e.SePrecio)
                .HasColumnName("se_precio")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            entity.Property(e => e.SeDuracMin)
                .HasColumnName("se_durac_min")
                .IsRequired();

            entity.Property(e => e.SeFechaCrea)
                .HasColumnName("se_fecha_crea")
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.Property(e => e.SeFechaModi)
                .HasColumnName("se_fecha_modi");

            entity.Property(e => e.SeEstatus)
                .HasColumnName("se_estatus")
                .HasDefaultValue(true);

            entity.Property(e => e.SeVersion)
                .HasColumnName("se_version")
                .IsRowVersion();
        });
    }
}
