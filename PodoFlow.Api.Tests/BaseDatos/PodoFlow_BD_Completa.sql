SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/* ============================================================================
   1. ESQUEMAS
============================================================================ */
IF SCHEMA_ID('seg') IS NULL EXEC('CREATE SCHEMA seg');
IF SCHEMA_ID('pac') IS NULL EXEC('CREATE SCHEMA pac');
IF SCHEMA_ID('cit') IS NULL EXEC('CREATE SCHEMA cit');
IF SCHEMA_ID('cli') IS NULL EXEC('CREATE SCHEMA cli');
IF SCHEMA_ID('com') IS NULL EXEC('CREATE SCHEMA com');
IF SCHEMA_ID('fin') IS NULL EXEC('CREATE SCHEMA fin');
IF SCHEMA_ID('inv') IS NULL EXEC('CREATE SCHEMA inv');
IF SCHEMA_ID('pri') IS NULL EXEC('CREATE SCHEMA pri');
IF SCHEMA_ID('aud') IS NULL EXEC('CREATE SCHEMA aud');
IF SCHEMA_ID('ana') IS NULL EXEC('CREATE SCHEMA ana');
IF SCHEMA_ID('rep') IS NULL EXEC('CREATE SCHEMA rep');
GO

/* ============================================================================
   2. SEGURIDAD - seg
============================================================================ */

CREATE TABLE seg.USUARIO
(
    us_id           BIGINT IDENTITY(1,1) NOT NULL,
    us_nombre       NVARCHAR(150) NOT NULL,
    us_correo       NVARCHAR(320) NOT NULL,
    us_entra_oid    UNIQUEIDENTIFIER NULL,
    us_entra_tid    UNIQUEIDENTIFIER NULL,
    us_fecha_crea   DATETIME2(0) NOT NULL CONSTRAINT DF_USUARIO_fecha_crea DEFAULT SYSUTCDATETIME(),
    us_fecha_modi   DATETIME2(0) NULL,
    us_ultim_acces  DATETIME2(0) NULL,
    us_estatus      BIT NOT NULL CONSTRAINT DF_USUARIO_estatus DEFAULT (1),
    us_version      ROWVERSION NOT NULL,

    CONSTRAINT PK_USUARIO PRIMARY KEY (us_id),
    CONSTRAINT UQ_USUARIO_correo UNIQUE (us_correo)
);
GO

CREATE UNIQUE INDEX UX_USUARIO_entra_oid
ON seg.USUARIO(us_entra_oid)
WHERE us_entra_oid IS NOT NULL;
GO

CREATE TABLE seg.ROL
(
    ro_id           INT IDENTITY(1,1) NOT NULL,
    ro_codigo       NVARCHAR(50) NOT NULL,
    ro_nombre       NVARCHAR(100) NOT NULL,
    ro_descri       NVARCHAR(500) NULL,
    ro_fecha_crea   DATETIME2(0) NOT NULL CONSTRAINT DF_ROL_fecha_crea DEFAULT SYSUTCDATETIME(),
    ro_estatus      BIT NOT NULL CONSTRAINT DF_ROL_estatus DEFAULT (1),

    CONSTRAINT PK_ROL PRIMARY KEY (ro_id),
    CONSTRAINT UQ_ROL_codigo UNIQUE (ro_codigo)
);
GO

CREATE TABLE seg.PERMISO
(
    pe_id           INT IDENTITY(1,1) NOT NULL,
    pe_codigo       NVARCHAR(80) NOT NULL,
    pe_nombre       NVARCHAR(120) NOT NULL,
    pe_modulo       NVARCHAR(80) NOT NULL,
    pe_descri       NVARCHAR(500) NULL,
    pe_estatus      BIT NOT NULL CONSTRAINT DF_PERMISO_estatus DEFAULT (1),

    CONSTRAINT PK_PERMISO PRIMARY KEY (pe_id),
    CONSTRAINT UQ_PERMISO_codigo UNIQUE (pe_codigo)
);
GO

CREATE TABLE seg.US_RO
(
    us_id           BIGINT NOT NULL,
    ro_id           INT NOT NULL,
    ur_fecha_asig   DATETIME2(0) NOT NULL CONSTRAINT DF_US_RO_fecha_asig DEFAULT SYSUTCDATETIME(),
    ur_estatus      BIT NOT NULL CONSTRAINT DF_US_RO_estatus DEFAULT (1),

    CONSTRAINT PK_US_RO PRIMARY KEY (us_id, ro_id),
    CONSTRAINT FK_US_RO_USUARIO FOREIGN KEY (us_id) REFERENCES seg.USUARIO(us_id),
    CONSTRAINT FK_US_RO_ROL FOREIGN KEY (ro_id) REFERENCES seg.ROL(ro_id)
);
GO

CREATE TABLE seg.RO_PE
(
    ro_id           INT NOT NULL,
    pe_id           INT NOT NULL,
    rp_fecha_asig   DATETIME2(0) NOT NULL CONSTRAINT DF_RO_PE_fecha_asig DEFAULT SYSUTCDATETIME(),
    rp_estatus      BIT NOT NULL CONSTRAINT DF_RO_PE_estatus DEFAULT (1),

    CONSTRAINT PK_RO_PE PRIMARY KEY (ro_id, pe_id),
    CONSTRAINT FK_RO_PE_ROL FOREIGN KEY (ro_id) REFERENCES seg.ROL(ro_id),
    CONSTRAINT FK_RO_PE_PERMISO FOREIGN KEY (pe_id) REFERENCES seg.PERMISO(pe_id)
);
GO

CREATE TABLE seg.INTENTO_ACCESO
(
    in_id           BIGINT IDENTITY(1,1) NOT NULL,
    us_id           BIGINT NULL,
    in_ident_acce        NVARCHAR(320) NULL,
    in_fecha_hora   DATETIME2(0) NOT NULL CONSTRAINT DF_INTENTO_fecha_hora DEFAULT SYSUTCDATETIME(),
    in_exitoso      BIT NOT NULL,
    in_motivo       NVARCHAR(300) NULL,
    in_direc_ip     NVARCHAR(45) NULL,
    in_agent_usua   NVARCHAR(500) NULL,

    CONSTRAINT PK_INTENTO_ACCESO PRIMARY KEY (in_id),
    CONSTRAINT FK_INTENTO_USUARIO FOREIGN KEY (us_id) REFERENCES seg.USUARIO(us_id)
);
GO

CREATE INDEX IX_INTENTO_USUARIO_fecha
ON seg.INTENTO_ACCESO(us_id, in_fecha_hora DESC);
GO

/* ============================================================================
   3. PACIENTES - pac
============================================================================ */

CREATE TABLE pac.PACIENTE
(
    pa_id           BIGINT IDENTITY(1,1) NOT NULL,
    pa_tipo_ident   NVARCHAR(20) NOT NULL,
    pa_ident        NVARCHAR(30) NOT NULL,
    pa_nombres      NVARCHAR(100) NOT NULL,
    pa_apelli       NVARCHAR(100) NOT NULL,
    pa_fecha_naci   DATE NULL,
    pa_genero       NVARCHAR(30) NULL,
    pa_direc        NVARCHAR(300) NULL,
    pa_fecha_crea   DATETIME2(0) NOT NULL CONSTRAINT DF_PACIENTE_fecha_crea DEFAULT SYSUTCDATETIME(),
    pa_fecha_modi   DATETIME2(0) NULL,
    pa_fecha_baja   DATETIME2(0) NULL,
    pa_motiv_baja   NVARCHAR(500) NULL,
    pa_estatus      BIT NOT NULL CONSTRAINT DF_PACIENTE_estatus DEFAULT (1),
    pa_version      ROWVERSION NOT NULL,

    CONSTRAINT PK_PACIENTE PRIMARY KEY (pa_id),
    CONSTRAINT UQ_PACIENTE_ident UNIQUE (pa_tipo_ident, pa_ident),
    CONSTRAINT CK_PACIENTE_tipo_ident CHECK
        (pa_tipo_ident IN ('CEDULA','PASAPORTE','RUC','OTRO'))
);
GO
UPDATE pac.PACIENTE
SET pa_tipo_ident = 'CEDULA', pa_ident = '1710034065', pa_nombres = 'Juan', pa_apelli = 'Pérez'
WHERE pa_id = 1;

CREATE INDEX IX_PACIENTE_nombre
ON pac.PACIENTE(pa_apelli, pa_nombres);
GO

/* Relación opcional 1:1 entre PACIENTE y USUARIO */
CREATE TABLE pac.PA_US
(
    pa_id           BIGINT NOT NULL,
    us_id           BIGINT NOT NULL,
    pu_fecha_asig   DATETIME2(0) NOT NULL CONSTRAINT DF_PA_US_fecha_asig DEFAULT SYSUTCDATETIME(),
    pu_estatus      BIT NOT NULL CONSTRAINT DF_PA_US_estatus DEFAULT (1),

    CONSTRAINT PK_PA_US PRIMARY KEY (pa_id, us_id),
    CONSTRAINT UQ_PA_US_pa UNIQUE (pa_id),
    CONSTRAINT UQ_PA_US_us UNIQUE (us_id),
    CONSTRAINT FK_PA_US_PACIENTE FOREIGN KEY (pa_id) REFERENCES pac.PACIENTE(pa_id),
    CONSTRAINT FK_PA_US_USUARIO FOREIGN KEY (us_id) REFERENCES seg.USUARIO(us_id)
);
GO

