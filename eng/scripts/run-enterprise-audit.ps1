param(
    [int]$Iterations = 20,
    [string]$WorkRoot = (Join-Path $env:TEMP "code-generator-enterprise-audit"),
    [string]$DocsRoot = (Join-Path $PSScriptRoot "..\..\docs\enterprise-solution-audits")
)

$ErrorActionPreference = "Stop"

$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\.."))
$docsRoot = [System.IO.Path]::GetFullPath($DocsRoot)
$sourceRoot = Join-Path $repoRoot "src"
$solutionFolderGuid = "{2150E333-8FDC-42A3-9474-1A3956D46DE8}"
$typeScriptProjectTypeGuid = "{54A90642-561A-4BB1-A94E-469ADEE60C69}"
$summary = New-Object System.Collections.Generic.List[object]

function Invoke-Checked {
    param(
        [string]$Command,
        [string]$WorkingDirectory
    )

    Write-Host "[$WorkingDirectory] $Command"
    Push-Location $WorkingDirectory

    try {
        cmd.exe /c $Command | Out-Host

        if ($LASTEXITCODE -ne 0) {
            throw "Command failed with exit code ${LASTEXITCODE}: $Command"
        }
    }
    finally {
        Pop-Location
    }
}

function Get-NoWarnValue {
    return (@("1998", "4014,", "SA1101", "SA1600,", "SA1200", "SA1633", "1591", "SA1309") -join ",")
}

function Set-OrCreateXmlElement {
    param(
        [xml]$Document,
        [System.Xml.XmlElement]$Parent,
        [string]$Name,
        [string]$Value
    )

    $node = $Parent.SelectSingleNode($Name)

    if ($null -eq $node) {
        $node = $Document.CreateElement($Name)
        $Parent.AppendChild($node) | Out-Null
    }

    $node.InnerText = $Value
}

function Update-CsprojProperties {
    param(
        [string]$ProjectPath,
        [bool]$GenerateDocumentationFile = $false,
        [bool]$AddInvariantGlobalization = $false
    )

    [xml]$document = Get-Content $ProjectPath
    $propertyGroup = $document.Project.PropertyGroup | Select-Object -First 1

    Set-OrCreateXmlElement -Document $document -Parent $propertyGroup -Name "NoWarn" -Value (Get-NoWarnValue)

    if ($GenerateDocumentationFile) {
        Set-OrCreateXmlElement -Document $document -Parent $propertyGroup -Name "GenerateDocumentationFile" -Value "true"
    }

    if ($AddInvariantGlobalization) {
        Set-OrCreateXmlElement -Document $document -Parent $propertyGroup -Name "InvariantGlobalization" -Value "false"
    }

    $document.Save($ProjectPath)
}

function Remove-TemplateDefaults {
    param(
        [string]$ProjectDirectory,
        [string]$TemplateType
    )

    Get-ChildItem -Path $ProjectDirectory -Recurse -Filter "*1.cs" | Where-Object { -not $_.PSIsContainer } | Remove-Item -Force

    if ($TemplateType -eq "webapi") {
        $weatherControllerPath = Join-Path $ProjectDirectory "Controllers\WeatherForecastController.cs"
        $weatherForecastPath = Join-Path $ProjectDirectory "WeatherForecast.cs"
        $controllersDirectory = Join-Path $ProjectDirectory "Controllers"

        if (Test-Path $weatherControllerPath) {
            Remove-Item $weatherControllerPath -Force
        }

        if (Test-Path $weatherForecastPath) {
            Remove-Item $weatherForecastPath -Force
        }

        if ((Test-Path $controllersDirectory) -and -not (Get-ChildItem $controllersDirectory -Force)) {
            Remove-Item $controllersDirectory -Force
        }
    }
}

function Sanitize-AngularProjectName {
    param([string]$Name)

    $sanitized = $Name.Replace(".", "-")
    $sanitized = [System.Text.RegularExpressions.Regex]::Replace($sanitized, "([a-z0-9])([A-Z])", '$1-$2')
    $sanitized = [System.Text.RegularExpressions.Regex]::Replace($sanitized, "[^a-zA-Z0-9\-]", "-")
    $sanitized = [System.Text.RegularExpressions.Regex]::Replace($sanitized, "\-+", "-")
    $sanitized = $sanitized.Trim("-").ToLowerInvariant()

    if ([string]::IsNullOrWhiteSpace($sanitized)) {
        return "app"
    }

    return $sanitized
}

