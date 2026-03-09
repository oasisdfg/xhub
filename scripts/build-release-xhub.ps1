<#
.SYNOPSIS
    Unified build and release script for programs distributed through xhub.

.DESCRIPTION
    Presents a menu to select a program, prompts for the new version number, then:
      1. Updates the version in the program's .csproj
      2. Updates AppVersion in the Inno Setup .iss script
      3. Cleans and builds the project
      4. Compiles the installer with Inno Setup (ISCC.exe)
      5. Creates a GitHub Release and uploads the installer

.NOTES
    Requires: dotnet CLI, Inno Setup 6 (ISCC.exe), GitHub CLI (gh)
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ---------------------------------------------------------------------------
# Per-program configuration
# NOTE: The paths below are developer-specific. Update them to match your
#       local repository layout before running this script.
# ---------------------------------------------------------------------------

$XhubRoot = $PSScriptRoot | Split-Path -Parent

$ProgramConfigs = @{
    1 = @{
        Name           = 'stretchedres'
        ProjectDir     = 'C:\Users\tommy\source\repos\stretchedres\stretchedres'
        ProjectFile    = 'C:\Users\tommy\source\repos\stretchedres\stretchedres\stretchedres.csproj'
        InnoScript     = (Join-Path $XhubRoot 'installers\stretchedres.iss')
        BuildOutput    = 'C:\Users\tommy\source\repos\stretchedres\stretchedres\bin\Release\net8.0-windows\win-x64'
        InstallerName  = 'stretchedres_setup.exe'
        GitHubRepo     = 'oasisdfg/xhub'
        TagFormat      = 'stretchedres-v{0}'
        ExeName        = 'stretchedres.exe'
    }
    2 = @{
        Name           = 'PlayerLookup'
        ProjectDir     = 'C:\Users\tommy\source\repos\PlayerLookup\PlayerLookup'
        ProjectFile    = 'C:\Users\tommy\source\repos\PlayerLookup\PlayerLookup\PlayerLookup.csproj'
        InnoScript     = (Join-Path $XhubRoot 'installers\playerlookup.iss')
        BuildOutput    = 'C:\Users\tommy\source\repos\PlayerLookup\PlayerLookup\bin\Release\net8.0-windows\win-x64'
        InstallerName  = 'playerlookup_setup.exe'
        GitHubRepo     = 'oasisdfg/xhub'
        TagFormat      = 'playerlookup-v{0}'
        ExeName        = 'PlayerLookup.exe'
    }
    3 = @{
        Name           = 'xtool'
        ProjectDir     = 'C:\Users\tommy\source\repos\xtool\xtool'
        ProjectFile    = 'C:\Users\tommy\source\repos\xtool\xtool\xtool.csproj'
        InnoScript     = 'C:\Users\tommy\source\repos\xtool\xtool\xtool.iss'
        BuildOutput    = 'G:\xtool\Release\net8.0-windows\win-x64'
        InstallerName  = 'xtool_setup.exe'
        GitHubRepo     = 'oasisdfg/xtool'
        TagFormat      = 'v{0}'
        ExeName        = 'xtool.exe'
    }
}

$InnoCompiler = 'C:\Program Files (x86)\Inno Setup 6\ISCC.exe'

# ---------------------------------------------------------------------------
# Helper functions
# ---------------------------------------------------------------------------

function Write-Step([string]$Message) {
    Write-Host "`n==> $Message" -ForegroundColor Cyan
}

function Update-CsprojVersion([string]$CsprojPath, [string]$Version) {
    $content = Get-Content $CsprojPath -Raw
    $content = $content -replace '(<Version>)[^<]*(</Version>)',         "`${1}$Version`${2}"
    $content = $content -replace '(<AssemblyVersion>)[^<]*(</AssemblyVersion>)', "`${1}$Version`${2}"
    $content = $content -replace '(<FileVersion>)[^<]*(</FileVersion>)',  "`${1}$Version`${2}"
    Set-Content $CsprojPath $content -NoNewline
}

function Update-IssVersion([string]$IssPath, [string]$Version) {
    $content = Get-Content $IssPath -Raw
    $content = $content -replace '(?m)^(AppVersion=).*$', "`${1}$Version"
    Set-Content $IssPath $content -NoNewline
}

# ---------------------------------------------------------------------------
# Menu
# ---------------------------------------------------------------------------

Write-Host ''
Write-Host '  xhub build/release script' -ForegroundColor White
Write-Host '  ─────────────────────────'
foreach ($key in ($ProgramConfigs.Keys | Sort-Object)) {
    Write-Host "  $key) $($ProgramConfigs[$key].Name)"
}
Write-Host ''