CREATE TABLE pac.DATO_CONTACTO
(
    da_id           BIGINT IDENTITY(1,1) NOT NULL,
    pa_id           BIGINT NOT NULL,
    da_tipo         NVARCHAR(20) NOT NULL,
    da_valor        NVARCHAR(320) NOT NULL,
    da_princ        BIT NOT NULL CONSTRAINT DF_DATO_CONTACTO_princ DEFAULT (0),
    da_verif        BIT NOT NULL CONSTRAINT DF_DATO_CONTACTO_verif DEFAULT (0),
    da_fecha_crea   DATETIME2(0) NOT NULL CONSTRAINT DF_DATO_CONTACTO_fecha_crea DEFAULT SYSUTCDATETIME(),
    da_estatus      BIT NOT NULL CONSTRAINT DF_DATO_CONTACTO_estatus DEFAULT (1),

    CONSTRAINT PK_DATO_CONTACTO PRIMARY KEY (da_id),
    CONSTRAINT FK_DATO_CONTACTO_PACIENTE FOREIGN KEY (pa_id) REFERENCES pac.PACIENTE(pa_id),
    CONSTRAINT CK_DATO_CONTACTO_tipo CHECK
        (da_tipo IN ('CELULAR','TELEFONO','CORREO','WHATSAPP','OTRO'))
);
GO

CREATE INDEX IX_DATO_CONTACTO_paciente
ON pac.DATO_CONTACTO(pa_id, da_estatus);
GO

/* ============================================================================
   4. CITAS Y AGENDA - cit
============================================================================ */

CREATE TABLE cit.SERVICIO
(
    se_id           INT IDENTITY(1,1) NOT NULL,
    se_nombre       NVARCHAR(150) NOT NULL,
    se_descri       NVARCHAR(1000) NULL,
    se_precio       DECIMAL(12,2) NOT NULL,
    se_durac_min    SMALLINT NOT NULL,
    se_fecha_crea   DATETIME2(0) NOT NULL CONSTRAINT DF_SERVICIO_fecha_crea DEFAULT SYSUTCDATETIME(),
    se_fecha_modi   DATETIME2(0) NULL,
    se_estatus      BIT NOT NULL CONSTRAINT DF_SERVICIO_estatus DEFAULT (1),
    se_version      ROWVERSION NOT NULL,

    CONSTRAINT PK_SERVICIO PRIMARY KEY (se_id),
    CONSTRAINT CK_SERVICIO_precio CHECK (se_precio >= 0),
    CONSTRAINT CK_SERVICIO_durac CHECK (se_durac_min > 0)
);
GO

CREATE TABLE cit.HORARIO_ATENCION
(
    ho_id           INT IDENTITY(1,1) NOT NULL,
    us_id           BIGINT NULL,
    ho_dia_sem      TINYINT NOT NULL,
    ho_hora_ini     TIME(0) NOT NULL,
    ho_hora_fin     TIME(0) NOT NULL,
    ho_fecha_desd   DATE NULL,
    ho_fecha_hast   DATE NULL,
    ho_estatus      BIT NOT NULL CONSTRAINT DF_HORARIO_estatus DEFAULT (1),

    CONSTRAINT PK_HORARIO_ATENCION PRIMARY KEY (ho_id),
    CONSTRAINT FK_HORARIO_USUARIO FOREIGN KEY (us_id) REFERENCES seg.USUARIO(us_id),
    CONSTRAINT CK_HORARIO_dia CHECK (ho_dia_sem BETWEEN 1 AND 7),
    CONSTRAINT CK_HORARIO_horas CHECK (ho_hora_fin > ho_hora_ini),
    CONSTRAINT CK_HORARIO_fechas CHECK
        (ho_fecha_hast IS NULL OR ho_fecha_desd IS NULL OR ho_fecha_hast >= ho_fecha_desd)
);
GO

CREATE TABLE cit.BLOQUEO_AGENDA
(
    bl_id           BIGINT IDENTITY(1,1) NOT NULL,
    us_id           BIGINT NULL,
    bl_fecha_ini    DATETIME2(0) NOT NULL,
    bl_fecha_fin    DATETIME2(0) NOT NULL,
    bl_motivo       NVARCHAR(300) NOT NULL,
    bl_fecha_crea   DATETIME2(0) NOT NULL CONSTRAINT DF_BLOQUEO_fecha_crea DEFAULT SYSUTCDATETIME(),
    bl_estatus      BIT NOT NULL CONSTRAINT DF_BLOQUEO_estatus DEFAULT (1),

    CONSTRAINT PK_BLOQUEO_AGENDA PRIMARY KEY (bl_id),
    CONSTRAINT FK_BLOQUEO_USUARIO FOREIGN KEY (us_id) REFERENCES seg.USUARIO(us_id),
    CONSTRAINT CK_BLOQUEO_fechas CHECK (bl_fecha_fin > bl_fecha_ini)
);
GO

CREATE TABLE cit.CITA
(
    ci_id           BIGINT IDENTITY(1,1) NOT NULL,
    pa_id           BIGINT NOT NULL,
    se_id           INT NOT NULL,
    us_id           BIGINT NULL,
    ci_fecha_ini    DATETIME2(0) NOT NULL,
    ci_fecha_fin    DATETIME2(0) NOT NULL,
    ci_estad        NVARCHAR(20) NOT NULL CONSTRAINT DF_CITA_estad DEFAULT ('SOLICITADA'),
    ci_canal        NVARCHAR(30) NOT NULL CONSTRAINT DF_CITA_canal DEFAULT ('SISTEMA'),
    ci_obser        NVARCHAR(1000) NULL,
    ci_fecha_crea   DATETIME2(0) NOT NULL CONSTRAINT DF_CITA_fecha_crea DEFAULT SYSUTCDATETIME(),
    ci_fecha_modi   DATETIME2(0) NULL,
    ci_version      ROWVERSION NOT NULL,

    CONSTRAINT PK_CITA PRIMARY KEY (ci_id),
    CONSTRAINT FK_CITA_PACIENTE FOREIGN KEY (pa_id) REFERENCES pac.PACIENTE(pa_id),
    CONSTRAINT FK_CITA_SERVICIO FOREIGN KEY (se_id) REFERENCES cit.SERVICIO(se_id),
    CONSTRAINT FK_CITA_USUARIO FOREIGN KEY (us_id) REFERENCES seg.USUARIO(us_id),
    CONSTRAINT CK_CITA_fechas CHECK (ci_fecha_fin > ci_fecha_ini),
    CONSTRAINT CK_CITA_estad CHECK
        (ci_estad IN ('SOLICITADA','CONFIRMADA','ATENDIDA','CANCELADA','REPROGRAMADA','NO_ASISTE'))
);
GO

CREATE INDEX IX_CITA_agenda
ON cit.CITA(ci_fecha_ini, ci_fecha_fin, ci_estad);
GO

CREATE INDEX IX_CITA_paciente
ON cit.CITA(pa_id, ci_fecha_ini DESC);
GO

CREATE TABLE cit.CAMBIO_CITA
(
    ca_id           BIGINT IDENTITY(1,1) NOT NULL,
    ci_id           BIGINT NOT NULL,
    us_id           BIGINT NULL,
    ca_estad_ante   NVARCHAR(20) NULL,
    ca_estad_nuev   NVARCHAR(20) NULL,
    ca_fecha_ante   DATETIME2(0) NULL,
    ca_fecha_nuev   DATETIME2(0) NULL,
    ca_motivo       NVARCHAR(500) NULL,
    ca_fecha_camb   DATETIME2(0) NOT NULL CONSTRAINT DF_CAMBIO_CITA_fecha DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_CAMBIO_CITA PRIMARY KEY (ca_id),
    CONSTRAINT FK_CAMBIO_CITA_CITA FOREIGN KEY (ci_id) REFERENCES cit.CITA(ci_id),
    CONSTRAINT FK_CAMBIO_CITA_USUARIO FOREIGN KEY (us_id) REFERENCES seg.USUARIO(us_id)
);
GO

CREATE INDEX IX_CAMBIO_CITA_cita
ON cit.CAMBIO_CITA(ci_id, ca_fecha_camb DESC);
GO

/* ============================================================================
   5. CLÍNICA - cli
============================================================================ */

CREATE TABLE cli.HISTORIA
(
    hi_id           BIGINT IDENTITY(1,1) NOT NULL,
    pa_id           BIGINT NOT NULL,
    us_id           BIGINT NULL,
    hi_fecha_aper   DATETIME2(0) NOT NULL CONSTRAINT DF_HISTORIA_fecha_aper DEFAULT SYSUTCDATETIME(),
    hi_estad        NVARCHAR(20) NOT NULL CONSTRAINT DF_HISTORIA_estad DEFAULT ('ACTIVA'),
    hi_fecha_bloq   DATETIME2(0) NULL,
    hi_motiv_bloq   NVARCHAR(500) NULL,
    hi_version      ROWVERSION NOT NULL,

    CONSTRAINT PK_HISTORIA PRIMARY KEY (hi_id),
    CONSTRAINT UQ_HISTORIA_paciente UNIQUE (pa_id),
    CONSTRAINT FK_HISTORIA_PACIENTE FOREIGN KEY (pa_id) REFERENCES pac.PACIENTE(pa_id),
    CONSTRAINT FK_HISTORIA_USUARIO FOREIGN KEY (us_id) REFERENCES seg.USUARIO(us_id),
    CONSTRAINT CK_HISTORIA_estad CHECK (hi_estad IN ('ACTIVA','BLOQUEADA','CERRADA'))
);
GO

CREATE TABLE cli.ANTECEDENTE
(
    an_id           BIGINT IDENTITY(1,1) NOT NULL,
    hi_id           BIGINT NOT NULL,
    us_id           BIGINT NULL,
    an_tipo         NVARCHAR(80) NOT NULL,
    an_descri       NVARCHAR(2000) NOT NULL,
    an_fecha_reg    DATETIME2(0) NOT NULL CONSTRAINT DF_ANTECEDENTE_fecha DEFAULT SYSUTCDATETIME(),
    an_estatus      BIT NOT NULL CONSTRAINT DF_ANTECEDENTE_estatus DEFAULT (1),

    CONSTRAINT PK_ANTECEDENTE PRIMARY KEY (an_id),
    CONSTRAINT FK_ANTECEDENTE_HISTORIA FOREIGN KEY (hi_id) REFERENCES cli.HISTORIA(hi_id),
    CONSTRAINT FK_ANTECEDENTE_USUARIO FOREIGN KEY (us_id) REFERENCES seg.USUARIO(us_id)
);
GO

