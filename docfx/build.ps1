[CmdletBinding()]
param(
    [switch] $Serve,

    [ValidateRange(1, 65535)]
    [int] $Port = 8080
)

$ErrorActionPreference = 'Stop'

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$localDocfx = Join-Path $repositoryRoot '.tmp\docfx-tool\docfx.exe'

if (Test-Path -LiteralPath $localDocfx) {
    $docfx = $localDocfx
} else {
    $docfxCommand = Get-Command docfx -ErrorAction SilentlyContinue
    if (-not $docfxCommand) {
        throw 'DocFX is not installed. Run: dotnet tool install --global docfx'
    }

    $docfx = $docfxCommand.Source
}

& $docfx (Join-Path $PSScriptRoot 'docfx.json')
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$siteRoot = Join-Path $PSScriptRoot '_site\Akeldov.Math'
$russianRoot = Join-Path $siteRoot 'ru'
$russianSourceRoot = Join-Path $PSScriptRoot 'ru'
$russianNavigationFile = Join-Path $russianSourceRoot 'navigation.json'
$russianNavigation = Get-Content -LiteralPath $russianNavigationFile -Raw -Encoding UTF8 |
    ConvertFrom-Json
$russianOverrides = @{}

if (Test-Path -LiteralPath $russianRoot) {
    Get-ChildItem -LiteralPath $russianSourceRoot -Recurse -Filter '*.md' -File |
        ForEach-Object {
            $relativePath = $_.FullName.Substring($russianSourceRoot.Length)
            $relativePath = $relativePath.TrimStart(
                [char][System.IO.Path]::DirectorySeparatorChar)
            $relativePath = [System.IO.Path]::ChangeExtension($relativePath, '.html')
            $generatedPage = Join-Path $russianRoot $relativePath

            if (Test-Path -LiteralPath $generatedPage) {
                $russianOverrides[$relativePath] =
                    [System.IO.File]::ReadAllBytes($generatedPage)
            }
        }

    Remove-Item -LiteralPath $russianRoot -Recurse -Force
}

New-Item -ItemType Directory -Path $russianRoot | Out-Null
Get-ChildItem -LiteralPath $siteRoot |
    Where-Object Name -notin @('api', 'ru') |
    ForEach-Object {
        Copy-Item -LiteralPath $_.FullName -Destination $russianRoot -Recurse -Force
    }

$russianTocHtml = Join-Path $russianRoot 'toc.html'
$russianTocJson = Join-Path $russianRoot 'toc.json'
$russianSearchIndex = Join-Path $russianRoot 'index.json'

if (Test-Path -LiteralPath $russianTocHtml) {
    $content = Get-Content -LiteralPath $russianTocHtml -Raw -Encoding UTF8
    $content = $content.Replace('href="api/', 'href="../api/')
    $content = $content.Replace('>Home<', ">$($russianNavigation.home)<")
    $content = $content.Replace('>Libraries<', ">$($russianNavigation.libraries)<")
    $content = $content.Replace(
        '>API References<',
        ">$($russianNavigation.apiReference)<")
    $content = $content.Replace('>About<', ">$($russianNavigation.about)<")
    [System.IO.File]::WriteAllText(
        $russianTocHtml,
        $content,
        [System.Text.UTF8Encoding]::new($false))
}

foreach ($jsonFile in @($russianTocJson, $russianSearchIndex)) {
    if (Test-Path -LiteralPath $jsonFile) {
        $content = Get-Content -LiteralPath $jsonFile -Raw -Encoding UTF8
        $content = $content.Replace('"href":"api/', '"href":"../api/')
        $content = $content.Replace(
            '"name":"Home"',
            "`"name`":`"$($russianNavigation.home)`"")
        $content = $content.Replace(
            '"name":"Libraries"',
            "`"name`":`"$($russianNavigation.libraries)`"")
        $content = $content.Replace(
            '"name":"API References"',
            "`"name`":`"$($russianNavigation.apiReference)`"")
        $content = $content.Replace(
            '"name":"About"',
            "`"name`":`"$($russianNavigation.about)`"")
        [System.IO.File]::WriteAllText(
            $jsonFile,
            $content,
            [System.Text.UTF8Encoding]::new($false))
    }
}

foreach ($override in $russianOverrides.GetEnumerator()) {
    $destination = Join-Path $russianRoot $override.Key
    $destinationDirectory = Split-Path -Parent $destination

    if (-not (Test-Path -LiteralPath $destinationDirectory)) {
        New-Item -ItemType Directory -Path $destinationDirectory | Out-Null
    }

    [System.IO.File]::WriteAllBytes($destination, $override.Value)
}

$russianIndex = Join-Path $russianRoot 'index.html'
if (Test-Path -LiteralPath $russianIndex) {
    $content = Get-Content -LiteralPath $russianIndex -Raw -Encoding UTF8
    $content = $content.Replace('content="../toc.html"', 'content="toc.html"')
    $content = $content.Replace('href="../Libraries/', 'href="Libraries/')
    [System.IO.File]::WriteAllText(
        $russianIndex,
        $content,
        [System.Text.UTF8Encoding]::new($false))
}

if ($Serve) {
    & $docfx serve (Join-Path $PSScriptRoot '_site') --port $Port
    exit $LASTEXITCODE
}