function New-EsProjContent {
    param([string]$AngularProjectName)

    return @"
<Project Sdk="Microsoft.VisualStudio.JavaScript.Sdk">
  <PropertyGroup>
    <StartupCommand>npm start</StartupCommand>
    <JavaScriptTestRoot>src\</JavaScriptTestRoot>
    <BuildCommand>npm run build</BuildCommand>
    <BuildOutputFolder>dist\$AngularProjectName</BuildOutputFolder>
    <ShouldRunNpmInstall>false</ShouldRunNpmInstall>
  </PropertyGroup>
</Project>
"@
}

function Write-Utf8NoBom {
    param(
        [string]$Path,
        [string]$Content
    )

    $encoding = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($Path, $Content, $encoding)
}

function Get-DeterministicPorts {
    param([string]$ProjectName)

    $sha = [System.Security.Cryptography.SHA256]::Create()

    try {
        $hash = $sha.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($ProjectName))
    }
    finally {
        $sha.Dispose()
    }

    return [PSCustomObject]@{
        HttpPort = 5000 + ([BitConverter]::ToUInt16($hash, 0) % 1000)
        HttpsPort = 7000 + ([BitConverter]::ToUInt16($hash, 2) % 1000)
    }
}

function Normalize-WebApiTemplateFiles {
    param(
        [string]$ProjectDirectory,
        [string]$ProjectName
    )

    $ports = Get-DeterministicPorts -ProjectName $ProjectName
    $launchSettingsPath = Join-Path $ProjectDirectory "Properties\launchSettings.json"
    $httpFilePath = Join-Path $ProjectDirectory "$ProjectName.http"

    if (Test-Path $launchSettingsPath) {
        $launchSettings = Get-Content -Raw $launchSettingsPath | ConvertFrom-Json
        $launchSettings.profiles.http.applicationUrl = "http://localhost:$($ports.HttpPort)"
        $launchSettings.profiles.https.applicationUrl = "https://localhost:$($ports.HttpsPort);http://localhost:$($ports.HttpPort)"

        Write-Utf8NoBom -Path $launchSettingsPath -Content (($launchSettings | ConvertTo-Json -Depth 10) + [Environment]::NewLine)
    }

    if (Test-Path $httpFilePath) {
        $lines = Get-Content $httpFilePath

        if ($lines.Count -gt 0) {
            $lines[0] = "@$ProjectName`_HostAddress = http://localhost:$($ports.HttpPort)"
            Write-Utf8NoBom -Path $httpFilePath -Content (($lines -join [Environment]::NewLine) + [Environment]::NewLine)
        }
    }
}