CREATE TABLE cli.ATENCION
(
    at_id           BIGINT IDENTITY(1,1) NOT NULL,
    hi_id           BIGINT NOT NULL,
    ci_id           BIGINT NULL,
    us_id           BIGINT NOT NULL,
    at_fecha        DATETIME2(0) NOT NULL CONSTRAINT DF_ATENCION_fecha DEFAULT SYSUTCDATETIME(),
    at_motiv_cons   NVARCHAR(1000) NULL,
    at_valor_clin   NVARCHAR(3000) NULL,
    at_diagn        NVARCHAR(2000) NULL,
    at_trata        NVARCHAR(3000) NULL,
    at_obser        NVARCHAR(3000) NULL,
    at_estad        NVARCHAR(20) NOT NULL CONSTRAINT DF_ATENCION_estad DEFAULT ('ABIERTA'),
    at_fecha_modi   DATETIME2(0) NULL,
    at_version      ROWVERSION NOT NULL,

    CONSTRAINT PK_ATENCION PRIMARY KEY (at_id),
    CONSTRAINT FK_ATENCION_HISTORIA FOREIGN KEY (hi_id) REFERENCES cli.HISTORIA(hi_id),
    CONSTRAINT FK_ATENCION_CITA FOREIGN KEY (ci_id) REFERENCES cit.CITA(ci_id),
    CONSTRAINT FK_ATENCION_USUARIO FOREIGN KEY (us_id) REFERENCES seg.USUARIO(us_id),
    CONSTRAINT CK_ATENCION_estad CHECK (at_estad IN ('ABIERTA','CERRADA','BLOQUEADA'))
);
GO

CREATE UNIQUE INDEX UX_ATENCION_cita
ON cli.ATENCION(ci_id)
WHERE ci_id IS NOT NULL;
GO

CREATE INDEX IX_ATENCION_historia
ON cli.ATENCION(hi_id, at_fecha DESC);
GO

CREATE TABLE cli.EVOLUCION
(
    ev_id           BIGINT IDENTITY(1,1) NOT NULL,
    at_id           BIGINT NOT NULL,
    us_id           BIGINT NOT NULL,
    ev_fecha        DATETIME2(0) NOT NULL CONSTRAINT DF_EVOLUCION_fecha DEFAULT SYSUTCDATETIME(),
    ev_descri       NVARCHAR(3000) NOT NULL,
    ev_estatus      BIT NOT NULL CONSTRAINT DF_EVOLUCION_estatus DEFAULT (1),

    CONSTRAINT PK_EVOLUCION PRIMARY KEY (ev_id),
    CONSTRAINT FK_EVOLUCION_ATENCION FOREIGN KEY (at_id) REFERENCES cli.ATENCION(at_id),
    CONSTRAINT FK_EVOLUCION_USUARIO FOREIGN KEY (us_id) REFERENCES seg.USUARIO(us_id)
);
GO

CREATE TABLE cli.ADJUNTO
(
    ad_id           BIGINT IDENTITY(1,1) NOT NULL,
    at_id           BIGINT NOT NULL,
    us_id           BIGINT NOT NULL,
    ad_nombre       NVARCHAR(255) NOT NULL,
    ad_ruta_arch     NVARCHAR(1000) NOT NULL,
    ad_tipo_mime    NVARCHAR(150) NULL,
    ad_huella         NVARCHAR(128) NULL,
    ad_taman_byte   BIGINT NULL,
    ad_fecha_carg   DATETIME2(0) NOT NULL CONSTRAINT DF_ADJUNTO_fecha DEFAULT SYSUTCDATETIME(),
    ad_estatus      BIT NOT NULL CONSTRAINT DF_ADJUNTO_estatus DEFAULT (1),

    CONSTRAINT PK_ADJUNTO PRIMARY KEY (ad_id),
    CONSTRAINT FK_ADJUNTO_ATENCION FOREIGN KEY (at_id) REFERENCES cli.ATENCION(at_id),
    CONSTRAINT FK_ADJUNTO_USUARIO FOREIGN KEY (us_id) REFERENCES seg.USUARIO(us_id),
    CONSTRAINT CK_ADJUNTO_taman CHECK (ad_taman_byte IS NULL OR ad_taman_byte >= 0)
);
GO

/* ============================================================================
   6. PRIVACIDAD Y LOPDP - pri
============================================================================ */

CREATE TABLE pri.FINALIDAD
(
    fi_id           INT IDENTITY(1,1) NOT NULL,
    fi_codigo       NVARCHAR(50) NOT NULL,
    fi_nombre       NVARCHAR(150) NOT NULL,
    fi_descri       NVARCHAR(1000) NULL,
    fi_requi_cons   BIT NOT NULL CONSTRAINT DF_FINALIDAD_requi DEFAULT (0),
    fi_estatus      BIT NOT NULL CONSTRAINT DF_FINALIDAD_estatus DEFAULT (1),

    CONSTRAINT PK_FINALIDAD PRIMARY KEY (fi_id),
    CONSTRAINT UQ_FINALIDAD_codigo UNIQUE (fi_codigo)
);
GO

CREATE TABLE pri.AVISO
(
    av_id           INT IDENTITY(1,1) NOT NULL,
    av_versi        NVARCHAR(30) NOT NULL,
    av_fecha_desd   DATE NOT NULL,
    av_fecha_hast   DATE NULL,
    av_ruta_arch     NVARCHAR(1000) NULL,
    av_huella         NVARCHAR(128) NULL,
    av_estatus      BIT NOT NULL CONSTRAINT DF_AVISO_estatus DEFAULT (1),

    CONSTRAINT PK_AVISO PRIMARY KEY (av_id),
    CONSTRAINT UQ_AVISO_versi UNIQUE (av_versi),
    CONSTRAINT CK_AVISO_fechas CHECK (av_fecha_hast IS NULL OR av_fecha_hast >= av_fecha_desd)
);
GO

CREATE TABLE pri.CONSENTIMIENTO
(
    co_id           BIGINT IDENTITY(1,1) NOT NULL,
    pa_id           BIGINT NOT NULL,
    fi_id           INT NOT NULL,
    av_id           INT NOT NULL,
    co_estad        NVARCHAR(15) NOT NULL,
    co_medio        NVARCHAR(50) NOT NULL,
    co_fecha_obte   DATETIME2(0) NOT NULL CONSTRAINT DF_CONSENTIMIENTO_fecha DEFAULT SYSUTCDATETIME(),
    co_fecha_revo   DATETIME2(0) NULL,
    co_obser        NVARCHAR(1000) NULL,
    co_version      ROWVERSION NOT NULL,

    CONSTRAINT PK_CONSENTIMIENTO PRIMARY KEY (co_id),
    CONSTRAINT UQ_CONSENTIMIENTO_pac_fin UNIQUE (pa_id, fi_id),
    CONSTRAINT FK_CONSENTIMIENTO_PACIENTE FOREIGN KEY (pa_id) REFERENCES pac.PACIENTE(pa_id),
    CONSTRAINT FK_CONSENTIMIENTO_FINALIDAD FOREIGN KEY (fi_id) REFERENCES pri.FINALIDAD(fi_id),
    CONSTRAINT FK_CONSENTIMIENTO_AVISO FOREIGN KEY (av_id) REFERENCES pri.AVISO(av_id),
    CONSTRAINT CK_CONSENTIMIENTO_estad CHECK (co_estad IN ('ACEPTADO','RECHAZADO','REVOCADO'))
);
GO

CREATE TABLE pri.EVENTO_CONSENTIMIENTO
(
    ev_id           BIGINT IDENTITY(1,1) NOT NULL,
    co_id           BIGINT NOT NULL,
    us_id           BIGINT NULL,
    ev_estad        NVARCHAR(15) NOT NULL,
    ev_fecha_hora   DATETIME2(0) NOT NULL CONSTRAINT DF_EVENTO_CONS_fecha DEFAULT SYSUTCDATETIME(),
    ev_medio        NVARCHAR(50) NOT NULL,
    ev_ruta_evid    NVARCHAR(1000) NULL,
    ev_obser        NVARCHAR(1000) NULL,

    CONSTRAINT PK_EVENTO_CONSENTIMIENTO PRIMARY KEY (ev_id),
    CONSTRAINT FK_EVENTO_CONS_CONSENTIMIENTO FOREIGN KEY (co_id) REFERENCES pri.CONSENTIMIENTO(co_id),
    CONSTRAINT FK_EVENTO_CONS_USUARIO FOREIGN KEY (us_id) REFERENCES seg.USUARIO(us_id),
    CONSTRAINT CK_EVENTO_CONS_estad CHECK (ev_estad IN ('ACEPTADO','RECHAZADO','REVOCADO'))
);
GO

