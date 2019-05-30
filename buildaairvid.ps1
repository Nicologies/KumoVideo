param(
  [parameter(mandatory=$true)]
  [string]$passwordOfKey
)
$ErrorActionPreference = "Stop"
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Set-Location $scriptPath

. .\IncreaseApkVer.ps1
. .\buildlib.ps1

($manifestPath = join-path $scriptPath ".\aairvid\Properties\AndroidManifest.xml") | Out-Null

function DoApkBuild ([bool]$isFreeVer = $True, [bool]$firstBuildOfThisRelease = $False)
{
    write-host building APK -ForegroundColor DarkGreen
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
    
    ($toRemove =join-path $scriptPath ("bin\Release\*.apk")) | Out-Null
    Remove-Item $toRemove -Force -ErrorAction SilentlyContinue
    $collectionOfArgs = @("aairvid\aairvid.csproj", "/t:Clean", "/target:SignAndroidPackage", "/fileLogger", "/noconsolelogger", "/verbosity:minimal", "/p:Configuration=Release")
    if(!$isFreeVer) 
    {
         $collectionOfArgs += '/p:DefineConstants="NON_FREE_VERSION" ';
    }
    msbuild.exe $collectionOfArgs

    if($LastExitCode -ne 0)
    {
        Write-Error "Failed to build. $LastExitCode" 
        return;
    }
    
    Stop-Process -processname dos2unix -ErrorAction SilentlyContinue    
    
    write-host suceeded -ForegroundColor DarkGreen
}

function SignAndAlignAndDist([bool]$isFreeVer = $True)
{
    $suffix = "";
    if(!$isFreeVer)
    {
        $suffix = "pro";
    }
    write-host signing
    $apktosignRelPath = (".\bin\Release\com.ezhang.kumovid"+ $suffix+".apk");
    ($apktosign = Join-Path $scriptPath ("bin\Release\com.ezhang.kumovid"+ $suffix+".apk")) | Out-Null;    
    
    
    ($signedApk = Join-Path $scriptPath ("bin\Release\com.ezhang.kumovid"+ $suffix+"-Signed.apk")) | Out-Null;
    ($alignedApk = Join-Path $scriptPath ("bin\Release\com.ezhang.kumovid"+ $suffix+"-aligned.apk")) | Out-Null;
    (jarsigner -certs -verbose:summary -signedjar $signedApk -sigalg SHA1withRSA -digestalg SHA1 -keystore "$scriptPath\release-key.keystore" -storepass $passwordOfKey -keypass $passwordOfKey $apktosign googkey) | Out-Null
    (zipalign -v 4 $signedApk $alignedApk) | Out-Null       
    
    ($distApk = Join-Path $scriptPath ("dist\com.ezhang.kumovid" + $suffix + ".signed.apk")) | Out-Null;
    Move-Item  -Path  $alignedApk -Destination $distApk -Force
}


(New-Item -ItemType Directory -Force -Path "dist") | Out-Null

(Get-Content .\aairvid\Properties\AndroidManifest.xml) | ForEach-Object { $_ -replace 'kumovidpro', 'kumovid'} | Set-Content .\aairvid\Properties\AndroidManifest.xml

. .\buildbonjour.ps1
. .\buildLibAirvidProto.ps1

$manifest = [IO.File]::ReadAllText($manifestPath)
$manifest = IncreaseVerName($manifest)
[IO.File]::WriteAllText($manifestPath, $manifest)
    
DoApkBuild -firstBuildOfThisRelease $True

SignAndAlignAndDist  -isFreeVer $True

$c = ((Get-Content .\aairvid\Properties\AndroidManifest.xml) | ForEach-Object { $_ -replace "android:label=`"kumovid`"", "android:label=`"kumovidpro`"" -replace "package=`"com.ezhang.kumovid`"", "package=`"com.ezhang.kumovidpro`"" } )
Set-Content .\aairvid\Properties\AndroidManifest.xml -Value $c
    
DoApkBuild -isFreeVer $False


SignAndAlignAndDist -isFreeVer $False
    
(Get-Content .\aairvid\Properties\AndroidManifest.xml) | ForEach-Object { $_ -replace 'kumovidpro', 'kumovid'} | Set-Content .\aairvid\Properties\AndroidManifest.xml
write-host 'done' -ForegroundColor DarkGreen

