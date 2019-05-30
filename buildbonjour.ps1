$ErrorActionPreference = "Stop"
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Set-Location $scriptPath

. .\buildlib.ps1

DoLibBuild -architech debug -projPath ".\Bonjour.NET\Bonjour.NET.csproj"
DoLibBuild -architech release -projPath ".\Bonjour.NET\Bonjour.NET.csproj"