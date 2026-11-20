# Builds the BIMPlugins.MaterialDesign package from a release of MaterialDesignInXamlToolkit:
#   ./build.ps1            -> artifacts/BIMPlugins.MaterialDesign.<version>.nupkg
# The release's sources are cloned into .work/ (not kept in this repository), renamed and themed by tools/rename.py,
# built for .NET Framework 4.6.2 (Revit 2019-2024) and .NET 8 (Revit 2025+), and packed by BIMPlugins.MaterialDesign.nuspec.
param(
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'

# The MaterialDesignInXamlToolkit release the package is built from.
$MaterialDesignVersion = '5.3.2'

# The package version: the release's, with a fourth number for BIMPlugins' own changes to the same release (5.3.2.1).
# The tag that publishes it is v<PackageVersion>.
$PackageVersion = '5.3.2.1'

$root = $PSScriptRoot
$work = Join-Path $root '.work'
$sources = Join-Path $work 'MaterialDesignInXamlToolkit'
$artifacts = Join-Path $root 'artifacts'

function Invoke-Checked([scriptblock]$command) {
    & $command
    if ($LASTEXITCODE -ne 0) { throw "Failed with exit code ${LASTEXITCODE}: $command" }
}

if (Test-Path $sources) { Remove-Item $sources -Recurse -Force }
New-Item -ItemType Directory -Force $work, $artifacts | Out-Null

Invoke-Checked { git clone --quiet --depth 1 --branch "v$MaterialDesignVersion" -c advice.detachedHead=false `
    https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit.git $sources }
Invoke-Checked { python (Join-Path $root 'tools/rename.py') $sources (Join-Path $root 'key/BIMPlugins.MaterialDesign.snk') }

$project = Join-Path $sources 'src/MaterialDesignThemes.Wpf/MaterialDesignThemes.Wpf.csproj'
foreach ($framework in 'net462', 'net8.0-windows') {
    Invoke-Checked { dotnet build $project -c $Configuration -f $framework -nologo -v q `
        "-p:MDIXVersion=$MaterialDesignVersion" "-p:MDIXColorsVersion=$MaterialDesignVersion" }
}

$output = Join-Path $sources "src/MaterialDesignThemes.Wpf/bin/$Configuration"

# The commit of this repository the package is built from, shown on nuget.org.
$commit = (git -C $root rev-parse HEAD).Trim()

Invoke-Checked { dotnet pack (Join-Path $root 'pack/Pack.csproj') -nologo -v q -o $artifacts `
    "-p:PackageVersion=$PackageVersion" "-p:BuildOutput=$output" "-p:MaterialDesignVersion=$MaterialDesignVersion" `
    "-p:RepositoryCommit=$commit" }

Write-Host "Packed BIMPlugins.MaterialDesign $PackageVersion to $artifacts"
