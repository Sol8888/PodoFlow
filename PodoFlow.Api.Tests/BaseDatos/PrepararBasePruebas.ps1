param(
    [string]$ScriptOficial = (Join-Path $PSScriptRoot 'PodoFlow_BD_Completa.sql'),
    [string]$Servidor = 'localhost\SQLEXPRESS',
    [string]$Usuario,
    [switch]$Recrear
)

$ErrorActionPreference = 'Stop'
$baseDatos = 'PODOFLOW_PRUEBAS'

if ($Usuario) {
    if (-not $env:SQLCMDPASSWORD) {
        throw 'Con -Usuario la contraseña debe estar en la variable de entorno SQLCMDPASSWORD.'
    }
    $autenticacion = @('-U', $Usuario)
}
else {
    $autenticacion = @('-E')
}

if (-not (Test-Path $ScriptOficial)) {
    throw "No se encontró el script oficial: $ScriptOficial"
}

if (-not (Get-Command sqlcmd -ErrorAction SilentlyContinue)) {
    throw 'No se encontró sqlcmd en el PATH.'
}

$temporal = Join-Path ([System.IO.Path]::GetTempPath()) ("podoflow_esquema_" + [guid]::NewGuid().ToString('N') + '.sql')
$lineas = Get-Content -Path $ScriptOficial -Encoding UTF8
$inicio = [Array]::FindIndex($lineas, [Predicate[string]] { param($l) $l.Trim() -eq 'SET NOCOUNT ON;' })
if ($inicio -lt 0) {
    throw 'El script oficial no tiene la línea SET NOCOUNT ON; esperada.'
}

$fin = [Array]::FindIndex($lineas, $inicio, [Predicate[string]] { param($l) $l -match '^\s*USE\s+' })

if ($fin -lt 0) {
    $fin = $lineas.Count
}

$esquema = $lineas[$inicio..($fin - 1)]

if ($esquema | Where-Object { $_ -match '^\s*(USE\s+|CREATE\s+DATABASE|DROP\s+DATABASE|ALTER\s+DATABASE)' }) {
    throw 'El esquema filtrado todavía contiene USE o instrucciones sobre bases de datos.'
}

$utf8 = New-Object System.Text.UTF8Encoding($true)
[System.IO.File]::WriteAllLines($temporal, [string[]]$esquema, $utf8)

try {
    if ($Recrear) {
        sqlcmd -S $Servidor @autenticacion -C -b -Q "IF DB_ID(N'$baseDatos') IS NOT NULL BEGIN ALTER DATABASE [$baseDatos] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$baseDatos]; END"
        if ($LASTEXITCODE -ne 0) { throw 'No se pudo eliminar la base de pruebas.' }
    }

    sqlcmd -S $Servidor @autenticacion -C -b -Q "IF DB_ID(N'$baseDatos') IS NULL CREATE DATABASE [$baseDatos];"
    if ($LASTEXITCODE -ne 0) { throw 'No se pudo crear la base de pruebas.' }

    $tablas = sqlcmd -S $Servidor @autenticacion -C -b -d $baseDatos -h -1 -W -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.tables;"
    if ([int]($tablas | Select-Object -First 1) -eq 0) {
        sqlcmd -S $Servidor @autenticacion -C -b -I -d $baseDatos -i $temporal
        if ($LASTEXITCODE -ne 0) { throw 'Falló la creación del esquema en la base de pruebas.' }
    }

    $datos = Join-Path $PSScriptRoot 'DatosSinteticos.sql'
    sqlcmd -S $Servidor @autenticacion -C -b -I -d $baseDatos -i $datos
    if ($LASTEXITCODE -ne 0) { throw 'Falló la carga de datos sintéticos.' }

    Write-Host "Base $baseDatos lista en $Servidor."
}
finally {
    Remove-Item $temporal -ErrorAction SilentlyContinue
}
