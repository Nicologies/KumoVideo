ApkVerIncrease.exe -m .\aairvid\Properties\AndroidManifest.xml
msbuild /p:Configuration=Arm aairvid.sln /fileLogger  /noconsolelogger /verbosity:minimal
msbuild /p:Configuration=Arm /t:SignAndroidPackage aairvid\aairvid.csproj /fileLogger  /noconsolelogger /verbosity:minimal
ApkVerIncrease.exe -m .\aairvid\Properties\AndroidManifest.xml
msbuild /p:Configuration=ArmV7 aairvid.sln /fileLogger  /noconsolelogger /verbosity:minimal
msbuild /p:Configuration=ArmV7 /t:SignAndroidPackage aairvid\aairvid.csproj /fileLogger  /noconsolelogger /verbosity:minimal
ApkVerIncrease.exe -m .\aairvid\Properties\AndroidManifest.xml
msbuild /p:Configuration=X86 aairvid.sln /fileLogger  /noconsolelogger /verbosity:minimal
msbuild /p:Configuration=X86 /t:SignAndroidPackage aairvid\aairvid.csproj /fileLogger  /noconsolelogger /verbosity:minimal

call sign.bat