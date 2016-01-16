@echo Off
set config=%1
if "%config%" == "" (
   set config=Release
)

set version=
if not "%PackageVersion%" == "" (
   set version=-Version %PackageVersion%
)

set nunit="packages\NUnit.Console.3.0.1\tools\nunit3-console.exe"

REM Build
%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild AzureBlobSearchHelper.sln /p:Configuration="%config%" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=Normal /nr:false
if not "%errorlevel%"=="0" goto failure

REM Unit tests
%nunit% AzureBlobSearchHelper\AzureBlobSearchHelper.Tests\bin\%config%\AzureBlobSearchHelper.Tests.dll
if not "%errorlevel%"=="0" goto failure

REM Package
mkdir Build
call %nuget% pack "AzureBlobSearchHelper\AzureBlobSearchHelper\AzureBlobSearchHelper.csproj" -symbols -o Build -p Configuration=%config% %version%
if not "%errorlevel%"=="0" goto failure

:success
exit 0

:failure
exit -1