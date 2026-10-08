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

    public DbSet<Usuario> Usuarios { get; set; }


    public DbSet<Rol> Roles { get; set; }


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


        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("USUARIO", "seg");

            entity.HasKey(e => e.UsId);

            entity.Property(e => e.UsId)
                .HasColumnName("us_id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.UsNombre)
                .HasColumnName("us_nombre")
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(e => e.UsCorreo)
                .HasColumnName("us_correo")
                .HasMaxLength(320)
                .IsRequired();

            entity.HasIndex(e => e.UsCorreo).IsUnique();

            entity.Property(e => e.UsEntraOid)
                .HasColumnName("us_entra_oid");

            entity.Property(e => e.UsEntraTid)
                .HasColumnName("us_entra_tid");

            entity.Property(e => e.UsFechaCrea)
                .HasColumnName("us_fecha_crea")
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.Property(e => e.UsFechaModi)
                .HasColumnName("us_fecha_modi");

            entity.Property(e => e.UsUltimAcces)
                .HasColumnName("us_ultim_acces");

            entity.Property(e => e.UsEstatus)
                .HasColumnName("us_estatus")
                .HasDefaultValue(true);

            entity.Property(e => e.UsVersion)
                .HasColumnName("us_version")
                .IsRowVersion();
        });


        modelBuilder.Entity<Rol>(entity =>
        {
            entity.ToTable("ROL", "seg");

            entity.HasKey(e => e.RoId);

            entity.Property(e => e.RoId)
                .HasColumnName("ro_id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.RoCodigo)
                .HasColumnName("ro_codigo")
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(e => e.RoCodigo)
                .IsUnique();

            entity.Property(e => e.RoNombre)
                .HasColumnName("ro_nombre")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.RoDescri)
                .HasColumnName("ro_descri")
                .HasMaxLength(500);

            entity.Property(e => e.RoFechaCrea)
                .HasColumnName("ro_fecha_crea")
                .HasPrecision(0)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.Property(e => e.RoEstatus)
                .HasColumnName("ro_estatus")
                .HasDefaultValue(true);
        });

    }
}