$selection = Read-Host 'Select program (1-3)'
if (-not $ProgramConfigs.ContainsKey([int]$selection)) {
    Write-Error "Invalid selection: $selection"
    exit 1
}

$cfg = $ProgramConfigs[[int]$selection]

$version = Read-Host "Enter new version for $($cfg.Name) (e.g. 1.2.3)"
if ($version -notmatch '^\d+\.\d+(\.\d+(\.\d+)?)?$') {
    Write-Error "Invalid version format: $version"
    exit 1
}

$tag      = $cfg.TagFormat -f $version
$exePath  = Join-Path $cfg.BuildOutput $cfg.ExeName
$installerOutput = Join-Path (Split-Path $cfg.InnoScript -Parent) "Output\$($cfg.InstallerName)"

Write-Host ''
Write-Host "  Program : $($cfg.Name)"   -ForegroundColor Yellow
Write-Host "  Version : $version"        -ForegroundColor Yellow
Write-Host "  Tag     : $tag"            -ForegroundColor Yellow
Write-Host ''
$confirm = Read-Host 'Proceed? [y/N]'
if ($confirm -notmatch '^[Yy]$') {
    Write-Host 'Aborted.' -ForegroundColor Red
    exit 0
}

# ---------------------------------------------------------------------------
# Step 1 – Update .csproj version
# ---------------------------------------------------------------------------

Write-Step "Updating version in $($cfg.ProjectFile)"
Update-CsprojVersion $cfg.ProjectFile $version
Write-Host "  Done."

# ---------------------------------------------------------------------------
# Step 2 – Update .iss AppVersion
# ---------------------------------------------------------------------------

Write-Step "Updating AppVersion in $($cfg.InnoScript)"
Update-IssVersion $cfg.InnoScript $version
Write-Host "  Done."

# ---------------------------------------------------------------------------
# Step 3 – Clean previous build
# ---------------------------------------------------------------------------

Write-Step "Cleaning previous build"
& dotnet clean $cfg.ProjectFile -c Release
if ($LASTEXITCODE -ne 0) { Write-Error "dotnet clean failed."; exit 1 }

# ---------------------------------------------------------------------------
# Step 4 – Build project
# ---------------------------------------------------------------------------

Write-Step "Building $($cfg.Name)"
& dotnet build $cfg.ProjectFile -c Release -r win-x64 --self-contained false
if ($LASTEXITCODE -ne 0) { Write-Error "dotnet build failed."; exit 1 }

# ---------------------------------------------------------------------------
# Step 5 – Verify exe exists in build output
# ---------------------------------------------------------------------------

Write-Step "Verifying build output: $exePath"
if (-not (Test-Path $exePath)) {
    Write-Error "Expected exe not found: $exePath"
    exit 1
}
Write-Host "  Found: $exePath"

# ---------------------------------------------------------------------------
# Step 6 – Compile installer with Inno Setup
# ---------------------------------------------------------------------------

Write-Step "Compiling installer with Inno Setup"
if (-not (Test-Path $InnoCompiler)) {
    Write-Error "Inno Setup compiler not found: $InnoCompiler"
    exit 1
}
& $InnoCompiler /DBuildOutput="$($cfg.BuildOutput)" $cfg.InnoScript
if ($LASTEXITCODE -ne 0) { Write-Error "Inno Setup compilation failed."; exit 1 }

# ---------------------------------------------------------------------------
# Step 7 – Verify installer output
# ---------------------------------------------------------------------------

Write-Step "Verifying installer output: $installerOutput"
if (-not (Test-Path $installerOutput)) {
    Write-Error "Expected installer not found: $installerOutput"
    exit 1
}
Write-Host "  Found: $installerOutput"

# ---------------------------------------------------------------------------
# Step 8 – Prompt for changelog
# ---------------------------------------------------------------------------

Write-Host ''
$changelog = Read-Host "Enter release notes / changelog for $tag"

# ---------------------------------------------------------------------------
# Step 9 – Create GitHub Release
# ---------------------------------------------------------------------------

Write-Step "Creating GitHub Release: $tag on $($cfg.GitHubRepo)"
& gh release create $tag $installerOutput `
    --repo $cfg.GitHubRepo `
    --title $tag `
    --notes $changelog
if ($LASTEXITCODE -ne 0) { Write-Error "gh release create failed."; exit 1 }

Write-Host ''
Write-Host "  Release $tag published successfully!" -ForegroundColor Green
