SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() <> N'PODOFLOW_PRUEBAS'
BEGIN
    RAISERROR(N'Este script solo se ejecuta en PODOFLOW_PRUEBAS.', 16, 1);
    RETURN;
END;

BEGIN TRANSACTION;

IF NOT EXISTS (SELECT 1 FROM seg.USUARIO WHERE us_id = 900001)
BEGIN
    SET IDENTITY_INSERT seg.USUARIO ON;
    INSERT INTO seg.USUARIO (us_id, us_nombre, us_correo, us_fecha_crea, us_estatus)
    VALUES
        (900001, N'Administradora de Pruebas', N'admin@pruebas.podoflow.test', '2026-01-01T12:00:00', 1),
        (900002, N'Usuario Inactivo de Pruebas', N'inactivo@pruebas.podoflow.test', '2026-01-01T12:00:00', 0);
    SET IDENTITY_INSERT seg.USUARIO OFF;
END;

IF NOT EXISTS (SELECT 1 FROM seg.US_RO WHERE us_id = 900001)
    INSERT INTO seg.US_RO (us_id, ro_id, ur_fecha_asig, ur_estatus)
    SELECT 900001, ro_id, '2026-01-01T12:00:00', 1
    FROM seg.ROL
    WHERE ro_codigo = N'ADMIN_PODOLOGA';

IF NOT EXISTS (SELECT 1 FROM pac.PACIENTE WHERE pa_id = 900001)
BEGIN
    SET IDENTITY_INSERT pac.PACIENTE ON;
    INSERT INTO pac.PACIENTE (pa_id, pa_tipo_ident, pa_ident, pa_nombres, pa_apelli, pa_fecha_naci, pa_genero, pa_direc, pa_fecha_crea, pa_estatus)
    VALUES
        (900001, N'CEDULA', N'1710034065', N'Ana', N'Adulta Pruebas', '1990-05-10', N'FEMENINO', N'Quito', '2026-01-01T12:00:00', 1),
        (900002, N'CEDULA', N'0923456784', N'Mateo', N'Menor Pruebas', '2016-03-15', N'MASCULINO', N'Quito', '2026-01-01T12:00:00', 1),
        (900003, N'PASAPORTE', N'PX123456', N'Rosa', N'Inactiva Pruebas', '1985-08-20', N'FEMENINO', N'Quito', '2026-01-01T12:00:00', 0);
    SET IDENTITY_INSERT pac.PACIENTE OFF;
END;

IF NOT EXISTS (SELECT 1 FROM pac.DATO_CONTACTO WHERE da_id = 900001)
BEGIN
    SET IDENTITY_INSERT pac.DATO_CONTACTO ON;
    INSERT INTO pac.DATO_CONTACTO (da_id, pa_id, da_tipo, da_valor, da_princ, da_verif, da_fecha_crea, da_estatus)
    VALUES
        (900001, 900001, N'CORREO', N'ana.adulta@pruebas.podoflow.test', 1, 1, '2026-01-01T12:00:00', 1),
        (900002, 900001, N'WHATSAPP', N'0991234567', 1, 1, '2026-01-01T12:00:00', 1);
    SET IDENTITY_INSERT pac.DATO_CONTACTO OFF;
END;

IF NOT EXISTS (SELECT 1 FROM cit.SERVICIO WHERE se_id = 900001)
BEGIN
    SET IDENTITY_INSERT cit.SERVICIO ON;
    INSERT INTO cit.SERVICIO (se_id, se_nombre, se_descri, se_precio, se_durac_min, se_fecha_crea, se_estatus)
    VALUES (900001, N'Consulta podológica de pruebas', N'Servicio sintético para las pruebas automatizadas.', 25.00, 30, '2026-01-01T12:00:00', 1);
    SET IDENTITY_INSERT cit.SERVICIO OFF;
END;

