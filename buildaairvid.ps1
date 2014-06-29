$ErrorActionPreference = "Stop"
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Set-Location $scriptPath

. .\IncreaseApkVer.ps1
. .\buildlib.ps1

$manifestPath = ".\aairvid\Properties\AndroidManifest.xml"

function DoApkBuild ([string]$architech, [bool]$isFreeVer = $True)
{
    write-host building $architech version of APK -ForegroundColor DarkGreen
    $manifest = [IO.File]::ReadAllText($manifestPath)
    $manifest = IncreaseVerNum($manifest)
    [IO.File]::WriteAllText($manifestPath, $manifest)
    
    $toRemove =".\bin\" + $architech + "\*.apk"
    Remove-Item $toRemove -Force -ErrorAction SilentlyContinue
    $config = "/p:Configuration=" + $architech;
    $collectionOfArgs = @("aairvid\aairvid.csproj", "/t:Clean", "/target:SignAndroidPackage", "/fileLogger", "/noconsolelogger", "/verbosity:minimal", $config)
    if(!$isFreeVer) 
    {
         $collectionOfArgs += '/p:DefineConstants="NON_FREE_VERSION" ';
    }
    msbuild $collectionOfArgs | Out-Null
    
    Stop-Process -processname dos2unix
    
    if($LastExitCode -ne 0)
    {
        Write-Error "Failed to build " + $architech + " version." 
        return;
    }
    write-host sucessfully built $architech version  of APK -ForegroundColor DarkGreen
}

function SignAndAlignAndDist([string]$architch, [bool]$isFreeVer = $True)
{
    $suffix = "";
    if(!$isFreeVer)
    {
        $suffix = "pro";
    }
    write-host signing $architch version
    $apktosign = ".\bin\" + $architch + "\com.ezhang.aairvid"+ $suffix+".apk";
    $signedApk = ".\bin\" + $architch + "\com.ezhang.aairvid"+ $suffix+"-Signed.apk";
    $alignedApk = ".\bin\" + $architch + "\com.ezhang.aairvid"+ $suffix+"-aligned.apk";
    (jarsigner -certs -verbose:summary -signedjar $signedApk -sigalg SHA1withRSA -digestalg SHA1 -keystore e:\mydoc\release-key.keystore -storepass IlyZrnXl169254 -keypass IlyZrnXl169254  $apktosign googkey) | Out-Null
    (zipalign -v 4 $signedApk $alignedApk) | Out-Null       
    
    $distApk = "dist\com.ezhang.aairvid" + $suffix + ".signed" +$architch+ ".apk"
    Move-Item  -Path  $alignedApk -Destination $distApk -Force
}


(New-Item -ItemType Directory -Force -Path "dist") | Out-Null

(Get-Content .\aairvid\Properties\AndroidManifest.xml) | ForEach-Object { $_ -replace 'aairvidpro', 'aairvid'} | Set-Content .\aairvid\Properties\AndroidManifest.xml

DoLibBuild -architech arm -projPath ".\Bonjour.NET\Bonjour.NET.csproj"
DoLibBuild -architech armv7 -projPath ".\Bonjour.NET\Bonjour.NET.csproj"
DoLibBuild -architech x86 -projPath ".\Bonjour.NET\Bonjour.NET.csproj"

DoLibBuild -architech arm -projPath ".\libairvidproto\libairvidproto.csproj"
DoLibBuild -architech armv7 -projPath ".\libairvidproto\libairvidproto.csproj"
DoLibBuild -architech x86 -projPath ".\libairvidproto\libairvidproto.csproj"

$manifest = [IO.File]::ReadAllText($manifestPath)
$manifest = IncreaseVerName($manifest)
[IO.File]::WriteAllText($manifestPath, $manifest)
    
DoApkBuild -architech arm
DoApkBuild -architech armv7
DoApkBuild -architech x86

SignAndAlignAndDist -architch arm
SignAndAlignAndDist -architch armv7
SignAndAlignAndDist -architch x86

$c = ((Get-Content .\aairvid\Properties\AndroidManifest.xml) | ForEach-Object { $_ -replace "android:label=`"aairvid`"", "android:label=`"aairvidpro`"" -replace "package=`"com.ezhang.aairvid`"", "package=`"com.ezhang.aairvidpro`"" } )
Set-Content .\aairvid\Properties\AndroidManifest.xml -Value $c
    
DoApkBuild -architech arm -isFreeVer $False
DoApkBuild -architech armv7 -isFreeVer $False
DoApkBuild -architech x86 -isFreeVer $False

SignAndAlignAndDist -architch arm -isFreeVer $False
SignAndAlignAndDist -architch armv7 -isFreeVer $False
SignAndAlignAndDist -architch x86 -isFreeVer $False
    
(Get-Content .\aairvid\Properties\AndroidManifest.xml) | ForEach-Object { $_ -replace 'aairvidpro', 'aairvid'} | Set-Content .\aairvid\Properties\AndroidManifest.xml
write-host 'done' -ForegroundColor DarkGreen