CREATE TABLE pri.SOLICITUD
(
    so_id           BIGINT IDENTITY(1,1) NOT NULL,
    pa_id           BIGINT NOT NULL,
    us_id           BIGINT NULL,
    so_tipo_dere    NVARCHAR(25) NOT NULL,
    so_tipo_soli    NVARCHAR(20) NOT NULL CONSTRAINT DF_SOLICITUD_tipo_soli DEFAULT ('TITULAR'),
    so_nombr_soli   NVARCHAR(200) NOT NULL,
    so_ident_soli   NVARCHAR(50) NOT NULL,
    so_fecha_rece   DATETIME2(0) NOT NULL CONSTRAINT DF_SOLICITUD_fecha DEFAULT SYSUTCDATETIME(),
    so_estad        NVARCHAR(20) NOT NULL CONSTRAINT DF_SOLICITUD_estad DEFAULT ('RECIBIDA'),
    so_medio_resp   NVARCHAR(50) NULL,
    so_detal        NVARCHAR(3000) NULL,
    so_respue       NVARCHAR(3000) NULL,
    so_fecha_cier   DATETIME2(0) NULL,
    so_version      ROWVERSION NOT NULL,

    CONSTRAINT PK_SOLICITUD PRIMARY KEY (so_id),
    CONSTRAINT FK_SOLICITUD_PACIENTE FOREIGN KEY (pa_id) REFERENCES pac.PACIENTE(pa_id),
    CONSTRAINT FK_SOLICITUD_USUARIO FOREIGN KEY (us_id) REFERENCES seg.USUARIO(us_id),
    CONSTRAINT CK_SOLICITUD_tipo_dere CHECK
        (so_tipo_dere IN ('INFORMACION','ACCESO','CONSULTA','RECTIFICACION','ACTUALIZACION',
                          'ELIMINACION','OPOSICION','SUSPENSION','PORTABILIDAD')),
    CONSTRAINT CK_SOLICITUD_tipo_soli CHECK (so_tipo_soli IN ('TITULAR','REPRESENTANTE')),
    CONSTRAINT CK_SOLICITUD_estad CHECK
        (so_estad IN ('RECIBIDA','VALIDANDO','EN_PROCESO','RESPONDIDA','CERRADA','RECHAZADA'))
);
GO

CREATE INDEX IX_SOLICITUD_paciente
ON pri.SOLICITUD(pa_id, so_fecha_rece DESC);
GO

CREATE TABLE pri.ACCION_SOLICITUD
(
    ac_id           BIGINT IDENTITY(1,1) NOT NULL,
    so_id           BIGINT NOT NULL,
    us_id           BIGINT NOT NULL,
    ac_fecha_hora   DATETIME2(0) NOT NULL CONSTRAINT DF_ACCION_SOL_fecha DEFAULT SYSUTCDATETIME(),
    ac_estad_ante   NVARCHAR(20) NULL,
    ac_estad_nuev   NVARCHAR(20) NULL,
    ac_accion       NVARCHAR(1000) NOT NULL,
    ac_obser        NVARCHAR(2000) NULL,

    CONSTRAINT PK_ACCION_SOLICITUD PRIMARY KEY (ac_id),
    CONSTRAINT FK_ACCION_SOL_SOLICITUD FOREIGN KEY (so_id) REFERENCES pri.SOLICITUD(so_id),
    CONSTRAINT FK_ACCION_SOL_USUARIO FOREIGN KEY (us_id) REFERENCES seg.USUARIO(us_id)
);
GO

CREATE TABLE pri.EVIDENCIA_SOLICITUD
(
    ev_id           BIGINT IDENTITY(1,1) NOT NULL,
    so_id           BIGINT NOT NULL,
    us_id           BIGINT NOT NULL,
    ev_tipo         NVARCHAR(80) NOT NULL,
    ev_ruta_arch     NVARCHAR(1000) NOT NULL,
    ev_huella         NVARCHAR(128) NULL,
    ev_fecha_carg   DATETIME2(0) NOT NULL CONSTRAINT DF_EVIDENCIA_SOL_fecha DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_EVIDENCIA_SOLICITUD PRIMARY KEY (ev_id),
    CONSTRAINT FK_EVIDENCIA_SOL_SOLICITUD FOREIGN KEY (so_id) REFERENCES pri.SOLICITUD(so_id),
    CONSTRAINT FK_EVIDENCIA_SOL_USUARIO FOREIGN KEY (us_id) REFERENCES seg.USUARIO(us_id)
);
GO

CREATE TABLE pri.EXPORTACION
(
    ex_id           BIGINT IDENTITY(1,1) NOT NULL,
    so_id           BIGINT NOT NULL,
    us_id           BIGINT NOT NULL,
    ex_forma       NVARCHAR(20) NOT NULL,
    ex_ruta_arch     NVARCHAR(1000) NOT NULL,
    ex_huella         NVARCHAR(128) NULL,
    ex_fecha_gene   DATETIME2(0) NOT NULL CONSTRAINT DF_EXPORTACION_fecha DEFAULT SYSUTCDATETIME(),
    ex_fecha_entr   DATETIME2(0) NULL,
    ex_estad        NVARCHAR(20) NOT NULL CONSTRAINT DF_EXPORTACION_estad DEFAULT ('GENERADA'),

    CONSTRAINT PK_EXPORTACION PRIMARY KEY (ex_id),
    CONSTRAINT FK_EXPORTACION_SOLICITUD FOREIGN KEY (so_id) REFERENCES pri.SOLICITUD(so_id),
    CONSTRAINT FK_EXPORTACION_USUARIO FOREIGN KEY (us_id) REFERENCES seg.USUARIO(us_id),
    CONSTRAINT CK_EXPORTACION_format CHECK (ex_forma IN ('JSON','CSV','PDF','ZIP')),
    CONSTRAINT CK_EXPORTACION_estad CHECK (ex_estad IN ('GENERADA','ENTREGADA','ANULADA'))
);
GO

CREATE TABLE pri.TRATAMIENTO_DATOS
(
    tr_id           INT IDENTITY(1,1) NOT NULL,
    fi_id           INT NOT NULL,
    us_id           BIGINT NULL,
    tr_codigo       NVARCHAR(50) NOT NULL,
    tr_nombre       NVARCHAR(200) NOT NULL,
    tr_modulo       NVARCHAR(80) NOT NULL,
    tr_descri       NVARCHAR(2000) NULL,
    tr_categ_titu   NVARCHAR(500) NULL,
    tr_base_legit   NVARCHAR(1000) NULL,
    tr_plazo_conse  NVARCHAR(500) NULL,
    tr_medi_segur   NVARCHAR(2000) NULL,
    tr_fecha_revi   DATE NULL,
    tr_estatus      BIT NOT NULL CONSTRAINT DF_TRATAMIENTO_estatus DEFAULT (1),

    CONSTRAINT PK_TRATAMIENTO_DATOS PRIMARY KEY (tr_id),
    CONSTRAINT UQ_TRATAMIENTO_codigo UNIQUE (tr_codigo),
    CONSTRAINT FK_TRATAMIENTO_FINALIDAD FOREIGN KEY (fi_id) REFERENCES pri.FINALIDAD(fi_id),
    CONSTRAINT FK_TRATAMIENTO_USUARIO FOREIGN KEY (us_id) REFERENCES seg.USUARIO(us_id)
);
GO

CREATE TABLE pri.CATEGORIA_DATO
(
    ca_id           INT IDENTITY(1,1) NOT NULL,
    ca_nombre       NVARCHAR(150) NOT NULL,
    ca_descri       NVARCHAR(1000) NULL,
    ca_sensi        BIT NOT NULL CONSTRAINT DF_CATEGORIA_sensi DEFAULT (0),
    ca_estatus      BIT NOT NULL CONSTRAINT DF_CATEGORIA_estatus DEFAULT (1),

    CONSTRAINT PK_CATEGORIA_DATO PRIMARY KEY (ca_id),
    CONSTRAINT UQ_CATEGORIA_nombre UNIQUE (ca_nombre)
);
GO

CREATE TABLE pri.TR_CA
(
    tr_id           INT NOT NULL,
    ca_id           INT NOT NULL,
    tc_fecha_asig   DATETIME2(0) NOT NULL CONSTRAINT DF_TR_CA_fecha DEFAULT SYSUTCDATETIME(),
    tc_estatus      BIT NOT NULL CONSTRAINT DF_TR_CA_estatus DEFAULT (1),

    CONSTRAINT PK_TR_CA PRIMARY KEY (tr_id, ca_id),
    CONSTRAINT FK_TR_CA_TRATAMIENTO FOREIGN KEY (tr_id) REFERENCES pri.TRATAMIENTO_DATOS(tr_id),
    CONSTRAINT FK_TR_CA_CATEGORIA FOREIGN KEY (ca_id) REFERENCES pri.CATEGORIA_DATO(ca_id)
);
GO

CREATE TABLE pri.ENCARGADO
(
    en_id           INT IDENTITY(1,1) NOT NULL,
    en_nombre       NVARCHAR(200) NOT NULL,
    en_servi        NVARCHAR(200) NOT NULL,
    en_rol_trata    NVARCHAR(100) NULL,
    en_descri       NVARCHAR(1500) NULL,
    en_riesgo       NVARCHAR(1000) NULL,
    en_medi_contr   NVARCHAR(2000) NULL,
    en_medi_tecni   NVARCHAR(2000) NULL,
    en_estatus      BIT NOT NULL CONSTRAINT DF_ENCARGADO_estatus DEFAULT (1),

    CONSTRAINT PK_ENCARGADO PRIMARY KEY (en_id)
);
GO

CREATE TABLE pri.TR_EN
(
    tr_id           INT NOT NULL,
    en_id           INT NOT NULL,
    te_fecha_asig   DATETIME2(0) NOT NULL CONSTRAINT DF_TR_EN_fecha DEFAULT SYSUTCDATETIME(),
    te_estatus      BIT NOT NULL CONSTRAINT DF_TR_EN_estatus DEFAULT (1),

    CONSTRAINT PK_TR_EN PRIMARY KEY (tr_id, en_id),
    CONSTRAINT FK_TR_EN_TRATAMIENTO FOREIGN KEY (tr_id) REFERENCES pri.TRATAMIENTO_DATOS(tr_id),
    CONSTRAINT FK_TR_EN_ENCARGADO FOREIGN KEY (en_id) REFERENCES pri.ENCARGADO(en_id)
);
GO

