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

    public DbSet<Permiso> Permisos { get; set; }

    public DbSet<UsuarioRol> UsuariosRoles { get; set; }

    public DbSet<RolPermiso> RolesPermisos { get; set; }

    public DbSet<IntentoAcceso> IntentosAcceso { get; set; }

    public DbSet<Paciente> Pacientes { get; set; }

    public DbSet<PacienteUsuario> PacientesUsuarios { get; set; }

    public DbSet<DatoContacto> DatosContacto { get; set; }

    public DbSet<EventoAuditoria> EventosAuditoria { get; set; }

    public DbSet<HorarioAtencion> HorariosAtencion { get; set; }

    public DbSet<BloqueoAgenda> BloqueosAgenda { get; set; }

    public DbSet<CambioCita> CambiosCita { get; set; }

    public DbSet<Cita> Citas { get; set; }

    public DbSet<Historia> Historias { get; set; }

    public DbSet<Antecedente> Antecedentes { get; set; }

    public DbSet<Atencion> Atenciones { get; set; }

    public DbSet<Evolucion> Evoluciones { get; set; }

    public DbSet<Adjunto> Adjuntos { get; set; }

    public DbSet<Insumo> Insumos { get; set; }

    public DbSet<Movimiento> Movimientos { get; set; }


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

        modelBuilder.Entity<Permiso>(entity =>
        {
            entity.ToTable("PERMISO", "seg");

            entity.HasKey(e => e.PeId);

            entity.Property(e => e.PeId)
                .HasColumnName("pe_id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.PeCodigo)
                .HasColumnName("pe_codigo")
                .HasMaxLength(80)
                .IsRequired();

            entity.HasIndex(e => e.PeCodigo)
                .IsUnique();

            entity.Property(e => e.PeNombre)
                .HasColumnName("pe_nombre")
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(e => e.PeModulo)
                .HasColumnName("pe_modulo")
                .HasMaxLength(80)
                .IsRequired();

            entity.Property(e => e.PeDescri)
                .HasColumnName("pe_descri")
                .HasMaxLength(500);

            entity.Property(e => e.PeEstatus)
                .HasColumnName("pe_estatus")
                .HasDefaultValue(true);
        });

        modelBuilder.Entity<UsuarioRol>(entity =>
        {
            entity.ToTable("US_RO", "seg");

            // Clave primaria compuesta.
            entity.HasKey(e => new
            {
                e.UsId,
                e.RoId
            });

            entity.Property(e => e.UsId)
                .HasColumnName("us_id")
                .ValueGeneratedNever();

            entity.Property(e => e.RoId)
                .HasColumnName("ro_id")
                .ValueGeneratedNever();

            entity.Property(e => e.UrFechaAsig)
                .HasColumnName("ur_fecha_asig")
                .HasPrecision(0)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.Property(e => e.UrEstatus)
                .HasColumnName("ur_estatus")
                .HasDefaultValue(true);

            // Relación con seg.USUARIO.
            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(e => e.UsId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación con seg.ROL.
            entity.HasOne<Rol>()
                .WithMany()
                .HasForeignKey(e => e.RoId)
                .OnDelete(DeleteBehavior.Restrict);
        });


        modelBuilder.Entity<RolPermiso>(entity =>
        {
            entity.ToTable("RO_PE", "seg");

            // Clave primaria compuesta
            entity.HasKey(e => new
            {
                e.RoId,
                e.PeId
            });

            entity.Property(e => e.RoId)
                .HasColumnName("ro_id")
                .ValueGeneratedNever();

            entity.Property(e => e.PeId)
                .HasColumnName("pe_id")
                .ValueGeneratedNever();

            entity.Property(e => e.RpFechaAsig)
                .HasColumnName("rp_fecha_asig")
                .HasPrecision(0)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.Property(e => e.RpEstatus)
                .HasColumnName("rp_estatus")
                .HasDefaultValue(true);

            entity.HasOne<Rol>()
                .WithMany()
                .HasForeignKey(e => e.RoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Permiso>()
                .WithMany()
                .HasForeignKey(e => e.PeId)
                .OnDelete(DeleteBehavior.Restrict);
        });


        modelBuilder.Entity<IntentoAcceso>(entity =>
        {
            entity.ToTable("INTENTO_ACCESO", "seg");

            entity.HasKey(e => e.InId);

            entity.Property(e => e.InId)
                .HasColumnName("in_id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.UsId)
                .HasColumnName("us_id");

            entity.Property(e => e.InIdentAcce)
                .HasColumnName("in_ident_acce")
                .HasMaxLength(320);

            entity.Property(e => e.InFechaHora)
                .HasColumnName("in_fecha_hora")
                .HasPrecision(0)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.Property(e => e.InExitoso)
                .HasColumnName("in_exitoso")
                .IsRequired();

            entity.Property(e => e.InMotivo)
                .HasColumnName("in_motivo")
                .HasMaxLength(300);

            entity.Property(e => e.InDirecIp)
                .HasColumnName("in_direc_ip")
                .HasMaxLength(45);

            entity.Property(e => e.InAgentUsua)
                .HasColumnName("in_agent_usua")
                .HasMaxLength(500);

            // Clave foránea opcional hacia seg.USUARIO.
            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(e => e.UsId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índice existente en SQL Server.
            entity.HasIndex(e => new
            {
                e.UsId,
                e.InFechaHora
            })
            .HasDatabaseName("IX_INTENTO_USUARIO_fecha")
            .IsDescending(false, true);
        });


        modelBuilder.Entity<Paciente>(entity =>
        {
            entity.ToTable("PACIENTE", "pac");
            entity.HasKey(x => x.PaId);

            entity.Property(x => x.PaId)
                .HasColumnName("pa_id").ValueGeneratedOnAdd();

            entity.Property(x => x.PaTipoIdent)
                .HasColumnName("pa_tipo_ident")
                .HasMaxLength(20).IsRequired();

            entity.Property(x => x.PaIdent)
                .HasColumnName("pa_ident")
                .HasMaxLength(30).IsRequired();

            entity.HasIndex(x => new { x.PaTipoIdent, x.PaIdent })
                .IsUnique();

            entity.Property(x => x.PaNombres)
                .HasColumnName("pa_nombres")
                .HasMaxLength(100).IsRequired();

            entity.Property(x => x.PaApelli)
                .HasColumnName("pa_apelli")
                .HasMaxLength(100).IsRequired();

            entity.Property(x => x.PaFechaNaci)
                .HasColumnName("pa_fecha_naci")
                .HasColumnType("date");

            entity.Property(x => x.PaGenero)
                .HasColumnName("pa_genero").HasMaxLength(30);

            entity.Property(x => x.PaDirec)
                .HasColumnName("pa_direc").HasMaxLength(300);

            entity.Property(x => x.PaFechaCrea)
                .HasColumnName("pa_fecha_crea")
                .HasPrecision(0)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.Property(x => x.PaFechaModi)
                .HasColumnName("pa_fecha_modi").HasPrecision(0);

            entity.Property(x => x.PaFechaBaja)
                .HasColumnName("pa_fecha_baja").HasPrecision(0);

            entity.Property(x => x.PaMotivBaja)
                .HasColumnName("pa_motiv_baja").HasMaxLength(500);

            entity.Property(x => x.PaEstatus)
                .HasColumnName("pa_estatus").HasDefaultValue(true);

            entity.Property(x => x.PaVersion)
                .HasColumnName("pa_version").IsRowVersion();
        });


        modelBuilder.Entity<PacienteUsuario>(entity =>
        {
            entity.ToTable("PA_US", "pac");

            entity.HasKey(x => new { x.PaId, x.UsId });

            entity.Property(x => x.PaId)
                .HasColumnName("pa_id").ValueGeneratedNever();

            entity.Property(x => x.UsId)
                .HasColumnName("us_id").ValueGeneratedNever();

            entity.Property(x => x.PuFechaAsig)
                .HasColumnName("pu_fecha_asig")
                .HasPrecision(0)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.Property(x => x.PuEstatus)
                .HasColumnName("pu_estatus")
                .HasDefaultValue(true);

            entity.HasIndex(x => x.PaId)
                .IsUnique().HasDatabaseName("UQ_PA_US_pa");

            entity.HasIndex(x => x.UsId)
                .IsUnique().HasDatabaseName("UQ_PA_US_us");

            entity.HasOne<Paciente>()
                .WithMany()
                .HasForeignKey(x => x.PaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(x => x.UsId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DatoContacto>(entity =>
        {
            entity.ToTable("DATO_CONTACTO", "pac");
            entity.HasKey(x => x.DaId);

            entity.Property(x => x.DaId)
                .HasColumnName("da_id").ValueGeneratedOnAdd();

            entity.Property(x => x.PaId)
                .HasColumnName("pa_id");

            entity.Property(x => x.DaTipo)
                .HasColumnName("da_tipo")
                .HasMaxLength(20).IsRequired();

            entity.Property(x => x.DaValor)
                .HasColumnName("da_valor")
                .HasMaxLength(320).IsRequired();

            entity.Property(x => x.DaPrinc)
                .HasColumnName("da_princ").HasDefaultValue(false);

            entity.Property(x => x.DaVerif)
                .HasColumnName("da_verif").HasDefaultValue(false);

            entity.Property(x => x.DaFechaCrea)
                .HasColumnName("da_fecha_crea")
                .HasPrecision(0)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.Property(x => x.DaEstatus)
                .HasColumnName("da_estatus").HasDefaultValue(true);

            entity.HasOne<Paciente>()
                .WithMany()
                .HasForeignKey(x => x.PaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.PaId, x.DaEstatus })
                .HasDatabaseName("IX_DATO_CONTACTO_paciente");
        });

        modelBuilder.Entity<EventoAuditoria>(entity =>
        {
            entity.ToTable("EVENTO_AUDITORIA", "aud");

            entity.HasKey(x => x.EvId);

            entity.Property(x => x.EvId)
                .HasColumnName("ev_id")
                .ValueGeneratedOnAdd();

            entity.Property(x => x.UsId)
                .HasColumnName("us_id");

            entity.Property(x => x.PaId)
                .HasColumnName("pa_id");

            entity.Property(x => x.EvFechaHora)
                .HasColumnName("ev_fecha_hora")
                .HasPrecision(0)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.Property(x => x.EvModulo)
                .HasColumnName("ev_modulo")
                .HasMaxLength(80)
                .IsRequired();

            entity.Property(x => x.EvAccion)
                .HasColumnName("ev_accion")
                .HasMaxLength(80)
                .IsRequired();

            entity.Property(x => x.EvEntid)
                .HasColumnName("ev_entid")
                .HasMaxLength(128);

            entity.Property(x => x.EvEntidId)
                .HasColumnName("ev_entid_id")
                .HasMaxLength(100);

            entity.Property(x => x.EvResul)
                .HasColumnName("ev_resul")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.EvDirecIp)
                .HasColumnName("ev_direc_ip")
                .HasMaxLength(45);

            entity.Property(x => x.EvAgentUsua)
                .HasColumnName("ev_agent_usua")
                .HasMaxLength(500);

            entity.Property(x => x.EvIdCorre)
                .HasColumnName("ev_id_corre");

            entity.Property(x => x.EvMetadJson)
                .HasColumnName("ev_metad_json")
                .HasColumnType("nvarchar(max)");

            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(x => x.UsId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Paciente>()
                .WithMany()
                .HasForeignKey(x => x.PaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new
            {
                x.UsId,
                x.EvFechaHora
            })
            .HasDatabaseName("IX_EVENTO_AUD_usuario_fecha")
            .IsDescending(false, true);
        });


        modelBuilder.Entity<HorarioAtencion>(entity =>
        {
            entity.ToTable("HORARIO_ATENCION", "cit");

            entity.HasKey(x => x.HoId);

            entity.Property(x => x.HoId)
                .HasColumnName("ho_id")
                .ValueGeneratedOnAdd();

            entity.Property(x => x.UsId)
                .HasColumnName("us_id");

            entity.Property(x => x.HoDiaSem)
                .HasColumnName("ho_dia_sem");

            entity.Property(x => x.HoHoraIni)
                .HasColumnName("ho_hora_ini")
                .HasColumnType("time(0)");

            entity.Property(x => x.HoHoraFin)
                .HasColumnName("ho_hora_fin")
                .HasColumnType("time(0)");

            entity.Property(x => x.HoFechaDesd)
                .HasColumnName("ho_fecha_desd")
                .HasColumnType("date");

            entity.Property(x => x.HoFechaHast)
                .HasColumnName("ho_fecha_hast")
                .HasColumnType("date");

            entity.Property(x => x.HoEstatus)
                .HasColumnName("ho_estatus")
                .HasDefaultValue(true);

            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(x => x.UsId)
                .OnDelete(DeleteBehavior.Restrict);
        });


        modelBuilder.Entity<BloqueoAgenda>(entity =>
        {
            entity.ToTable("BLOQUEO_AGENDA", "cit");

            entity.HasKey(x => x.BlId);

            entity.Property(x => x.BlId)
                .HasColumnName("bl_id")
                .ValueGeneratedOnAdd();

            entity.Property(x => x.UsId)
                .HasColumnName("us_id");

            entity.Property(x => x.BlFechaIni)
                .HasColumnName("bl_fecha_ini")
                .HasPrecision(0)
                .IsRequired();

            entity.Property(x => x.BlFechaFin)
                .HasColumnName("bl_fecha_fin")
                .HasPrecision(0)
                .IsRequired();

            entity.Property(x => x.BlMotivo)
                .HasColumnName("bl_motivo")
                .HasMaxLength(300)
                .IsRequired();

            entity.Property(x => x.BlFechaCrea)
                .HasColumnName("bl_fecha_crea")
                .HasPrecision(0)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.Property(x => x.BlEstatus)
                .HasColumnName("bl_estatus")
                .HasDefaultValue(true);

            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(x => x.UsId)
                .OnDelete(DeleteBehavior.Restrict);
        });


        modelBuilder.Entity<CambioCita>(entity =>
        {
            entity.ToTable("CAMBIO_CITA", "cit");

            entity.HasKey(x => x.CaId);

            entity.Property(x => x.CaId)
                .HasColumnName("ca_id")
                .ValueGeneratedOnAdd();

            entity.Property(x => x.CiId)
                .HasColumnName("ci_id")
                .IsRequired();

            entity.Property(x => x.UsId)
                .HasColumnName("us_id");

            entity.Property(x => x.CaEstadAnte)
                .HasColumnName("ca_estad_ante")
                .HasMaxLength(20);

            entity.Property(x => x.CaEstadNuev)
                .HasColumnName("ca_estad_nuev")
                .HasMaxLength(20);

            entity.Property(x => x.CaFechaAnte)
                .HasColumnName("ca_fecha_ante")
                .HasPrecision(0);

            entity.Property(x => x.CaFechaNuev)
                .HasColumnName("ca_fecha_nuev")
                .HasPrecision(0);

            entity.Property(x => x.CaMotivo)
                .HasColumnName("ca_motivo")
                .HasMaxLength(500);

            entity.Property(x => x.CaFechaCamb)
                .HasColumnName("ca_fecha_camb")
                .HasPrecision(0)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(x => x.UsId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new
            {
                x.CiId,
                x.CaFechaCamb
            })
            .HasDatabaseName("IX_CAMBIO_CITA_cita")
            .IsDescending(false, true);
        });


        modelBuilder.Entity<Cita>(entity =>
        {
            entity.ToTable("CITA", "cit");

            entity.HasKey(x => x.CiId);

            entity.Property(x => x.CiId)
                .HasColumnName("ci_id")
                .ValueGeneratedOnAdd();

            entity.Property(x => x.PaId)
                .HasColumnName("pa_id")
                .IsRequired();

            entity.Property(x => x.SeId)
                .HasColumnName("se_id")
                .IsRequired();

            entity.Property(x => x.UsId)
                .HasColumnName("us_id");

            entity.Property(x => x.CiFechaIni)
                .HasColumnName("ci_fecha_ini")
                .HasPrecision(0)
                .IsRequired();

            entity.Property(x => x.CiFechaFin)
                .HasColumnName("ci_fecha_fin")
                .HasPrecision(0)
                .IsRequired();

            entity.Property(x => x.CiEstad)
                .HasColumnName("ci_estad")
                .HasMaxLength(20)
                .HasDefaultValue("SOLICITADA")
                .IsRequired();

            entity.Property(x => x.CiCanal)
                .HasColumnName("ci_canal")
                .HasMaxLength(30)
                .HasDefaultValue("SISTEMA")
                .IsRequired();

            entity.Property(x => x.CiObser)
                .HasColumnName("ci_obser")
                .HasMaxLength(1000);

            entity.Property(x => x.CiFechaCrea)
                .HasColumnName("ci_fecha_crea")
                .HasPrecision(0)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.Property(x => x.CiFechaModi)
                .HasColumnName("ci_fecha_modi")
                .HasPrecision(0);

            entity.Property(x => x.CiVersion)
                .HasColumnName("ci_version")
                .IsRowVersion();

            entity.HasOne<Paciente>()
                .WithMany()
                .HasForeignKey(x => x.PaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Servicio>()
                .WithMany()
                .HasForeignKey(x => x.SeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(x => x.UsId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new
            {
                x.CiFechaIni,
                x.CiFechaFin,
                x.CiEstad
            })
            .HasDatabaseName("IX_CITA_agenda");

            entity.HasIndex(x => new
            {
                x.PaId,
                x.CiFechaIni
            })
            .HasDatabaseName("IX_CITA_paciente")
            .IsDescending(false, true);
        });

        modelBuilder.Entity<Historia>(entity =>
        {
            entity.ToTable("HISTORIA", "cli");

            entity.HasKey(x => x.HiId);

            entity.Property(x => x.HiId)
                .HasColumnName("hi_id")
                .ValueGeneratedOnAdd();

            entity.Property(x => x.PaId)
                .HasColumnName("pa_id")
                .IsRequired();

            entity.HasIndex(x => x.PaId)
                .IsUnique()
                .HasDatabaseName("UQ_HISTORIA_paciente");

            entity.Property(x => x.UsId)
                .HasColumnName("us_id");

            entity.Property(x => x.HiFechaAper)
                .HasColumnName("hi_fecha_aper")
                .HasPrecision(0)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.Property(x => x.HiEstad)
                .HasColumnName("hi_estad")
                .HasMaxLength(20)
                .HasDefaultValue("ACTIVA")
                .IsRequired();

            entity.Property(x => x.HiFechaBloq)
                .HasColumnName("hi_fecha_bloq")
                .HasPrecision(0);

            entity.Property(x => x.HiMotivBloq)
                .HasColumnName("hi_motiv_bloq")
                .HasMaxLength(500);

            entity.Property(x => x.HiVersion)
                .HasColumnName("hi_version")
                .IsRowVersion();

            entity.HasOne<Paciente>()
                .WithMany()
                .HasForeignKey(x => x.PaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(x => x.UsId)
                .OnDelete(DeleteBehavior.Restrict);
        });


        modelBuilder.Entity<Antecedente>(entity =>
        {
            entity.ToTable("ANTECEDENTE", "cli");

            entity.HasKey(x => x.AnId);

            entity.Property(x => x.AnId)
                .HasColumnName("an_id")
                .ValueGeneratedOnAdd();

            entity.Property(x => x.HiId)
                .HasColumnName("hi_id")
                .IsRequired();

            entity.Property(x => x.UsId)
                .HasColumnName("us_id");

            entity.Property(x => x.AnTipo)
                .HasColumnName("an_tipo")
                .HasMaxLength(80)
                .IsRequired();

            entity.Property(x => x.AnDescri)
                .HasColumnName("an_descri")
                .HasMaxLength(2000)
                .IsRequired();

            entity.Property(x => x.AnFechaReg)
                .HasColumnName("an_fecha_reg")
                .HasPrecision(0)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.Property(x => x.AnEstatus)
                .HasColumnName("an_estatus")
                .HasDefaultValue(true);

            entity.HasOne<Historia>()
                .WithMany()
                .HasForeignKey(x => x.HiId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(x => x.UsId)
                .OnDelete(DeleteBehavior.Restrict);
        });


        modelBuilder.Entity<Atencion>(entity =>
        {
            entity.ToTable("ATENCION", "cli");

            entity.HasKey(x => x.AtId);

            entity.Property(x => x.AtId)
                .HasColumnName("at_id")
                .ValueGeneratedOnAdd();

            entity.Property(x => x.HiId)
                .HasColumnName("hi_id")
                .IsRequired();

            entity.Property(x => x.CiId)
                .HasColumnName("ci_id");

            entity.Property(x => x.UsId)
                .HasColumnName("us_id")
                .IsRequired();

            entity.Property(x => x.AtFecha)
                .HasColumnName("at_fecha")
                .HasPrecision(0)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.Property(x => x.AtMotivCons)
                .HasColumnName("at_motiv_cons")
                .HasMaxLength(1000);

            entity.Property(x => x.AtValorClin)
                .HasColumnName("at_valor_clin")
                .HasMaxLength(3000);

            entity.Property(x => x.AtDiagn)
                .HasColumnName("at_diagn")
                .HasMaxLength(2000);

            entity.Property(x => x.AtTrata)
                .HasColumnName("at_trata")
                .HasMaxLength(3000);

            entity.Property(x => x.AtObser)
                .HasColumnName("at_obser")
                .HasMaxLength(3000);

            entity.Property(x => x.AtEstad)
                .HasColumnName("at_estad")
                .HasMaxLength(20)
                .HasDefaultValue("ABIERTA")
                .IsRequired();

            entity.Property(x => x.AtFechaModi)
                .HasColumnName("at_fecha_modi")
                .HasPrecision(0);

            entity.Property(x => x.AtVersion)
                .HasColumnName("at_version")
                .IsRowVersion();

            entity.HasOne<Historia>()
                .WithMany()
                .HasForeignKey(x => x.HiId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Cita>()
                .WithMany()
                .HasForeignKey(x => x.CiId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(x => x.UsId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.CiId)
                .IsUnique()
                .HasFilter("[ci_id] IS NOT NULL")
                .HasDatabaseName("UX_ATENCION_cita");

            entity.HasIndex(x => new { x.HiId, x.AtFecha })
                .IsDescending(false, true)
                .HasDatabaseName("IX_ATENCION_historia");
        });


        modelBuilder.Entity<Evolucion>(entity =>
        {
            entity.ToTable("EVOLUCION", "cli");

            entity.HasKey(x => x.EvId);

            entity.Property(x => x.EvId)
                .HasColumnName("ev_id")
                .ValueGeneratedOnAdd();

            entity.Property(x => x.AtId)
                .HasColumnName("at_id")
                .IsRequired();

            entity.Property(x => x.UsId)
                .HasColumnName("us_id")
                .IsRequired();

            entity.Property(x => x.EvFecha)
                .HasColumnName("ev_fecha")
                .HasPrecision(0)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.Property(x => x.EvDescri)
                .HasColumnName("ev_descri")
                .HasMaxLength(3000)
                .IsRequired();

            entity.Property(x => x.EvEstatus)
                .HasColumnName("ev_estatus")
                .HasDefaultValue(true);

            entity.HasOne<Atencion>()
                .WithMany()
                .HasForeignKey(x => x.AtId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(x => x.UsId)
                .OnDelete(DeleteBehavior.Restrict);
        });


        modelBuilder.Entity<Adjunto>(entity =>
        {
            entity.ToTable("ADJUNTO", "cli");

            entity.HasKey(x => x.AdId);

            entity.Property(x => x.AdId)
                .HasColumnName("ad_id")
                .ValueGeneratedOnAdd();

            entity.Property(x => x.AtId)
                .HasColumnName("at_id")
                .IsRequired();

            entity.Property(x => x.UsId)
                .HasColumnName("us_id")
                .IsRequired();

            entity.Property(x => x.AdNombre)
                .HasColumnName("ad_nombre")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(x => x.AdRutaArch)
                .HasColumnName("ad_ruta_arch")
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(x => x.AdTipoMime)
                .HasColumnName("ad_tipo_mime")
                .HasMaxLength(150);

            entity.Property(x => x.AdHuella)
                .HasColumnName("ad_huella")
                .HasMaxLength(128);

            entity.Property(x => x.AdTamanByte)
                .HasColumnName("ad_taman_byte");

            entity.Property(x => x.AdFechaCarg)
                .HasColumnName("ad_fecha_carg")
                .HasPrecision(0)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.Property(x => x.AdEstatus)
                .HasColumnName("ad_estatus")
                .HasDefaultValue(true);

            entity.HasOne<Atencion>()
                .WithMany()
                .HasForeignKey(x => x.AtId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(x => x.UsId)
                .OnDelete(DeleteBehavior.Restrict);
        });


        modelBuilder.Entity<Insumo>(entity =>
        {
            entity.ToTable("INSUMO", "inv");

            entity.HasKey(x => x.InId);

            entity.Property(x => x.InId)
                .HasColumnName("in_id")
                .ValueGeneratedOnAdd();

            entity.Property(x => x.InNombre)
                .HasColumnName("in_nombre")
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.InCateg)
                .HasColumnName("in_categ")
                .HasMaxLength(80)
                .IsRequired();

            entity.Property(x => x.InUnidad)
                .HasColumnName("in_unidad")
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.InStockActu)
                .HasColumnName("in_stock_actu")
                .HasColumnType("decimal(12,2)")
                .HasDefaultValue(0m);

            entity.Property(x => x.InStockMini)
                .HasColumnName("in_stock_mini")
                .HasColumnType("decimal(12,2)")
                .HasDefaultValue(0m);

            entity.Property(x => x.InFechaCrea)
                .HasColumnName("in_fecha_crea")
                .HasPrecision(0)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.Property(x => x.InFechaModi)
                .HasColumnName("in_fecha_modi")
                .HasPrecision(0);

            entity.Property(x => x.InEstatus)
                .HasColumnName("in_estatus")
                .HasDefaultValue(true);

            entity.Property(x => x.InVersion)
                .HasColumnName("in_version")
                .IsRowVersion();
        });


        modelBuilder.Entity<Movimiento>(entity =>
        {
            entity.ToTable("MOVIMIENTO", "inv");

            entity.HasKey(x => x.MoId);

            entity.Property(x => x.MoId)
                .HasColumnName("mo_id")
                .ValueGeneratedOnAdd();

            entity.Property(x => x.InId)
                .HasColumnName("in_id")
                .IsRequired();

            entity.Property(x => x.AtId)
                .HasColumnName("at_id");

            entity.Property(x => x.UsId)
                .HasColumnName("us_id")
                .IsRequired();

            entity.Property(x => x.MoTipo)
                .HasColumnName("mo_tipo")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.MoCant)
                .HasColumnName("mo_cant")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            entity.Property(x => x.MoStockAnte)
                .HasColumnName("mo_stock_ante")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            entity.Property(x => x.MoStockNuev)
                .HasColumnName("mo_stock_nuev")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            entity.Property(x => x.MoFechaHora)
                .HasColumnName("mo_fecha_hora")
                .HasPrecision(0)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.Property(x => x.MoMotivo)
                .HasColumnName("mo_motivo")
                .HasMaxLength(500)
                .IsRequired();

            entity.HasOne<Insumo>()
                .WithMany()
                .HasForeignKey(x => x.InId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Atencion>()
                .WithMany()
                .HasForeignKey(x => x.AtId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(x => x.UsId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new
            {
                x.InId,
                x.MoFechaHora
            })
                .IsDescending(false, true)
                .HasDatabaseName("IX_MOVIMIENTO_insumo");
        });

    }
}