IF NOT EXISTS (SELECT 1 FROM cit.CITA WHERE ci_id = 900001)
BEGIN
    SET IDENTITY_INSERT cit.CITA ON;
    INSERT INTO cit.CITA (ci_id, pa_id, se_id, us_id, ci_fecha_ini, ci_fecha_fin, ci_estad, ci_canal, ci_obser, ci_fecha_crea)
    VALUES
        (900001, 900001, 900001, 900001, '2030-01-15T15:00:00', '2030-01-15T15:30:00', N'SOLICITADA', N'SISTEMA', N'Cita futura de pruebas', '2026-01-01T12:00:00'),
        (900002, 900001, 900001, 900001, '2025-01-10T15:00:00', '2025-01-10T15:30:00', N'ATENDIDA', N'SISTEMA', N'Cita pasada de pruebas', '2025-01-01T12:00:00');
    SET IDENTITY_INSERT cit.CITA OFF;
END;

IF NOT EXISTS (SELECT 1 FROM pri.AVISO WHERE av_id = 900001)
BEGIN
    SET IDENTITY_INSERT pri.AVISO ON;
    INSERT INTO pri.AVISO (av_id, av_versi, av_fecha_desd, av_fecha_hast, av_ruta_arch, av_huella, av_estatus)
    VALUES (900001, N'PRUEBAS-1.0', '2020-01-01', NULL, N'avisos/aviso-pruebas.pdf', NULL, 1);
    SET IDENTITY_INSERT pri.AVISO OFF;
END;

IF NOT EXISTS (SELECT 1 FROM com.PLANTILLA WHERE pl_id = 900001)
BEGIN
    SET IDENTITY_INSERT com.PLANTILLA ON;
    INSERT INTO com.PLANTILLA (pl_id, pl_nombre, pl_canal, pl_tipo, pl_asunto, pl_conte, pl_versi, pl_fecha_crea, pl_estatus)
    VALUES
        (900001, N'Recordatorio de cita por correo (pruebas)', N'CORREO', N'RECORDATORIO_CITA', N'Recordatorio de su cita', N'<p>Hola {{1}}, le recordamos su cita del {{2}} a las {{3}}.</p>', N'1.0', '2026-01-01T12:00:00', 1),
        (900002, N'recordatorio_cita_pruebas', N'WHATSAPP', N'RECORDATORIO_CITA', NULL, N'Hola {{1}}, su cita es el {{2}} a las {{3}}.', N'1.0', '2026-01-01T12:00:00', 1);
    SET IDENTITY_INSERT com.PLANTILLA OFF;
END;

IF NOT EXISTS (SELECT 1 FROM pri.CONSENTIMIENTO WHERE co_id = 900001)
BEGIN
    SET IDENTITY_INSERT pri.CONSENTIMIENTO ON;
    INSERT INTO pri.CONSENTIMIENTO (co_id, pa_id, fi_id, av_id, co_estad, co_medio, co_fecha_obte, co_fecha_revo, co_obser)
    SELECT 900001, 900001, fi_id, 900001, N'ACEPTADO', N'PRESENCIAL', '2026-01-01T12:00:00', NULL, N'Autorizado por el titular'
    FROM pri.FINALIDAD
    WHERE fi_codigo = N'FIDELIZACION';
    SET IDENTITY_INSERT pri.CONSENTIMIENTO OFF;
END;

COMMIT TRANSACTION;

SELECT N'usuarios' AS dato, COUNT(*) AS filas FROM seg.USUARIO WHERE us_id >= 900001
UNION ALL SELECT N'pacientes', COUNT(*) FROM pac.PACIENTE WHERE pa_id >= 900001
UNION ALL SELECT N'servicios', COUNT(*) FROM cit.SERVICIO WHERE se_id >= 900001
UNION ALL SELECT N'citas', COUNT(*) FROM cit.CITA WHERE ci_id >= 900001
UNION ALL SELECT N'avisos', COUNT(*) FROM pri.AVISO WHERE av_id >= 900001
UNION ALL SELECT N'plantillas', COUNT(*) FROM com.PLANTILLA WHERE pl_id >= 900001
UNION ALL SELECT N'consentimientos', COUNT(*) FROM pri.CONSENTIMIENTO WHERE co_id >= 900001;