CREATE TABLE pri.RIESGO
(
    ri_id           BIGINT IDENTITY(1,1) NOT NULL,
    tr_id           INT NOT NULL,
    ri_descri       NVARCHAR(2000) NOT NULL,
    ri_proba        TINYINT NOT NULL,
    ri_impac        TINYINT NOT NULL,
    ri_nivel        TINYINT NOT NULL,
    ri_medi_mitig   NVARCHAR(2000) NULL,
    ri_riesg_resid  TINYINT NULL,
    ri_estad        NVARCHAR(20) NOT NULL CONSTRAINT DF_RIESGO_estad DEFAULT ('ABIERTO'),
    ri_fecha_revi   DATE NULL,

    CONSTRAINT PK_RIESGO PRIMARY KEY (ri_id),
    CONSTRAINT FK_RIESGO_TRATAMIENTO FOREIGN KEY (tr_id) REFERENCES pri.TRATAMIENTO_DATOS(tr_id),
    CONSTRAINT CK_RIESGO_proba CHECK (ri_proba BETWEEN 1 AND 5),
    CONSTRAINT CK_RIESGO_impac CHECK (ri_impac BETWEEN 1 AND 5),
    CONSTRAINT CK_RIESGO_nivel CHECK (ri_nivel BETWEEN 1 AND 25),
    CONSTRAINT CK_RIESGO_resid CHECK (ri_riesg_resid IS NULL OR ri_riesg_resid BETWEEN 1 AND 25),
    CONSTRAINT CK_RIESGO_estad CHECK (ri_estad IN ('ABIERTO','MITIGADO','ACEPTADO','CERRADO'))
);
GO

CREATE TABLE pri.EVALUACION_IMPACTO
(
    ev_id           BIGINT IDENTITY(1,1) NOT NULL,
    us_id           BIGINT NOT NULL,
    ev_versi        NVARCHAR(30) NOT NULL,
    ev_fecha        DATE NOT NULL,
    ev_alcan        NVARCHAR(2000) NOT NULL,
    ev_descri       NVARCHAR(3000) NULL,
    ev_conclu       NVARCHAR(3000) NULL,
    ev_nivel_resid  TINYINT NULL,
    ev_estad        NVARCHAR(20) NOT NULL CONSTRAINT DF_EVALUACION_estad DEFAULT ('BORRADOR'),
    ev_fecha_modi   DATETIME2(0) NULL,

    CONSTRAINT PK_EVALUACION_IMPACTO PRIMARY KEY (ev_id),
    CONSTRAINT FK_EVALUACION_USUARIO FOREIGN KEY (us_id) REFERENCES seg.USUARIO(us_id),
    CONSTRAINT CK_EVALUACION_nivel CHECK (ev_nivel_resid IS NULL OR ev_nivel_resid BETWEEN 1 AND 25),
    CONSTRAINT CK_EVALUACION_estad CHECK (ev_estad IN ('BORRADOR','APROBADA','REVISADA','CERRADA'))
);
GO

CREATE TABLE pri.EV_RI
(
    ev_id           BIGINT NOT NULL,
    ri_id           BIGINT NOT NULL,
    er_fecha_asig   DATETIME2(0) NOT NULL CONSTRAINT DF_EV_RI_fecha DEFAULT SYSUTCDATETIME(),
    er_obser        NVARCHAR(1000) NULL,

    CONSTRAINT PK_EV_RI PRIMARY KEY (ev_id, ri_id),
    CONSTRAINT FK_EV_RI_EVALUACION FOREIGN KEY (ev_id) REFERENCES pri.EVALUACION_IMPACTO(ev_id),
    CONSTRAINT FK_EV_RI_RIESGO FOREIGN KEY (ri_id) REFERENCES pri.RIESGO(ri_id)
);
GO

CREATE TABLE pri.DOCUMENTO
(
    do_id           BIGINT IDENTITY(1,1) NOT NULL,
    us_id           BIGINT NOT NULL,
    do_tipo         NVARCHAR(50) NOT NULL,
    do_versi        NVARCHAR(30) NULL,
    do_nombre       NVARCHAR(255) NOT NULL,
    do_ruta_arch     NVARCHAR(1000) NOT NULL,
    do_huella         NVARCHAR(128) NULL,
    do_fecha_carg   DATETIME2(0) NOT NULL CONSTRAINT DF_DOCUMENTO_fecha DEFAULT SYSUTCDATETIME(),
    do_estatus      BIT NOT NULL CONSTRAINT DF_DOCUMENTO_estatus DEFAULT (1),

    CONSTRAINT PK_DOCUMENTO PRIMARY KEY (do_id),
    CONSTRAINT FK_DOCUMENTO_USUARIO FOREIGN KEY (us_id) REFERENCES seg.USUARIO(us_id),
    CONSTRAINT CK_DOCUMENTO_tipo CHECK
        (do_tipo IN ('RAT','EIPD','MATRIZ_RIESGO','MATRIZ_ENCARGADO','EVIDENCIA_CONTROL',
                     'AVISO_PRIVACIDAD','OTRO'))
);
GO

CREATE TABLE pri.INCIDENTE
(
    in_id           BIGINT IDENTITY(1,1) NOT NULL,
    us_id           BIGINT NOT NULL,
    in_fecha_dete   DATETIME2(0) NOT NULL,
    in_tipo         NVARCHAR(100) NOT NULL,
    in_modulo       NVARCHAR(80) NOT NULL,
    in_descri       NVARCHAR(3000) NOT NULL,
    in_datos_afec   NVARCHAR(2000) NULL,
    in_impac_preli  NVARCHAR(1500) NULL,
    in_estad        NVARCHAR(20) NOT NULL CONSTRAINT DF_INCIDENTE_estad DEFAULT ('ABIERTO'),
    in_fecha_cier   DATETIME2(0) NULL,
    in_version      ROWVERSION NOT NULL,

    CONSTRAINT PK_INCIDENTE PRIMARY KEY (in_id),
    CONSTRAINT FK_INCIDENTE_USUARIO FOREIGN KEY (us_id) REFERENCES seg.USUARIO(us_id),
    CONSTRAINT CK_INCIDENTE_estad CHECK
        (in_estad IN ('ABIERTO','ANALISIS','CONTENIDO','RECUPERADO','CERRADO'))
);
GO

CREATE TABLE pri.ACCION_INCIDENTE
(
    ac_id           BIGINT IDENTITY(1,1) NOT NULL,
    in_id           BIGINT NOT NULL,
    us_id           BIGINT NOT NULL,
    ac_fecha_hora   DATETIME2(0) NOT NULL CONSTRAINT DF_ACCION_INC_fecha DEFAULT SYSUTCDATETIME(),
    ac_tipo         NVARCHAR(50) NOT NULL,
    ac_accion       NVARCHAR(2000) NOT NULL,
    ac_obser        NVARCHAR(1500) NULL,

    CONSTRAINT PK_ACCION_INCIDENTE PRIMARY KEY (ac_id),
    CONSTRAINT FK_ACCION_INC_INCIDENTE FOREIGN KEY (in_id) REFERENCES pri.INCIDENTE(in_id),
    CONSTRAINT FK_ACCION_INC_USUARIO FOREIGN KEY (us_id) REFERENCES seg.USUARIO(us_id)
);
GO

/* ============================================================================
   7. COMUNICACIONES - com
============================================================================ */

CREATE TABLE com.PLANTILLA
(
    pl_id           INT IDENTITY(1,1) NOT NULL,
    pl_nombre       NVARCHAR(150) NOT NULL,
    pl_canal        NVARCHAR(20) NOT NULL,
    pl_tipo         NVARCHAR(50) NOT NULL,
    pl_asunto       NVARCHAR(250) NULL,
    pl_conte        NVARCHAR(MAX) NOT NULL,
    pl_versi        NVARCHAR(20) NOT NULL CONSTRAINT DF_PLANTILLA_versi DEFAULT ('1.0'),
    pl_fecha_crea   DATETIME2(0) NOT NULL CONSTRAINT DF_PLANTILLA_fecha DEFAULT SYSUTCDATETIME(),
    pl_estatus      BIT NOT NULL CONSTRAINT DF_PLANTILLA_estatus DEFAULT (1),

    CONSTRAINT PK_PLANTILLA PRIMARY KEY (pl_id),
    CONSTRAINT CK_PLANTILLA_canal CHECK (pl_canal IN ('CORREO','WHATSAPP'))
);
GO

CREATE TABLE com.OPCION_COMUNICACION
(
    op_id           BIGINT IDENTITY(1,1) NOT NULL,
    pa_id           BIGINT NOT NULL,
    fi_id           INT NOT NULL,
    co_id           BIGINT NULL,
    op_canal        NVARCHAR(20) NOT NULL,
    op_permi        BIT NOT NULL,
    op_fecha_actu   DATETIME2(0) NOT NULL CONSTRAINT DF_OPCION_fecha DEFAULT SYSUTCDATETIME(),
    op_estatus      BIT NOT NULL CONSTRAINT DF_OPCION_estatus DEFAULT (1),

    CONSTRAINT PK_OPCION_COMUNICACION PRIMARY KEY (op_id),
    CONSTRAINT UQ_OPCION_pac_canal_fin UNIQUE (pa_id, op_canal, fi_id),
    CONSTRAINT FK_OPCION_PACIENTE FOREIGN KEY (pa_id) REFERENCES pac.PACIENTE(pa_id),
    CONSTRAINT FK_OPCION_FINALIDAD FOREIGN KEY (fi_id) REFERENCES pri.FINALIDAD(fi_id),
    CONSTRAINT FK_OPCION_CONSENTIMIENTO FOREIGN KEY (co_id) REFERENCES pri.CONSENTIMIENTO(co_id),
    CONSTRAINT CK_OPCION_canal CHECK (op_canal IN ('CORREO','WHATSAPP'))
);
GO

