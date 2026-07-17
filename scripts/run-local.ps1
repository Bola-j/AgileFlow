[CmdletBinding()]
param(
    [switch]$SkipInstall
)

$ErrorActionPreference = 'Stop'

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$smtpContainerName = 'agileflow-smtp4dev'
$smtpImage = 'rnwood/smtp4dev'

function Require-Command {
    param([Parameter(Mandatory)][string]$Name)

    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "'$Name' is required but was not found on PATH."
    }
}

function Start-DeveloperConsole {
    param(
        [Parameter(Mandatory)][string]$Title,
        [Parameter(Mandatory)][string]$Command
    )

    $escapedRoot = $repositoryRoot.Replace("'", "''")
    $windowCommand = "`$Host.UI.RawUI.WindowTitle = '$Title'; Set-Location -LiteralPath '$escapedRoot'; $Command"
    Start-Process -FilePath 'powershell.exe' -ArgumentList @(
        '-NoExit',
        '-NoProfile',
        '-ExecutionPolicy', 'Bypass',
        '-Command', $windowCommand
    ) | Out-Null
}

Require-Command docker
Require-Command dotnet
Require-Command npm

if (-not (docker info 2>$null)) {
    throw 'Docker Desktop is not running. Start it, then run this script again.'
}

$smtpStatus = docker ps -aq --filter "name=^/$smtpContainerName$"
if ($smtpStatus) {
    $isRunning = docker ps -q --filter "name=^/$smtpContainerName$"
    if (-not $isRunning) {
        docker start $smtpContainerName | Out-Null
    }
}
else {
    docker run --detach --rm --name $smtpContainerName -p 3000:80 -p 2525:25 $smtpImage | Out-Null
}

if (-not $SkipInstall -and -not (Test-Path (Join-Path $repositoryRoot 'frontend/node_modules'))) {
    Write-Host 'Installing frontend dependencies...'
    Push-Location (Join-Path $repositoryRoot 'frontend')
    try {
        npm ci
    }
    finally {
        Pop-Location
    }
}

Start-DeveloperConsole -Title 'AgileFlow API' -Command 'dotnet run --project backend/API/API.csproj --launch-profile http'
Start-DeveloperConsole -Title 'AgileFlow Frontend' -Command 'npm --prefix frontend run dev'

Write-Host 'AgileFlow services started:'
Write-Host '  API:      http://localhost:6358/swagger'
Write-Host '  Frontend: http://127.0.0.1:5173'
Write-Host '  SMTP UI:  http://localhost:3000'
Write-Host ''
Write-Host "Stop SMTP with: docker stop $smtpContainerName"
