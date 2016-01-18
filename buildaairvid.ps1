$ErrorActionPreference = "Stop"
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Set-Location $scriptPath

. .\IncreaseApkVer.ps1
. .\buildlib.ps1

($manifestPath = join-path $scriptPath ".\aairvid\Properties\AndroidManifest.xml") | Out-Null

function DoApkBuild ([string]$architech, [bool]$isFreeVer = $True, [bool]$firstBuildOfThisRelease = $False)
{
    write-host building $architech version of APK -ForegroundColor DarkGreen
    $manifest = [IO.File]::ReadAllText($manifestPath)
    if($firstBuildOfThisRelease)
    {
        $manifest = InitVerNumForNewRelease($manifest)
    }
    else
    {
        $manifest = IncreaseVerNum($manifest)
    }
    [IO.File]::WriteAllText($manifestPath, $manifest)
    
    ($toRemove =join-path $scriptPath ("bin\" + $architech + "\*.apk")) | Out-Null
    Remove-Item $toRemove -Force -ErrorAction SilentlyContinue
    $config = "/p:Configuration=" + $architech;
    $collectionOfArgs = @("aairvid\aairvid.csproj", "/t:Clean", "/target:SignAndroidPackage", "/fileLogger", "/noconsolelogger", "/verbosity:minimal", $config)
    if(!$isFreeVer) 
    {
         $collectionOfArgs += '/p:DefineConstants="NON_FREE_VERSION" ';
    }
    msbuild $collectionOfArgs | Out-Null
    
    Stop-Process -processname dos2unix -ErrorAction SilentlyContinue
    
    if($LastExitCode -ne 0)
    {
        Write-Error "Failed to build $architech version." 
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
    $apktosignRelPath = (".\bin\" + $architch + "\com.ezhang.kumovid"+ $suffix+".apk");
    ($apktosign = Join-Path $scriptPath ("bin\" + $architch + "\com.ezhang.kumovid"+ $suffix+".apk")) | Out-Null;    
    
    
    ($signedApk = Join-Path $scriptPath ("bin\" + $architch + "\com.ezhang.kumovid"+ $suffix+"-Signed.apk")) | Out-Null;
    ($alignedApk = Join-Path $scriptPath ("bin\" + $architch + "\com.ezhang.kumovid"+ $suffix+"-aligned.apk")) | Out-Null;
    (jarsigner -certs -verbose:summary -signedjar $signedApk -sigalg SHA1withRSA -digestalg SHA1 -keystore e:\mydoc\release-key.keystore -storepass IlyZrnXl169254 -keypass IlyZrnXl169254  $apktosign googkey) | Out-Null
    (zipalign -v 4 $signedApk $alignedApk) | Out-Null       
    
    ($distApk = Join-Path $scriptPath ("dist\com.ezhang.kumovid" + $suffix + ".signed" +$architch+ ".apk")) | Out-Null;
    Move-Item  -Path  $alignedApk -Destination $distApk -Force
}


(New-Item -ItemType Directory -Force -Path "dist") | Out-Null

(Get-Content .\aairvid\Properties\AndroidManifest.xml) | ForEach-Object { $_ -replace 'kumovidpro', 'kumovid'} | Set-Content .\aairvid\Properties\AndroidManifest.xml

. .\buildbonjour.ps1
. .\buildLibAirvidProto.ps1

$manifest = [IO.File]::ReadAllText($manifestPath)
$manifest = IncreaseVerName($manifest)
[IO.File]::WriteAllText($manifestPath, $manifest)
    
DoApkBuild -architech arm -firstBuildOfThisRelease $True
DoApkBuild -architech armv7
DoApkBuild -architech x86

SignAndAlignAndDist -architch arm
SignAndAlignAndDist -architch armv7
SignAndAlignAndDist -architch x86

$c = ((Get-Content .\aairvid\Properties\AndroidManifest.xml) | ForEach-Object { $_ -replace "android:label=`"kumovid`"", "android:label=`"kumovidpro`"" -replace "package=`"com.ezhang.kumovid`"", "package=`"com.ezhang.kumovidpro`"" } )
Set-Content .\aairvid\Properties\AndroidManifest.xml -Value $c
    
DoApkBuild -architech arm -isFreeVer $False
DoApkBuild -architech armv7 -isFreeVer $False
DoApkBuild -architech x86 -isFreeVer $False

SignAndAlignAndDist -architch arm -isFreeVer $False
SignAndAlignAndDist -architch armv7 -isFreeVer $False
SignAndAlignAndDist -architch x86 -isFreeVer $False
    
(Get-Content .\aairvid\Properties\AndroidManifest.xml) | ForEach-Object { $_ -replace 'kumovidpro', 'kumovid'} | Set-Content .\aairvid\Properties\AndroidManifest.xml
write-host 'done' -ForegroundColor DarkGreen