function Get-RelativePath {
    param(
        [string]$BasePath,
        [string]$TargetPath
    )

    $baseUri = [Uri]((Resolve-Path $BasePath).Path.TrimEnd("\") + "\")
    $targetUri = [Uri](Resolve-Path $TargetPath).Path

    return [Uri]::UnescapeDataString($baseUri.MakeRelativeUri($targetUri).ToString()).Replace("/", "\")
}

function Find-LastProjectEndIndex {
    param([System.Collections.Generic.List[string]]$Lines)

    $lastIndex = -1

    for ($i = 0; $i -lt $Lines.Count; $i++) {
        if ($Lines[$i].Trim() -eq "EndProject") {
            $lastIndex = $i
        }

        if ($Lines[$i].Trim() -eq "Global") {
            break
        }
    }

    return $lastIndex
}

function Find-GlobalSectionEndIndex {
    param(
        [System.Collections.Generic.List[string]]$Lines,
        [string]$SectionName
    )

    $inSection = $false

    for ($i = 0; $i -lt $Lines.Count; $i++) {
        if ($Lines[$i].Contains("GlobalSection($SectionName)")) {
            $inSection = $true
            continue
        }

        if ($inSection -and $Lines[$i].Contains("EndGlobalSection")) {
            return $i
        }
    }

    return -1
}

function Get-SolutionConfigurationPlatforms {
    param([System.Collections.Generic.List[string]]$Lines)

    $platforms = New-Object System.Collections.Generic.List[string]
    $inSection = $false

    foreach ($line in $Lines) {
        if ($line.Contains("GlobalSection(SolutionConfigurationPlatforms)")) {
            $inSection = $true
            continue
        }

        if ($inSection) {
            if ($line.Contains("EndGlobalSection")) {
                break
            }

            $trimmed = $line.Trim()

            if (-not [string]::IsNullOrWhiteSpace($trimmed)) {
                $platforms.Add(($trimmed.Split("=")[0]).Trim())
            }
        }
    }

    if ($platforms.Count -eq 0) {
        foreach ($platform in @("Debug|Any CPU", "Debug|x64", "Debug|x86", "Release|Any CPU", "Release|x64", "Release|x86")) {
            $platforms.Add($platform)
        }
    }

    return $platforms
}

function Find-SolutionFolderGuid {
    param(
        [System.Collections.Generic.List[string]]$Lines,
        [string]$FolderName
    )

    $escapedGuid = [Regex]::Escape($solutionFolderGuid)
    $escapedFolder = [Regex]::Escape($FolderName)
    $pattern = "Project\(`"$escapedGuid`"\)\s*=\s*`"$escapedFolder`",\s*`"$escapedFolder`",\s*`"(\{[A-F0-9\-]+\})`""

    foreach ($line in $Lines) {
        $match = [Regex]::Match($line, $pattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)

        if ($match.Success) {
            return $match.Groups[1].Value
        }
    }

    return $null
}

function Add-EsprojToSolution {
    param(
        [string]$SolutionPath,
        [string]$ProjectName,
        [string]$ProjectPath
    )

    $solutionDirectory = Split-Path -Parent $SolutionPath
    $lines = [System.Collections.Generic.List[string]]::new()

    foreach ($line in Get-Content $SolutionPath) {
        $lines.Add($line)
    }

    $projectGuid = "{0}" -f ([guid]::NewGuid().ToString().ToUpperInvariant())
    $relativePath = Get-RelativePath -BasePath $solutionDirectory -TargetPath $ProjectPath
    $pathParts = $relativePath.Split("\")
    $solutionFolderGuidForProject = $null

    if ($pathParts.Length -gt 2) {
        $solutionFolderGuidForProject = Find-SolutionFolderGuid -Lines $lines -FolderName $pathParts[0]
    }

    $insertProjectIndex = Find-LastProjectEndIndex -Lines $lines

    if ($insertProjectIndex -lt 0) {
        $insertProjectIndex = ($lines | Select-String -Pattern "^MinimumVisualStudioVersion").LineNumber - 1
    }

    $projectEntry = [string[]]@(
        "Project(`"$typeScriptProjectTypeGuid`") = `"$ProjectName`", `"$relativePath`", `"$projectGuid`"",
        "EndProject"
    )

    $lines.InsertRange($insertProjectIndex + 1, $projectEntry)

    $configEntries = New-Object System.Collections.Generic.List[string]
    foreach ($platform in Get-SolutionConfigurationPlatforms -Lines $lines) {
        $configName = $platform.Split("|")[0]
        $configEntries.Add("`t`t$projectGuid.$platform.ActiveCfg = $configName|Any CPU")
        $configEntries.Add("`t`t$projectGuid.$platform.Build.0 = $configName|Any CPU")
        $configEntries.Add("`t`t$projectGuid.$platform.Deploy.0 = $configName|Any CPU")
    }

    $configSectionEndIndex = Find-GlobalSectionEndIndex -Lines $lines -SectionName "ProjectConfigurationPlatforms"

    if ($configSectionEndIndex -ge 0) {
        $lines.InsertRange($configSectionEndIndex, $configEntries)
    }

    if (-not [string]::IsNullOrWhiteSpace($solutionFolderGuidForProject)) {
        $nestedSectionEndIndex = Find-GlobalSectionEndIndex -Lines $lines -SectionName "NestedProjects"

        if ($nestedSectionEndIndex -ge 0) {
            $lines.Insert($nestedSectionEndIndex, "`t`t$projectGuid = $solutionFolderGuidForProject")
        }
    }

    Set-Content -Path $SolutionPath -Value $lines
}

function Get-FilteredFiles {
    param([string]$Root)

    $result = @{}
    $prefixLength = $Root.TrimEnd("\").Length + 1

    Get-ChildItem -Path $Root -Recurse | Where-Object {
        -not $_.PSIsContainer -and $_.FullName -notmatch "\\(bin|obj|node_modules|\.angular)\\"
    } | ForEach-Object {
        $relativePath = $_.FullName.Substring($prefixLength).Replace("\", "/")
        $result[$relativePath] = $_.FullName
    }

    return $result
}

function Get-ComparableContent {
    param([string]$Path)

    $extension = [System.IO.Path]::GetExtension($Path).ToLowerInvariant()
    $fileName = [System.IO.Path]::GetFileName($Path)

    if ($fileName -ieq "launchSettings.json") {
        return ((Get-Content -Raw $Path | ConvertFrom-Json) | ConvertTo-Json -Depth 20 -Compress)
    }

    if ($extension -in @(".csproj", ".esproj", ".md", ".http", ".json", ".xml")) {
        return ([System.IO.File]::ReadAllText($Path)).Replace("`r`n", "`n").TrimEnd("`r", "`n")
    }

    return $null
}

function Compare-Trees {
    param(
        [string]$BaselineRoot,
        [string]$GeneratedRoot
    )

    $baselineFiles = Get-FilteredFiles -Root $BaselineRoot
    $generatedFiles = Get-FilteredFiles -Root $GeneratedRoot
    $baselineKeys = @($baselineFiles.Keys)
    $generatedKeys = @($generatedFiles.Keys)
    $missingFiles = New-Object System.Collections.Generic.List[string]
    $extraFiles = New-Object System.Collections.Generic.List[string]
    $differentFiles = New-Object System.Collections.Generic.List[string]
    $ignoredFiles = New-Object System.Collections.Generic.List[string]

    foreach ($key in ($baselineKeys | Sort-Object)) {
        if ($generatedKeys -notcontains $key) {
            $missingFiles.Add($key)
        }
    }

    foreach ($key in ($generatedKeys | Sort-Object)) {
        if ($baselineKeys -notcontains $key) {
            $extraFiles.Add($key)
        }
    }

    foreach ($key in ($baselineKeys | Where-Object { $generatedKeys -contains $_ } | Sort-Object)) {
        if ($key.EndsWith(".sln") -or $key.EndsWith(".slnx")) {
            $ignoredFiles.Add($key)
            continue
        }

        $baselineComparable = Get-ComparableContent -Path $baselineFiles[$key]
        $generatedComparable = Get-ComparableContent -Path $generatedFiles[$key]

        if ($null -ne $baselineComparable -and $null -ne $generatedComparable) {
            if ($baselineComparable -ne $generatedComparable) {
                $differentFiles.Add($key)
            }
        }
        else {
            $baselineHash = (Get-FileHash -Algorithm SHA256 -Path $baselineFiles[$key]).Hash
            $generatedHash = (Get-FileHash -Algorithm SHA256 -Path $generatedFiles[$key]).Hash

            if ($baselineHash -ne $generatedHash) {
                $differentFiles.Add($key)
            }
        }
    }

    return [PSCustomObject]@{
        MissingFiles = $missingFiles
        ExtraFiles = $extraFiles
        DifferentFiles = $differentFiles
        IgnoredFiles = $ignoredFiles
        BaselineFileCount = $baselineFiles.Count
        GeneratedFileCount = $generatedFiles.Count
    }
}

function Format-ListSection {
    param(
        [string]$Title,
        [System.Collections.IEnumerable]$Items
    )

    $itemsArray = @($Items)

    if ($itemsArray.Count -eq 0) {
        return @("## $Title", "", "None.", "")
    }

    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add("## $Title")
    $lines.Add("")

    foreach ($item in $itemsArray) {
        $lines.Add("- $item")
    }

    $lines.Add("")
    return $lines
}

function Write-AuditReport {
    param(
        [int]$Iteration,
        [string]$SolutionName,
        [string]$BaselineRoot,
        [string]$GeneratedRoot,
        [object]$Comparison,
        [string]$ReportPath
    )

    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add("# Iteration {0:D2} Audit" -f $Iteration)
    $lines.Add("")
    $lines.Add("## Overview")
    $lines.Add("")
    $lines.Add(('- Solution: `{0}`' -f $SolutionName))
    $lines.Add(('- Baseline root: `{0}`' -f $BaselineRoot))
    $lines.Add(('- Generated root: `{0}`' -f $GeneratedRoot))
    $lines.Add("- Normalization: Web API launch/profile ports are normalized before comparison because `dotnet new webapi` assigns them non-deterministically.")
    $lines.Add("- Normalization: `.csproj`, `.esproj`, `.md`, `.http`, `.xml`, and JSON formatting are compared semantically so the audit focuses on meaningful content differences instead of line endings or pretty-printing.")
    $lines.Add("- Baseline files: $($Comparison.BaselineFileCount)")
    $lines.Add("- Generated files: $($Comparison.GeneratedFileCount)")
    $lines.Add("- Missing files: $($Comparison.MissingFiles.Count)")
    $lines.Add("- Extra files: $($Comparison.ExtraFiles.Count)")
    $lines.Add("- Different files: $($Comparison.DifferentFiles.Count)")
    $lines.Add("- Ignored non-deterministic files: $($Comparison.IgnoredFiles.Count)")
    $lines.Add("")

    foreach ($line in (Format-ListSection -Title "Missing Files" -Items $Comparison.MissingFiles)) {
        $lines.Add($line)
    }

    foreach ($line in (Format-ListSection -Title "Extra Files" -Items $Comparison.ExtraFiles)) {
        $lines.Add($line)
    }

    foreach ($line in (Format-ListSection -Title "Different Files" -Items $Comparison.DifferentFiles)) {
        $lines.Add($line)
    }

    foreach ($line in (Format-ListSection -Title "Ignored Files" -Items $Comparison.IgnoredFiles)) {
        $lines.Add($line)
    }

    $lines.Add("## Repo Changes Needed")
    $lines.Add("")

    if ($Comparison.MissingFiles.Count -eq 0 -and $Comparison.ExtraFiles.Count -eq 0 -and $Comparison.DifferentFiles.Count -eq 0) {
        $lines.Add("No additional changes were identified from this iteration.")
    }
    else {
        $highSignalPaths = @($Comparison.MissingFiles + $Comparison.ExtraFiles + $Comparison.DifferentFiles | Select-Object -First 12)
        $lines.Add("The generator still needs to align the following paths with the direct scaffold:")
        $lines.Add("")

        foreach ($path in $highSignalPaths) {
            $lines.Add("- $path")
        }
    }

    [System.IO.File]::WriteAllLines($ReportPath, [string[]]$lines)
}

function New-BaselineEnterpriseSolution {
    param(
        [string]$Root,
        [string]$SolutionName
    )

    $solutionDirectory = Join-Path $Root $SolutionName
    $srcDirectory = Join-Path $solutionDirectory "src"
    $docsDirectory = Join-Path $solutionDirectory "docs"

    New-Item -ItemType Directory -Path $solutionDirectory -Force | Out-Null
    New-Item -ItemType Directory -Path $srcDirectory -Force | Out-Null
    New-Item -ItemType Directory -Path $docsDirectory -Force | Out-Null

    Invoke-Checked -Command "dotnet new sln -n $SolutionName" -WorkingDirectory $solutionDirectory

    Set-Content -Path (Join-Path $solutionDirectory "README.md") -Value "# $SolutionName"
    Set-Content -Path (Join-Path $docsDirectory "README.md") -Value "# $SolutionName"

    $projectSpecs = @(
        @{ Name = "$SolutionName.Api"; Type = "webapi"; GenerateDocumentationFile = $true; AddInvariantGlobalization = $true },
        @{ Name = "$SolutionName.Domain"; Type = "classlib"; GenerateDocumentationFile = $false; AddInvariantGlobalization = $false },
        @{ Name = "$SolutionName.Infrastructure"; Type = "classlib"; GenerateDocumentationFile = $false; AddInvariantGlobalization = $false },
        @{ Name = "$SolutionName.Application"; Type = "classlib"; GenerateDocumentationFile = $false; AddInvariantGlobalization = $false }
    )

    foreach ($spec in $projectSpecs) {
        $projectDirectory = Join-Path $srcDirectory $spec.Name
        New-Item -ItemType Directory -Path $projectDirectory -Force | Out-Null

        Invoke-Checked -Command ("dotnet new {0} --framework net9.0 --no-restore" -f $spec.Type) -WorkingDirectory $projectDirectory
        Remove-TemplateDefaults -ProjectDirectory $projectDirectory -TemplateType $spec.Type
        Update-CsprojProperties -ProjectPath (Join-Path $projectDirectory ("{0}.csproj" -f $spec.Name)) -GenerateDocumentationFile $spec.GenerateDocumentationFile -AddInvariantGlobalization $spec.AddInvariantGlobalization

        if ($spec.Type -eq "webapi") {
            Normalize-WebApiTemplateFiles -ProjectDirectory $projectDirectory -ProjectName $spec.Name
        }

        Invoke-Checked -Command ("dotnet sln {0}.sln add {1}" -f $SolutionName, (Join-Path $projectDirectory ("{0}.csproj" -f $spec.Name))) -WorkingDirectory $solutionDirectory
    }

    Invoke-Checked -Command ("dotnet add {0} reference {1}" -f (Join-Path $srcDirectory "$SolutionName.Infrastructure"), (Join-Path $srcDirectory "$SolutionName.Domain\$SolutionName.Domain.csproj")) -WorkingDirectory $solutionDirectory
    Invoke-Checked -Command ("dotnet add {0} reference {1}" -f (Join-Path $srcDirectory "$SolutionName.Application"), (Join-Path $srcDirectory "$SolutionName.Domain\$SolutionName.Domain.csproj")) -WorkingDirectory $solutionDirectory
    Invoke-Checked -Command ("dotnet add {0} reference {1}" -f (Join-Path $srcDirectory "$SolutionName.Api"), (Join-Path $srcDirectory "$SolutionName.Application\$SolutionName.Application.csproj")) -WorkingDirectory $solutionDirectory
    Invoke-Checked -Command ("dotnet add {0} reference {1}" -f (Join-Path $srcDirectory "$SolutionName.Api"), (Join-Path $srcDirectory "$SolutionName.Infrastructure\$SolutionName.Infrastructure.csproj")) -WorkingDirectory $solutionDirectory

    $frontendProjectName = "$SolutionName.Web"
    $frontendAngularName = Sanitize-AngularProjectName -Name $frontendProjectName
    $frontendDirectory = Join-Path $srcDirectory $frontendProjectName
    $esprojPath = Join-Path $frontendDirectory "$frontendProjectName.esproj"

    New-Item -ItemType Directory -Path $frontendDirectory -Force | Out-Null
    Invoke-Checked -Command ("ng new {0} --no-create-application --directory ./ --defaults --skip-git --skip-install" -f $frontendAngularName) -WorkingDirectory $frontendDirectory
    Invoke-Checked -Command ("ng g application {0} --defaults --skip-tests --skip-install" -f $frontendAngularName) -WorkingDirectory $frontendDirectory
    Set-Content -Path $esprojPath -Value (New-EsProjContent -AngularProjectName $frontendAngularName)
    Add-EsprojToSolution -SolutionPath (Join-Path $solutionDirectory "$SolutionName.sln") -ProjectName $frontendProjectName -ProjectPath $esprojPath

    return $solutionDirectory
}

function New-GeneratedEnterpriseSolution {
    param(
        [string]$Root,
        [string]$SolutionName
    )

    $generatorName = "$SolutionName.Generator"
    $generatorWorkspace = Join-Path $Root "generator-workspace"
    $generatedRoot = Join-Path $Root "generated-output"

    New-Item -ItemType Directory -Path $generatorWorkspace -Force | Out-Null
    New-Item -ItemType Directory -Path $generatedRoot -Force | Out-Null

    Invoke-Checked -Command ("dotnet run --project src\CodeGenerator.Cli --no-build -- -n {0} -o {1} --local-source-root {2}" -f $generatorName, $generatorWorkspace, $sourceRoot) -WorkingDirectory $repoRoot

    $generatorProjectPath = Join-Path $generatorWorkspace "$generatorName\src\$generatorName.Cli\$generatorName.Cli.csproj"
    Invoke-Checked -Command ("dotnet build {0}" -f $generatorProjectPath) -WorkingDirectory $repoRoot
    Invoke-Checked -Command ("dotnet run --project {0} --no-build -- enterprise-solution -n {1} -o {2}" -f $generatorProjectPath, $SolutionName, $generatedRoot) -WorkingDirectory $repoRoot

    return (Join-Path $generatedRoot $SolutionName)
}

if (Test-Path $WorkRoot) {
    Remove-Item -Recurse -Force -ErrorAction SilentlyContinue $WorkRoot
}

New-Item -ItemType Directory -Path $WorkRoot -Force | Out-Null
New-Item -ItemType Directory -Path $docsRoot -Force | Out-Null
Get-ChildItem -Path $docsRoot -Filter "iteration-*.md" -ErrorAction SilentlyContinue | Remove-Item -Force

Invoke-Checked -Command "dotnet build src\CodeGenerator.Cli\CodeGenerator.Cli.csproj" -WorkingDirectory $repoRoot

for ($iteration = 1; $iteration -le $Iterations; $iteration++) {
    $iterationDirectory = Join-Path $WorkRoot ("iteration-{0:D2}" -f $iteration)
    $solutionName = "EnterpriseAudit{0:D2}" -f $iteration
    $reportPath = Join-Path $docsRoot ("iteration-{0:D2}.md" -f $iteration)

    if (Test-Path $iterationDirectory) {
        Remove-Item -Recurse -Force -ErrorAction SilentlyContinue $iterationDirectory
    }

    New-Item -ItemType Directory -Path $iterationDirectory -Force | Out-Null

    try {
        $baselineRoot = New-BaselineEnterpriseSolution -Root (Join-Path $iterationDirectory "baseline") -SolutionName $solutionName
        $generatedRoot = New-GeneratedEnterpriseSolution -Root (Join-Path $iterationDirectory "generated") -SolutionName $solutionName
        $comparison = Compare-Trees -BaselineRoot $baselineRoot -GeneratedRoot $generatedRoot

        Write-AuditReport -Iteration $iteration -SolutionName $solutionName -BaselineRoot $baselineRoot -GeneratedRoot $generatedRoot -Comparison $comparison -ReportPath $reportPath

        $summary.Add([PSCustomObject]@{
            Iteration = $iteration
            Solution = $solutionName
            Missing = $comparison.MissingFiles.Count
            Extra = $comparison.ExtraFiles.Count
            Different = $comparison.DifferentFiles.Count
        }) | Out-Null
    }
    catch {
        $failureLines = @(
            ("# Iteration {0:D2} Audit" -f $iteration),
            "",
            "## Failure",
            "",
            $_.Exception.ToString()
        )

        [System.IO.File]::WriteAllLines($reportPath, [string[]]$failureLines)

        $summary.Add([PSCustomObject]@{
            Iteration = $iteration
            Solution = $solutionName
            Missing = "n/a"
            Extra = "n/a"
            Different = "failed"
        }) | Out-Null
    }
}

$summaryLines = New-Object System.Collections.Generic.List[string]
$summaryLines.Add("# Enterprise Solution Audit Summary")
$summaryLines.Add("")
$summaryLines.Add("| Iteration | Solution | Missing | Extra | Different |")
$summaryLines.Add("| --- | --- | ---: | ---: | ---: |")

foreach ($row in $summary) {
    $iterationLabel = if ($row.Iteration -is [int]) { "{0:D2}" -f $row.Iteration } else { [string]$row.Iteration }
    $summaryLines.Add("| $iterationLabel | $($row.Solution) | $($row.Missing) | $($row.Extra) | $($row.Different) |")
}

[System.IO.File]::WriteAllLines((Join-Path $docsRoot "SUMMARY.md"), [string[]]$summaryLines)