CREATE TABLE com.NOTIFICACION
(
    no_id           BIGINT IDENTITY(1,1) NOT NULL,
    pa_id           BIGINT NOT NULL,
    ci_id           BIGINT NULL,
    pl_id           INT NOT NULL,
    op_id           BIGINT NULL,
    us_id           BIGINT NULL,
    no_tipo         NVARCHAR(50) NOT NULL,
    no_canal        NVARCHAR(20) NOT NULL,
    no_fecha_prog   DATETIME2(0) NOT NULL,
    no_estad        NVARCHAR(20) NOT NULL CONSTRAINT DF_NOTIFICACION_estad DEFAULT ('PENDIENTE'),
    no_fecha_crea   DATETIME2(0) NOT NULL CONSTRAINT DF_NOTIFICACION_fecha DEFAULT SYSUTCDATETIME(),
    no_version      ROWVERSION NOT NULL,

    CONSTRAINT PK_NOTIFICACION PRIMARY KEY (no_id),
    CONSTRAINT FK_NOTIFICACION_PACIENTE FOREIGN KEY (pa_id) REFERENCES pac.PACIENTE(pa_id),
    CONSTRAINT FK_NOTIFICACION_CITA FOREIGN KEY (ci_id) REFERENCES cit.CITA(ci_id),
    CONSTRAINT FK_NOTIFICACION_PLANTILLA FOREIGN KEY (pl_id) REFERENCES com.PLANTILLA(pl_id),
    CONSTRAINT FK_NOTIFICACION_OPCION FOREIGN KEY (op_id) REFERENCES com.OPCION_COMUNICACION(op_id),
    CONSTRAINT FK_NOTIFICACION_USUARIO FOREIGN KEY (us_id) REFERENCES seg.USUARIO(us_id),
    CONSTRAINT CK_NOTIFICACION_canal CHECK (no_canal IN ('CORREO','WHATSAPP')),
    CONSTRAINT CK_NOTIFICACION_estad CHECK
        (no_estad IN ('PENDIENTE','PROCESANDO','ENVIADA','ERROR','CANCELADA'))
);
GO

CREATE INDEX IX_NOTIFICACION_programadas
ON com.NOTIFICACION(no_estad, no_fecha_prog);
GO

CREATE TABLE com.ENVIO
(
    en_id           BIGINT IDENTITY(1,1) NOT NULL,
    no_id           BIGINT NOT NULL,
    en_num_inten    SMALLINT NOT NULL,
    en_fecha_hora   DATETIME2(0) NOT NULL CONSTRAINT DF_ENVIO_fecha DEFAULT SYSUTCDATETIME(),
    en_resul       NVARCHAR(20) NOT NULL,
    en_id_prove     NVARCHAR(300) NULL,
    en_cod_error    NVARCHAR(100) NULL,
    en_detal_error  NVARCHAR(2000) NULL,

    CONSTRAINT PK_ENVIO PRIMARY KEY (en_id),
    CONSTRAINT FK_ENVIO_NOTIFICACION FOREIGN KEY (no_id) REFERENCES com.NOTIFICACION(no_id),
    CONSTRAINT CK_ENVIO_intento CHECK (en_num_inten > 0),
    CONSTRAINT CK_ENVIO_result CHECK (en_resul IN ('ENVIADO','ERROR'))
);
GO

CREATE UNIQUE INDEX UX_ENVIO_notif_intento
ON com.ENVIO(no_id, en_num_inten);
GO

/* ============================================================================
   8. FINANZAS / PAGOS INTERNOS - fin
============================================================================ */

CREATE TABLE fin.PRESTACION
(
    pr_id           BIGINT IDENTITY(1,1) NOT NULL,
    pa_id           BIGINT NOT NULL,
    ci_id           BIGINT NULL,
    at_id           BIGINT NULL,
    se_id           INT NOT NULL,
    us_id           BIGINT NOT NULL,
    pr_fecha_pres   DATETIME2(0) NOT NULL CONSTRAINT DF_PRESTACION_fecha DEFAULT SYSUTCDATETIME(),
    pr_precio_apli  DECIMAL(12,2) NOT NULL,
    pr_cant         DECIMAL(10,2) NOT NULL CONSTRAINT DF_PRESTACION_cant DEFAULT (1),
    pr_total        AS (CONVERT(DECIMAL(12,2), pr_precio_apli * pr_cant)) PERSISTED,
    pr_estad        NVARCHAR(20) NOT NULL CONSTRAINT DF_PRESTACION_estad DEFAULT ('PENDIENTE'),
    pr_obser        NVARCHAR(1000) NULL,

    CONSTRAINT PK_PRESTACION PRIMARY KEY (pr_id),
    CONSTRAINT FK_PRESTACION_PACIENTE FOREIGN KEY (pa_id) REFERENCES pac.PACIENTE(pa_id),
    CONSTRAINT FK_PRESTACION_CITA FOREIGN KEY (ci_id) REFERENCES cit.CITA(ci_id),
    CONSTRAINT FK_PRESTACION_ATENCION FOREIGN KEY (at_id) REFERENCES cli.ATENCION(at_id),
    CONSTRAINT FK_PRESTACION_SERVICIO FOREIGN KEY (se_id) REFERENCES cit.SERVICIO(se_id),
    CONSTRAINT FK_PRESTACION_USUARIO FOREIGN KEY (us_id) REFERENCES seg.USUARIO(us_id),
    CONSTRAINT CK_PRESTACION_precio CHECK (pr_precio_apli >= 0),
    CONSTRAINT CK_PRESTACION_cant CHECK (pr_cant > 0),
    CONSTRAINT CK_PRESTACION_estad CHECK (pr_estad IN ('PENDIENTE','PAGADA','ANULADA'))
);
GO

CREATE TABLE fin.FORMA_PAGO
(
    fo_id           INT IDENTITY(1,1) NOT NULL,
    fo_codigo       NVARCHAR(30) NOT NULL,
    fo_nombre       NVARCHAR(100) NOT NULL,
    fo_estatus      BIT NOT NULL CONSTRAINT DF_FORMA_PAGO_estatus DEFAULT (1),

    CONSTRAINT PK_FORMA_PAGO PRIMARY KEY (fo_id),
    CONSTRAINT UQ_FORMA_PAGO_codigo UNIQUE (fo_codigo)
);
GO

CREATE TABLE fin.COBRO
(
    co_id           BIGINT IDENTITY(1,1) NOT NULL,
    pr_id           BIGINT NOT NULL,
    fo_id           INT NOT NULL,
    us_id           BIGINT NOT NULL,
    co_fecha_pago   DATETIME2(0) NOT NULL CONSTRAINT DF_COBRO_fecha DEFAULT SYSUTCDATETIME(),
    co_valor        DECIMAL(12,2) NOT NULL,
    co_refe         NVARCHAR(150) NULL,
    co_obser        NVARCHAR(1000) NULL,
    co_estad        NVARCHAR(20) NOT NULL CONSTRAINT DF_COBRO_estad DEFAULT ('REGISTRADO'),

    CONSTRAINT PK_COBRO PRIMARY KEY (co_id),
    CONSTRAINT FK_COBRO_PRESTACION FOREIGN KEY (pr_id) REFERENCES fin.PRESTACION(pr_id),
    CONSTRAINT FK_COBRO_FORMA_PAGO FOREIGN KEY (fo_id) REFERENCES fin.FORMA_PAGO(fo_id),
    CONSTRAINT FK_COBRO_USUARIO FOREIGN KEY (us_id) REFERENCES seg.USUARIO(us_id),
    CONSTRAINT CK_COBRO_valor CHECK (co_valor > 0),
    CONSTRAINT CK_COBRO_estad CHECK (co_estad IN ('REGISTRADO','ANULADO'))
);
GO

CREATE INDEX IX_COBRO_fecha
ON fin.COBRO(co_fecha_pago DESC);
GO

/* ============================================================================
   9. INVENTARIO - inv
============================================================================ */

CREATE TABLE inv.INSUMO
(
    in_id           INT IDENTITY(1,1) NOT NULL,
    in_nombre       NVARCHAR(150) NOT NULL,
    in_categ        NVARCHAR(80) NOT NULL,
    in_unidad       NVARCHAR(30) NOT NULL,
    in_stock_actu   DECIMAL(12,2) NOT NULL CONSTRAINT DF_INSUMO_stock DEFAULT (0),
    in_stock_mini   DECIMAL(12,2) NOT NULL CONSTRAINT DF_INSUMO_stock_mini DEFAULT (0),
    in_fecha_crea   DATETIME2(0) NOT NULL CONSTRAINT DF_INSUMO_fecha DEFAULT SYSUTCDATETIME(),
    in_fecha_modi   DATETIME2(0) NULL,
    in_estatus      BIT NOT NULL CONSTRAINT DF_INSUMO_estatus DEFAULT (1),
    in_version      ROWVERSION NOT NULL,

    CONSTRAINT PK_INSUMO PRIMARY KEY (in_id),
    CONSTRAINT CK_INSUMO_stock CHECK (in_stock_actu >= 0),
    CONSTRAINT CK_INSUMO_stock_mini CHECK (in_stock_mini >= 0)
);
GO

