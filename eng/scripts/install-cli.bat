@echo off
setlocal

set "SOLUTION_DIR=%~dp0..\.."
set "CLI_PROJECT=%SOLUTION_DIR%\src\CodeGenerator.Cli\CodeGenerator.Cli.csproj"

REM Uninstall existing version (ignore errors if not installed)
echo Uninstalling existing version (if any)...
dotnet tool uninstall --global QuinntyneBrown.CodeGenerator.Cli 2>nul

echo Building CodeGenerator.Cli...
dotnet build "%CLI_PROJECT%" -c Release
if %errorlevel% neq 0 (
    echo Build failed.
    exit /b 1
)

echo Packing CodeGenerator.Cli...
dotnet pack "%CLI_PROJECT%" -c Release
if %errorlevel% neq 0 (
    echo Failed to pack CodeGenerator.Cli.
    exit /b 1
)

echo Installing CodeGenerator.Cli as a global tool...
dotnet tool install --global --add-source "%SOLUTION_DIR%\src\CodeGenerator.Cli\nupkg" QuinntyneBrown.CodeGenerator.Cli
if %errorlevel% neq 0 (
    echo Failed to install CodeGenerator.Cli.
    exit /b 1
)

echo CodeGenerator.Cli installed successfully. Run 'create-code-cli' to use it.
