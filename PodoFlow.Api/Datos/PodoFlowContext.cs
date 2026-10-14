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
    public DbSet<Finalidad> Finalidades { get; set; }
    public DbSet<Aviso> Avisos { get; set; }
    public DbSet<Consentimiento> Consentimientos { get; set; }
    public DbSet<EventoConsentimiento> EventosConsentimiento { get; set; } 
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Solicitud> Solicitudes { get; set; }
    public DbSet<AccionSolicitud> AccionesSolicitud { get; set; }
    public DbSet<EvidenciaSolicitud> EvidenciasSolicitud { get; set; }

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

        modelBuilder.Entity<Finalidad>(entity =>
        {
            entity.ToTable("FINALIDAD", "pri");

            entity.HasKey(e => e.FiId);

            entity.HasIndex(e => e.FiCodigo)
                .IsUnique();

            entity.Property(e => e.FiId)
                .HasColumnName("fi_id");

            entity.Property(e => e.FiCodigo)
                .HasColumnName("fi_codigo")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.FiNombre)
                .HasColumnName("fi_nombre")
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(e => e.FiDescri)
                .HasColumnName("fi_descri")
                .HasMaxLength(1000);

            entity.Property(e => e.FiRequiCons)
                .HasColumnName("fi_requi_cons")
                .IsRequired();

            entity.Property(e => e.FiEstatus)
                .HasColumnName("fi_estatus")
                .HasDefaultValue(true); 
        });

        modelBuilder.Entity<Aviso>(entity =>
        {
            entity.ToTable("AVISO", "pri", t =>
                t.HasCheckConstraint("CK_AVISO_fechas",
                    "av_fecha_hast IS NULL OR av_fecha_hast >= av_fecha_desd"));

            entity.HasKey(e => e.AvId);

            entity.HasIndex(e => e.AvVersi)
                .IsUnique();

            entity.Property(e => e.AvId)
                .HasColumnName("av_id");

            entity.Property(e => e.AvVersi)
                .HasColumnName("av_versi")
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(e => e.AvFechaDesd)
                .HasColumnName("av_fecha_desd")
                .HasColumnType("date")
                .IsRequired();

            entity.Property(e => e.AvFechaHast)
                .HasColumnName("av_fecha_hast")
                .HasColumnType("date");

            entity.Property(e => e.AvRutaArch)
                .HasColumnName("av_ruta_arch")
                .HasMaxLength(1000);

            entity.Property(e => e.AvHuella)
                .HasColumnName("av_huella")
                .HasMaxLength(128);

            entity.Property(e => e.AvEstatus)
                .HasColumnName("av_estatus")
                .IsRequired();
        });

        modelBuilder.Entity<Consentimiento>(entity =>
        {
            entity.ToTable("CONSENTIMIENTO", "pri", t =>
         t.HasCheckConstraint("CK_CONSENTIMIENTO_estad",
             "co_estad IN ('ACEPTADO','RECHAZADO','REVOCADO')"));

            entity.HasKey(e => e.CoId);

            entity.HasIndex(e => new { e.PaId, e.FiId })
                .IsUnique();

            entity.Property(e => e.CoId)
                .HasColumnName("co_id");

            entity.Property(e => e.PaId)
                .HasColumnName("pa_id")
                .IsRequired();

            entity.Property(e => e.FiId)
                .HasColumnName("fi_id")
                .IsRequired();

            entity.Property(e => e.AvId)
                .HasColumnName("av_id")
                .IsRequired();

            entity.Property(e => e.CoEstad)
                .HasColumnName("co_estad")
                .HasMaxLength(15)
                .IsRequired();

            entity.Property(e => e.CoMedio)
                .HasColumnName("co_medio")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.CoFechaObte)
                .HasColumnName("co_fecha_obte")
                .HasColumnType("datetime2(0)");

            entity.Property(e => e.CoFechaRevo)
                .HasColumnName("co_fecha_revo")
                .HasColumnType("datetime2(0)");

            entity.Property(e => e.CoObser)
                .HasColumnName("co_obser")
                .HasMaxLength(1000);

            entity.Property(e => e.CoVersion)
                .HasColumnName("co_version")
                .IsRowVersion();

            entity.HasOne<Finalidad>()
                .WithMany()
                .HasForeignKey(e => e.FiId);

            entity.HasOne<Aviso>()
                .WithMany()
                .HasForeignKey(e => e.AvId);
        });
        modelBuilder.Entity<EventoConsentimiento>(entity =>
        {
            entity.ToTable("EVENTO_CONSENTIMIENTO", "pri");

            entity.HasKey(e => e.EvId);

            entity.Property(e => e.EvId)
                .HasColumnName("ev_id");

            entity.Property(e => e.CoId)
                .HasColumnName("co_id");

            entity.Property(e => e.UsId)
                .HasColumnName("us_id");

            entity.Property(e => e.EvEstad)
                .HasColumnName("ev_estad")
                .HasMaxLength(15)
                .IsRequired();

            entity.Property(e => e.EvFechaHora)
                .HasColumnName("ev_fecha_hora")
                .HasColumnType("datetime2(0)");

            entity.Property(e => e.EvMedio)
                .HasColumnName("ev_medio")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.EvRutaEvid)
                .HasColumnName("ev_ruta_evid")
                .HasMaxLength(1000);

            entity.Property(e => e.EvObser)
                .HasColumnName("ev_obser")
                .HasMaxLength(1000);

            entity.HasOne(e => e.Consentimiento)
                .WithMany()
                .HasForeignKey(e => e.CoId);
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

        modelBuilder.Entity<Solicitud>(entity =>
        {
            entity.ToTable("SOLICITUD", "pri");

            entity.HasKey(e => e.SoId);

            entity.HasIndex(e => new { e.PaId, e.SoFechaRece });

            entity.Property(e => e.SoId)
                .HasColumnName("so_id");

            entity.Property(e => e.PaId)
                .HasColumnName("pa_id")
                .IsRequired();

            entity.Property(e => e.UsId)
                .HasColumnName("us_id");

            entity.Property(e => e.SoTipoDere)
                .HasColumnName("so_tipo_dere")
                .HasMaxLength(25)
                .IsRequired();

            entity.Property(e => e.SoTipoSoli)
                .HasColumnName("so_tipo_soli")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.SoNombrSoli)
                .HasColumnName("so_nombr_soli")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.SoIdentSoli)
                .HasColumnName("so_ident_soli")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.SoFechaRece)
                .HasColumnName("so_fecha_rece")
                .HasColumnType("datetime2(0)");

            entity.Property(e => e.SoEstad)
                .HasColumnName("so_estad")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.SoMedioResp)
                .HasColumnName("so_medio_resp")
                .HasMaxLength(50);

            entity.Property(e => e.SoDetal)
                .HasColumnName("so_detal")
                .HasMaxLength(3000);

            entity.Property(e => e.SoRespue)
                .HasColumnName("so_respue")
                .HasMaxLength(3000);

            entity.Property(e => e.SoFechaCier)
                .HasColumnName("so_fecha_cier")
                .HasColumnType("datetime2(0)");

            entity.Property(e => e.SoVersion)
                .HasColumnName("so_version")
                .IsRowVersion();
        });

        modelBuilder.Entity<AccionSolicitud>(entity =>
        {
            entity.ToTable("ACCION_SOLICITUD", "pri");

            entity.HasKey(e => e.AcId);

            entity.Property(e => e.AcId)
                .HasColumnName("ac_id");

            entity.Property(e => e.SoId)
                .HasColumnName("so_id");

            entity.Property(e => e.UsId)
                .HasColumnName("us_id")
                .IsRequired();

            entity.Property(e => e.AcFechaHora)
                .HasColumnName("ac_fecha_hora")
                .HasColumnType("datetime2(0)");

            entity.Property(e => e.AcEstadAnte)
                .HasColumnName("ac_estad_ante")
                .HasMaxLength(20);

            entity.Property(e => e.AcEstadNuev)
                .HasColumnName("ac_estad_nuev")
                .HasMaxLength(20);

            entity.Property(e => e.AcAccion)
                .HasColumnName("ac_accion")
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(e => e.AcObser)
                .HasColumnName("ac_obser")
                .HasMaxLength(2000);

            entity.HasOne(e => e.Solicitud)
                .WithMany()
                .HasForeignKey(e => e.SoId);
        });
        modelBuilder.Entity<EvidenciaSolicitud>(entity =>
        {
            entity.ToTable("EVIDENCIA_SOLICITUD", "pri");

            entity.HasKey(e => e.EvId);

            entity.Property(e => e.EvId)
                .HasColumnName("ev_id");

            entity.Property(e => e.SoId)
                .HasColumnName("so_id")
                .IsRequired();

            entity.Property(e => e.UsId)
                .HasColumnName("us_id")
                .IsRequired();

            entity.Property(e => e.EvTipo)
                .HasColumnName("ev_tipo")
                .HasMaxLength(80)
                .IsRequired();

            entity.Property(e => e.EvRutaArch)
                .HasColumnName("ev_ruta_arch")
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(e => e.EvHuella)
                .HasColumnName("ev_huella")
                .HasMaxLength(128);

            entity.Property(e => e.EvFechaCarg)
                .HasColumnName("ev_fecha_carg")
                .HasColumnType("datetime2(0)");

            entity.HasOne<Solicitud>()
                .WithMany()
                .HasForeignKey(e => e.SoId);
        });

    }


}
