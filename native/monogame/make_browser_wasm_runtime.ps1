param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$preset = switch ($Configuration)
{
    "Debug" { "browser-wasm-debug" }
    "Release" { "browser-wasm-release" }
    default { throw "Unsupported configuration '$Configuration'." }
}

$browserWasmRoot = (Resolve-Path (Join-Path $PSScriptRoot "browser-wasm")).Path

Push-Location $browserWasmRoot
try
{
    & cmake --preset $preset
    if ($LASTEXITCODE -ne 0)
    {
        throw "cmake failed with exit code $LASTEXITCODE."
    }

    & cmake --build --preset $preset
    if ($LASTEXITCODE -ne 0)
    {
        throw "cmake failed with exit code $LASTEXITCODE."
    }
}
finally
{
    Pop-Location
}
