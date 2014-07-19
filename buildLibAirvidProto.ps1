$ErrorActionPreference = "Stop"
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Set-Location $scriptPath

. .\buildlib.ps1

DoLibBuild -architech debug -projPath ".\libairvidproto\libairvidproto.csproj"
DoLibBuild -architech release -projPath ".\libairvidproto\libairvidproto.csproj"