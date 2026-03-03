@echo off
setlocal

set "SOLUTION_DIR=%~dp0..\.."
set "CLI_PROJECT=%SOLUTION_DIR%\src\CodeGenerator.Cli\CodeGenerator.Cli.csproj"

echo Packing CodeGenerator.Cli...
dotnet pack "%CLI_PROJECT%" -c Release
if %errorlevel% neq 0 (
    echo Failed to pack CodeGenerator.Cli.
    exit /b 1
)

echo Installing CodeGenerator.Cli as a global tool...
dotnet tool install --global --add-source "%SOLUTION_DIR%\src\CodeGenerator.Cli\nupkg" QuinntyneBrown.CodeGenerator.Cli
if %errorlevel% neq 0 (
    echo Install failed. Attempting update...
    dotnet tool update --global --add-source "%SOLUTION_DIR%\src\CodeGenerator.Cli\nupkg" QuinntyneBrown.CodeGenerator.Cli
    if %errorlevel% neq 0 (
        echo Failed to install or update CodeGenerator.Cli.
        exit /b 1
    )
)

echo CodeGenerator.Cli installed successfully. Run 'create-code-cli' to use it.
