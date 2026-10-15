param(
    [Parameter(Mandatory)][string]$Manifest,
    [Parameter(Mandatory)][string]$Deployment,
    [switch]$Apply,
    [string]$PreviousManifest
)
$ErrorActionPreference = 'Stop'
# Compile outside the web project so import tooling is never a public endpoint.
$toolDirectory = Join-Path ([IO.Path]::GetTempPath()) ('mynotes-import-' + [Guid]::NewGuid().ToString('N'))
[IO.Directory]::CreateDirectory($toolDirectory) | Out-Null
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'samsung_notes_import.cs.template') -Destination (Join-Path $toolDirectory 'Program.cs')
@'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net9.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup>
  <ItemGroup><PackageReference Include="Microsoft.Data.SqlClient" Version="5.1.6" /></ItemGroup>
</Project>
'@ | Set-Content -LiteralPath (Join-Path $toolDirectory 'Import.csproj') -Encoding utf8
$mode = if ($Apply) { 'apply' } else { 'verify' }
$arguments = @([IO.Path]::GetFullPath($Manifest), [IO.Path]::GetFullPath($Deployment), $mode)
if ($PreviousManifest) {
    if (!$Apply) { throw 'Repair requires -Apply and an archived previous manifest.' }
    $arguments[2] = 'repair'
    $arguments += [IO.Path]::GetFullPath($PreviousManifest)
}
& dotnet run --project (Join-Path $toolDirectory 'Import.csproj') -- @arguments
if ($LASTEXITCODE -ne 0) { throw 'Import failed. Review output and archived progress; no automatic cleanup or deletion is performed.' }
