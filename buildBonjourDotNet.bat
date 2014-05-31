cd Bonjour.NET
@echo building arm version
@msbuild /p:Configuration=Arm /t:SignAndroidPackage Bonjour.NET.csproj /fileLogger  /noconsolelogger /verbosity:minimal

@echo building armv7 version
@msbuild /p:Configuration=ArmV7 /t:SignAndroidPackage Bonjour.NET.csproj /fileLogger  /noconsolelogger /verbosity:minimal

@echo building x86 version
@msbuild /p:Configuration=X86 /t:SignAndroidPackage Bonjour.NET.csproj /fileLogger  /noconsolelogger /verbosity:minimal

cd ..