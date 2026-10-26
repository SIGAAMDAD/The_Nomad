[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",
    [ValidateRange(1, 128)]
    [int]$Jobs = [Math]::Min([Environment]::ProcessorCount, 8),
    [ValidateSet("msvc", "msvc2022", "linux")]
    [string]$Toolchain = "msvc",
    [switch]$ConfigureOnly,
    [switch]$Clean
)

$ErrorActionPreference = "Stop"
$Preset = "$Toolchain-$($Configuration.ToLowerInvariant())"

if (-not (Get-Command "cmake" -ErrorAction SilentlyContinue)) {
    Write-Error "cmake was not found. Install CMake 3.24 or newer and add it to PATH."
    exit 1
}

Push-Location $PSScriptRoot
try {
    & cmake --preset $Preset
    if ($LASTEXITCODE -ne 0) {
        throw "CMake configuration failed ($LASTEXITCODE)."
    }

    if ($ConfigureOnly) { return }
    if ($Clean) {
        & cmake --build --preset $Preset --target clean
    } else {
        & cmake --build --preset $Preset --parallel $Jobs
    }
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed ($LASTEXITCODE)."
    }
} catch {
    [Console]::Error.WriteLine($_)
    exit 1
} finally {
    Pop-Location
}
