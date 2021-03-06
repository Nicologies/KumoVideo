function DoLibBuild([string]$projPath, [string]$architech)
{
    write-host building $architech version of $projPath -ForegroundColor DarkGreen
    $config = "/p:Configuration=" + $architech;
    $collectionOfArgs = @($projPath, "/t:Clean", "/t:Build", "/fileLogger", "/noconsolelogger", "/verbosity:minimal", $config)
    
    msbuild $collectionOfArgs | Out-Null
    
    if($LastExitCode -ne 0)
    {
        Write-Error "Failed to build $architech version of $projPath"
        return;
    }
    write-host sucessfully built $architech version  of $projPath -ForegroundColor DarkGreen
}