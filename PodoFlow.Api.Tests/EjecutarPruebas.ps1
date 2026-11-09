param(
    [string]$Informe = 'C:\Claude\Informe_Pruebas_PodoFlow'
)

$ErrorActionPreference = 'Continue'
$proyecto = $PSScriptRoot
$raiz = Split-Path $proyecto -Parent
$resultados = Join-Path $proyecto 'TestResults'

if (Test-Path $resultados) {
    Remove-Item $resultados -Recurse -Force
}

Push-Location $raiz
try {
    dotnet tool restore | Out-Null

    dotnet test (Join-Path $proyecto 'PodoFlow.Api.Tests.csproj') `
        --collect:"XPlat Code Coverage" `
        --results-directory $resultados `
        --logger "html;LogFileName=resultados-pruebas.html" `
        --logger "trx;LogFileName=resultados-pruebas.trx" `
        --logger "console;verbosity=normal"
    $codigoPruebas = $LASTEXITCODE

    $cobertura = Get-ChildItem $resultados -Recurse -Filter 'coverage.cobertura.xml' |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1

    if ($null -eq $cobertura) {
        throw 'No se generó el archivo de cobertura.'
    }

    dotnet reportgenerator `
        "-reports:$($cobertura.FullName)" `
        "-targetdir:$(Join-Path $resultados 'cobertura')" `
        "-reporttypes:Html;TextSummary;JsonSummary" `
        "-classfilters:+PodoFlow.Api.Servicios.*" `
        "-title:PodoFlow - Cobertura de Servicios"

    if ($Informe) {
        New-Item -ItemType Directory -Force -Path $Informe | Out-Null
        $destinoCobertura = Join-Path $Informe 'cobertura'

        if (Test-Path $destinoCobertura) {
            Remove-Item $destinoCobertura -Recurse -Force
        }

        Copy-Item (Join-Path $resultados 'cobertura') $destinoCobertura -Recurse
        Copy-Item (Get-ChildItem $resultados -Recurse -Filter 'resultados-pruebas.html' | Select-Object -First 1).FullName (Join-Path $Informe 'resultados-pruebas.html') -Force
        Copy-Item (Get-ChildItem $resultados -Recurse -Filter 'resultados-pruebas.trx' | Select-Object -First 1).FullName (Join-Path $Informe 'resultados-pruebas.trx') -Force
    }

    Get-Content (Join-Path $resultados 'cobertura\Summary.txt') -TotalCount 12
    exit $codigoPruebas
}
finally {
    Pop-Location
}
