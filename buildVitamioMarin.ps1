$ErrorActionPreference = "Stop"
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Set-Location $scriptPath

. .\buildlib.ps1

DoLibBuild -architech debug -projPath ".\VitamitoMarin\VitamitoMarin.csproj"
DoLibBuild -architech release -projPath ".\VitamitoMarin\VitamitoMarin.csproj"