CREATE TABLE inv.MOVIMIENTO
(
    mo_id           BIGINT IDENTITY(1,1) NOT NULL,
    in_id           INT NOT NULL,
    at_id           BIGINT NULL,
    us_id           BIGINT NOT NULL,
    mo_tipo         NVARCHAR(20) NOT NULL,
    mo_cant         DECIMAL(12,2) NOT NULL,
    mo_stock_ante   DECIMAL(12,2) NOT NULL,
    mo_stock_nuev   DECIMAL(12,2) NOT NULL,
    mo_fecha_hora   DATETIME2(0) NOT NULL CONSTRAINT DF_MOVIMIENTO_fecha DEFAULT SYSUTCDATETIME(),
    mo_motivo       NVARCHAR(500) NOT NULL,

    CONSTRAINT PK_MOVIMIENTO PRIMARY KEY (mo_id),
    CONSTRAINT FK_MOVIMIENTO_INSUMO FOREIGN KEY (in_id) REFERENCES inv.INSUMO(in_id),
    CONSTRAINT FK_MOVIMIENTO_ATENCION FOREIGN KEY (at_id) REFERENCES cli.ATENCION(at_id),
    CONSTRAINT FK_MOVIMIENTO_USUARIO FOREIGN KEY (us_id) REFERENCES seg.USUARIO(us_id),
    CONSTRAINT CK_MOVIMIENTO_tipo CHECK
        (mo_tipo IN ('INGRESO','CONSUMO','AJUSTE_POS','AJUSTE_NEG')),
    CONSTRAINT CK_MOVIMIENTO_cant CHECK (mo_cant > 0),
    CONSTRAINT CK_MOVIMIENTO_stock_ante CHECK (mo_stock_ante >= 0),
    CONSTRAINT CK_MOVIMIENTO_stock_nuev CHECK (mo_stock_nuev >= 0)
);
GO

CREATE INDEX IX_MOVIMIENTO_insumo
ON inv.MOVIMIENTO(in_id, mo_fecha_hora DESC);
GO

/* ============================================================================
   10. AUDITORÍA - aud
============================================================================ */

CREATE TABLE aud.EVENTO_AUDITORIA
(
    ev_id           BIGINT IDENTITY(1,1) NOT NULL,
    us_id           BIGINT NULL,
    pa_id           BIGINT NULL,
    ev_fecha_hora   DATETIME2(0) NOT NULL CONSTRAINT DF_EVENTO_AUD_fecha DEFAULT SYSUTCDATETIME(),
    ev_modulo       NVARCHAR(80) NOT NULL,
    ev_accion       NVARCHAR(80) NOT NULL,
    ev_entid        NVARCHAR(128) NULL,
    ev_entid_id     NVARCHAR(100) NULL,
    ev_resul       NVARCHAR(20) NOT NULL,
    ev_direc_ip     NVARCHAR(45) NULL,
    ev_agent_usua   NVARCHAR(500) NULL,
    ev_id_corre     UNIQUEIDENTIFIER NULL,
    ev_metad_json    NVARCHAR(MAX) NULL,

    CONSTRAINT PK_EVENTO_AUDITORIA PRIMARY KEY (ev_id),
    CONSTRAINT FK_EVENTO_AUD_USUARIO FOREIGN KEY (us_id) REFERENCES seg.USUARIO(us_id),
    CONSTRAINT FK_EVENTO_AUD_PACIENTE FOREIGN KEY (pa_id) REFERENCES pac.PACIENTE(pa_id),
    CONSTRAINT CK_EVENTO_AUD_result CHECK (ev_resul IN ('CORRECTO','ERROR','DENEGADO')),
    CONSTRAINT CK_EVENTO_AUD_json CHECK (ev_metad_json IS NULL OR ISJSON(ev_metad_json) = 1)
);
GO

CREATE INDEX IX_EVENTO_AUD_usuario_fecha
ON aud.EVENTO_AUDITORIA(us_id, ev_fecha_hora DESC);
GO

CREATE INDEX IX_EVENTO_AUD_paciente_fecha
ON aud.EVENTO_AUDITORIA(pa_id, ev_fecha_hora DESC)
WHERE pa_id IS NOT NULL;
GO

CREATE OR ALTER TRIGGER aud.TRG_EVENTO_AUDITORIA_NO_MODIFICAR
ON aud.EVENTO_AUDITORIA
INSTEAD OF UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    THROW 51001, 'Los registros de auditoria no pueden modificarse ni eliminarse.', 1;
END;
GO

/* ============================================================================
   11. ANALÍTICA SEUDONIMIZADA - ana
============================================================================ */

CREATE TABLE ana.SEUDONIMO
(
    se_id           UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_SEUDONIMO_id DEFAULT NEWID(),
    pa_id           BIGINT NOT NULL,
    se_fecha_crea   DATETIME2(0) NOT NULL CONSTRAINT DF_SEUDONIMO_fecha DEFAULT SYSUTCDATETIME(),
    se_estatus      BIT NOT NULL CONSTRAINT DF_SEUDONIMO_estatus DEFAULT (1),

    CONSTRAINT PK_SEUDONIMO PRIMARY KEY (se_id),
    CONSTRAINT UQ_SEUDONIMO_paciente UNIQUE (pa_id),
    CONSTRAINT FK_SEUDONIMO_PACIENTE FOREIGN KEY (pa_id) REFERENCES pac.PACIENTE(pa_id)
);
GO

CREATE TABLE ana.EJECUCION
(
    ej_id           BIGINT IDENTITY(1,1) NOT NULL,
    us_id           BIGINT NOT NULL,
    en_id           INT NULL,
    fi_id           INT NOT NULL,
    ej_fecha_hora   DATETIME2(0) NOT NULL CONSTRAINT DF_EJECUCION_fecha DEFAULT SYSUTCDATETIME(),
    ej_estad        NVARCHAR(20) NOT NULL CONSTRAINT DF_EJECUCION_estad DEFAULT ('PENDIENTE'),
    ej_cant_reg     INT NOT NULL CONSTRAINT DF_EJECUCION_cant DEFAULT (0),
    ej_huell_entr    NVARCHAR(128) NULL,
    ej_huell_sali    NVARCHAR(128) NULL,
    ej_obser        NVARCHAR(1500) NULL,

    CONSTRAINT PK_EJECUCION PRIMARY KEY (ej_id),
    CONSTRAINT FK_EJECUCION_USUARIO FOREIGN KEY (us_id) REFERENCES seg.USUARIO(us_id),
    CONSTRAINT FK_EJECUCION_ENCARGADO FOREIGN KEY (en_id) REFERENCES pri.ENCARGADO(en_id),
    CONSTRAINT FK_EJECUCION_FINALIDAD FOREIGN KEY (fi_id) REFERENCES pri.FINALIDAD(fi_id),
    CONSTRAINT CK_EJECUCION_cant CHECK (ej_cant_reg >= 0),
    CONSTRAINT CK_EJECUCION_estad CHECK (ej_estad IN ('PENDIENTE','PROCESANDO','COMPLETADA','ERROR'))
);
GO

CREATE TABLE ana.DATO_ANALITICO
(
    da_id           BIGINT IDENTITY(1,1) NOT NULL,
    ej_id           BIGINT NOT NULL,
    se_id           UNIQUEIDENTIFIER NOT NULL,
    da_frec_aten    INT NOT NULL,
    da_categ_serv   NVARCHAR(100) NULL,
    da_fecha_ulti   DATE NULL,
    da_dias_inact   INT NULL,

    CONSTRAINT PK_DATO_ANALITICO PRIMARY KEY (da_id),
    CONSTRAINT FK_DATO_ANALITICO_EJECUCION FOREIGN KEY (ej_id) REFERENCES ana.EJECUCION(ej_id),
    CONSTRAINT FK_DATO_ANALITICO_SEUDONIMO FOREIGN KEY (se_id) REFERENCES ana.SEUDONIMO(se_id),
    CONSTRAINT CK_DATO_ANALITICO_frec CHECK (da_frec_aten >= 0),
    CONSTRAINT CK_DATO_ANALITICO_dias CHECK (da_dias_inact IS NULL OR da_dias_inact >= 0)
);
GO

CREATE UNIQUE INDEX UX_DATO_ANALITICO_ejec_seud
ON ana.DATO_ANALITICO(ej_id, se_id);
GO

CREATE TABLE ana.RECOMENDACION
(
    re_id           BIGINT IDENTITY(1,1) NOT NULL,
    ej_id           BIGINT NOT NULL,
    re_segme      NVARCHAR(150) NOT NULL,
    re_hallaz       NVARCHAR(2000) NOT NULL,
    re_sugere       NVARCHAR(2000) NOT NULL,
    re_pacie_estim  INT NOT NULL,
    re_nivel_priva  NVARCHAR(30) NOT NULL CONSTRAINT DF_RECOMENDACION_priva DEFAULT ('SEUDONIMIZADO'),
    re_fecha_gene   DATETIME2(0) NOT NULL CONSTRAINT DF_RECOMENDACION_fecha DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_RECOMENDACION PRIMARY KEY (re_id),
    CONSTRAINT FK_RECOMENDACION_EJECUCION FOREIGN KEY (ej_id) REFERENCES ana.EJECUCION(ej_id),
    CONSTRAINT CK_RECOMENDACION_pacie CHECK (re_pacie_estim >= 0),
    CONSTRAINT CK_RECOMENDACION_priva CHECK
        (re_nivel_priva IN ('AGREGADO','SEUDONIMIZADO'))
);
GO

/* ============================================================================
   12. VISTAS DE REPORTES / DASHBOARD - rep
============================================================================ */

CREATE OR ALTER VIEW rep.VW_DASH_CITAS
AS
SELECT
    CAST(ci_fecha_ini AS DATE) AS fecha,
    COUNT_BIG(*) AS total_citas,
    SUM(CASE WHEN ci_estad = 'ATENDIDA' THEN 1 ELSE 0 END) AS citas_atendidas,
    SUM(CASE WHEN ci_estad = 'CANCELADA' THEN 1 ELSE 0 END) AS citas_canceladas,
    SUM(CASE WHEN ci_estad = 'NO_ASISTE' THEN 1 ELSE 0 END) AS citas_no_asiste
FROM cit.CITA
GROUP BY CAST(ci_fecha_ini AS DATE);
GO

CREATE OR ALTER VIEW rep.VW_DASH_PACIENTES
AS
SELECT
    COUNT_BIG(*) AS total_pacientes,
    SUM(CASE WHEN pa_estatus = 1 THEN 1 ELSE 0 END) AS pacientes_activos,
    SUM(CASE WHEN pa_estatus = 0 THEN 1 ELSE 0 END) AS pacientes_inactivos
FROM pac.PACIENTE;
GO

CREATE OR ALTER VIEW rep.VW_DASH_INGRESOS
AS
SELECT
    CAST(co_fecha_pago AS DATE) AS fecha,
    COUNT_BIG(*) AS total_cobros,
    SUM(CASE WHEN co_estad = 'REGISTRADO' THEN co_valor ELSE 0 END) AS total_ingresos
FROM fin.COBRO
GROUP BY CAST(co_fecha_pago AS DATE);
GO

CREATE OR ALTER VIEW rep.VW_DASH_INVENTARIO
AS
SELECT
    in_id,
    in_nombre,
    in_categ,
    in_unidad,
    in_stock_actu,
    in_stock_mini,
    CASE WHEN in_stock_actu <= in_stock_mini THEN 1 ELSE 0 END AS stock_bajo
FROM inv.INSUMO
WHERE in_estatus = 1;
GO

CREATE OR ALTER VIEW rep.VW_SERVICIOS_FRECUENTES
AS
SELECT
    s.se_id,
    s.se_nombre,
    COUNT_BIG(*) AS total_prestaciones,
    SUM(p.pr_total) AS total_generado
FROM fin.PRESTACION p
INNER JOIN cit.SERVICIO s ON s.se_id = p.se_id
WHERE p.pr_estad <> 'ANULADA'
GROUP BY s.se_id, s.se_nombre;
GO

/* ============================================================================
   13. DATOS BASE / CATÁLOGOS
============================================================================ */

IF NOT EXISTS (SELECT 1 FROM seg.ROL WHERE ro_codigo = 'ADMIN_PODOLOGA')
BEGIN
    INSERT INTO seg.ROL (ro_codigo, ro_nombre, ro_descri)
    VALUES ('ADMIN_PODOLOGA', 'Podologa / Administradora',
            'Gestion clinica, administrativa, seguridad, auditoria y cumplimiento.');
END;

IF NOT EXISTS (SELECT 1 FROM seg.ROL WHERE ro_codigo = 'PACIENTE')
BEGIN
    INSERT INTO seg.ROL (ro_codigo, ro_nombre, ro_descri)
    VALUES ('PACIENTE', 'Paciente',
            'Acceso limitado a funcionalidades autorizadas para pacientes.');
END;
GO

INSERT INTO seg.PERMISO(pe_codigo, pe_nombre, pe_modulo, pe_descri)
SELECT v.pe_codigo, v.pe_nombre, v.pe_modulo, v.pe_descri
FROM (VALUES
    ('PACIENTE_VER',       'Consultar pacientes',              'PACIENTES',      'Consultar informacion autorizada de pacientes'),
    ('PACIENTE_CREAR',     'Registrar pacientes',              'PACIENTES',      'Crear nuevos pacientes'),
    ('PACIENTE_EDITAR',    'Actualizar pacientes',             'PACIENTES',      'Actualizar informacion de pacientes'),
    ('CITA_VER',           'Consultar citas',                  'CITAS',          'Consultar agenda y citas'),
    ('CITA_CREAR',         'Crear citas',                      'CITAS',          'Registrar nuevas citas'),
    ('CITA_EDITAR',        'Modificar citas',                  'CITAS',          'Reprogramar o modificar citas'),
    ('CITA_CANCELAR',      'Cancelar citas',                   'CITAS',          'Cancelar citas'),
    ('CLINICA_VER',        'Consultar historia clinica',       'CLINICA',        'Consultar historial clinico autorizado'),
    ('CLINICA_EDITAR',     'Registrar atencion clinica',       'CLINICA',        'Crear atenciones, evoluciones y adjuntos'),
    ('AUDITORIA_VER',      'Consultar auditoria',              'AUDITORIA',      'Consultar bitacora de auditoria'),
    ('COMUNICACION_GEST',  'Gestionar comunicaciones',         'COMUNICACION',   'Programar y revisar notificaciones'),
    ('PRIVACIDAD_GEST',    'Gestionar privacidad',             'PRIVACIDAD',     'Gestionar consentimientos y derechos'),
    ('PAGO_GEST',          'Gestionar pagos internos',         'FINANZAS',       'Registrar prestaciones y cobros'),
    ('INVENTARIO_GEST',    'Gestionar inventario',             'INVENTARIO',     'Registrar items y movimientos'),
    ('ANALITICA_VER',      'Consultar analitica',              'ANALITICA',      'Ejecutar y consultar analitica seudonimizada')
) v(pe_codigo, pe_nombre, pe_modulo, pe_descri)
WHERE NOT EXISTS
(
    SELECT 1
    FROM seg.PERMISO p
    WHERE p.pe_codigo = v.pe_codigo
);
GO

/* Administrador recibe todos los permisos */
INSERT INTO seg.RO_PE(ro_id, pe_id)
SELECT r.ro_id, p.pe_id
FROM seg.ROL r
CROSS JOIN seg.PERMISO p
WHERE r.ro_codigo = 'ADMIN_PODOLOGA'
AND NOT EXISTS
(
    SELECT 1
    FROM seg.RO_PE rp
    WHERE rp.ro_id = r.ro_id
      AND rp.pe_id = p.pe_id
);
GO

/* Paciente: permisos mínimos */
INSERT INTO seg.RO_PE(ro_id, pe_id)
SELECT r.ro_id, p.pe_id
FROM seg.ROL r
INNER JOIN seg.PERMISO p
    ON p.pe_codigo IN ('CITA_VER','CITA_CREAR')
WHERE r.ro_codigo = 'PACIENTE'
AND NOT EXISTS
(
    SELECT 1
    FROM seg.RO_PE rp
    WHERE rp.ro_id = r.ro_id
      AND rp.pe_id = p.pe_id
);
GO

INSERT INTO pri.FINALIDAD(fi_codigo, fi_nombre, fi_descri, fi_requi_cons)
SELECT v.fi_codigo, v.fi_nombre, v.fi_descri, v.fi_requi_cons
FROM (VALUES
    ('ATENCION_CLINICA', 'Atencion clinica',
     'Tratamiento de datos necesarios para brindar atencion podologica.', CAST(0 AS BIT)),
    ('GESTION_CITAS', 'Gestion de citas',
     'Tratamiento de datos para solicitud, confirmacion y gestion de citas.', CAST(0 AS BIT)),
    ('RECORDATORIOS', 'Recordatorios de citas',
     'Envio de recordatorios y confirmaciones operativas.', CAST(1 AS BIT)),
    ('FIDELIZACION', 'Fidelizacion',
     'Comunicaciones no esenciales orientadas al seguimiento y fidelizacion.', CAST(1 AS BIT)),
    ('ANALITICA', 'Analitica seudonimizada',
     'Uso de datos minimizados y seudonimizados para analitica administrativa.', CAST(1 AS BIT))
) v(fi_codigo, fi_nombre, fi_descri, fi_requi_cons)
WHERE NOT EXISTS
(
    SELECT 1
    FROM pri.FINALIDAD f
    WHERE f.fi_codigo = v.fi_codigo
);
GO

INSERT INTO fin.FORMA_PAGO(fo_codigo, fo_nombre)
SELECT v.fo_codigo, v.fo_nombre
FROM (VALUES
    ('EFECTIVO', 'Efectivo'),
    ('TRANSFERENCIA', 'Transferencia'),
    ('TARJETA', 'Tarjeta - solo referencia interna, sin almacenar datos sensibles'),
    ('OTRO', 'Otro')
) v(fo_codigo, fo_nombre)
WHERE NOT EXISTS
(
    SELECT 1
    FROM fin.FORMA_PAGO f
    WHERE f.fo_codigo = v.fo_codigo
);
GO

INSERT INTO pri.CATEGORIA_DATO(ca_nombre, ca_descri, ca_sensi)
SELECT v.ca_nombre, v.ca_descri, v.ca_sensi
FROM (VALUES
    ('Identificacion', 'Cedula, pasaporte u otro identificador del titular.', CAST(0 AS BIT)),
    ('Contacto', 'Correo electronico, celular, telefono o WhatsApp.', CAST(0 AS BIT)),
    ('Datos demograficos', 'Fecha de nacimiento, genero y direccion.', CAST(0 AS BIT)),
    ('Datos de salud', 'Antecedentes, valoraciones, diagnosticos, tratamientos y evoluciones.', CAST(1 AS BIT)),
    ('Datos administrativos', 'Citas, servicios prestados e informacion operativa.', CAST(0 AS BIT)),
    ('Datos de pagos internos', 'Valores, metodo y referencia interna de pagos.', CAST(0 AS BIT)),
    ('Datos seudonimizados', 'Identificadores seudonimos y metricas minimizadas para analitica.', CAST(1 AS BIT))
) v(ca_nombre, ca_descri, ca_sensi)
WHERE NOT EXISTS
(
    SELECT 1
    FROM pri.CATEGORIA_DATO c
    WHERE c.ca_nombre = v.ca_nombre
);
GO

/* ============================================================================
   14. VALIDACIÓN RÁPIDA
============================================================================ */

SELECT
    s.name AS esquema,
    t.name AS tabla
FROM sys.tables t
INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE s.name IN ('seg','pac','cit','cli','com','fin','inv','pri','aud','ana')
ORDER BY s.name, t.name;
GO
