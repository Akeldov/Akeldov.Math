[CmdletBinding()]
param(
    [switch] $Serve,

    [ValidateRange(1, 65535)]
    [int] $Port = 8080
)

$ErrorActionPreference = 'Stop'

function Get-RelativeSitePath {
    param(
        [Parameter(Mandatory)]
        [string] $Root,

        [Parameter(Mandatory)]
        [string] $Path
    )

    $relativePath = $Path.Substring($Root.Length)
    $relativePath = $relativePath.TrimStart(
        [char][System.IO.Path]::DirectorySeparatorChar)
    return $relativePath.Replace(
        [System.IO.Path]::DirectorySeparatorChar,
        '/')
}

function Set-PageSeoMetadata {
    param(
        [Parameter(Mandatory)]
        [string] $Path,

        [Parameter(Mandatory)]
        [string] $CanonicalUrl,

        [string] $EnglishUrl,

        [string] $RussianUrl
    )

    $content = Get-Content -LiteralPath $Path -Raw -Encoding UTF8
    $lines = @(
        "      <link rel=`"canonical`" href=`"$CanonicalUrl`">"
    )

    if ($EnglishUrl -and $RussianUrl) {
        $lines += "      <link rel=`"alternate`" hreflang=`"en`" href=`"$EnglishUrl`">"
        $lines += "      <link rel=`"alternate`" hreflang=`"ru`" href=`"$RussianUrl`">"
        $lines += "      <link rel=`"alternate`" hreflang=`"x-default`" href=`"$EnglishUrl`">"
    }

    $content = $content.Replace(
        '  </head>',
        "$($lines -join [Environment]::NewLine)$([Environment]::NewLine)  </head>")
    [System.IO.File]::WriteAllText(
        $Path,
        $content,
        [System.Text.UTF8Encoding]::new($false))
}

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
$englishRoot = Join-Path $siteRoot 'en'
$russianRoot = Join-Path $siteRoot 'ru'
$russianSourceRoot = Join-Path $PSScriptRoot 'ru'
$russianNavigationFile = Join-Path $russianSourceRoot 'navigation.json'
$russianNavigation = Get-Content -LiteralPath $russianNavigationFile -Raw -Encoding UTF8 |
    ConvertFrom-Json
$russianOverrides = @{}

if (Test-Path -LiteralPath $englishRoot) {
    Remove-Item -LiteralPath $englishRoot -Recurse -Force
}

New-Item -ItemType Directory -Path $englishRoot | Out-Null
Get-ChildItem -LiteralPath $siteRoot |
    Where-Object Name -notin @(
        'api',
        'en',
        'ru',
        'library-navigation.json',
        'robots.txt',
        'sitemap.xml') |
    ForEach-Object {
        Copy-Item -LiteralPath $_.FullName -Destination $englishRoot -Recurse -Force
    }

$englishTocHtml = Join-Path $englishRoot 'toc.html'
$englishTocJson = Join-Path $englishRoot 'toc.json'
$englishSearchIndex = Join-Path $englishRoot 'index.json'

if (Test-Path -LiteralPath $englishTocHtml) {
    $content = Get-Content -LiteralPath $englishTocHtml -Raw -Encoding UTF8
    $content = $content.Replace('href="api/', 'href="../api/')
    [System.IO.File]::WriteAllText(
        $englishTocHtml,
        $content,
        [System.Text.UTF8Encoding]::new($false))
}

foreach ($jsonFile in @($englishTocJson, $englishSearchIndex)) {
    if (Test-Path -LiteralPath $jsonFile) {
        $content = Get-Content -LiteralPath $jsonFile -Raw -Encoding UTF8
        $content = $content.Replace('"href":"api/', '"href":"../api/')
        [System.IO.File]::WriteAllText(
            $jsonFile,
            $content,
            [System.Text.UTF8Encoding]::new($false))
    }
}

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
    Where-Object Name -notin @(
        'api',
        'en',
        'ru',
        'library-navigation.json',
        'robots.txt',
        'sitemap.xml') |
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

$docfxConfig = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'docfx.json') `
    -Raw -Encoding UTF8 |
    ConvertFrom-Json
$siteBaseUrl = $docfxConfig.build.sitemap.baseUrl.TrimEnd('/') + '/'
$apiRoot = Join-Path $siteRoot 'api'
$sitemapEntries = @()

Get-ChildItem -LiteralPath $englishRoot -Recurse -Filter '*.html' -File |
    ForEach-Object {
        $relativePath = Get-RelativeSitePath -Root $englishRoot -Path $_.FullName
        $content = Get-Content -LiteralPath $_.FullName -Raw -Encoding UTF8

        if ($content.Contains('<meta name="robots" content="noindex">')) {
            return
        }

        $englishUrl = "$siteBaseUrl" + "en/$relativePath"
        $russianUrl = if ($russianOverrides.ContainsKey(
            $relativePath.Replace('/', [System.IO.Path]::DirectorySeparatorChar))) {
            "$siteBaseUrl" + "ru/$relativePath"
        } else {
            $null
        }

        Set-PageSeoMetadata `
            -Path $_.FullName `
            -CanonicalUrl $englishUrl `
            -EnglishUrl $englishUrl `
            -RussianUrl $russianUrl
        $sitemapEntries += [pscustomobject]@{
            Url = $englishUrl
            EnglishUrl = $englishUrl
            RussianUrl = $russianUrl
        }
    }

Get-ChildItem -LiteralPath $russianRoot -Recurse -Filter '*.html' -File |
    ForEach-Object {
        $relativePath = Get-RelativeSitePath -Root $russianRoot -Path $_.FullName
        $overrideKey = $relativePath.Replace(
            '/',
            [System.IO.Path]::DirectorySeparatorChar)
        $englishUrl = "$siteBaseUrl" + "en/$relativePath"

        if ($russianOverrides.ContainsKey($overrideKey)) {
            $russianUrl = "$siteBaseUrl" + "ru/$relativePath"
            Set-PageSeoMetadata `
                -Path $_.FullName `
                -CanonicalUrl $russianUrl `
                -EnglishUrl $englishUrl `
                -RussianUrl $russianUrl
            $sitemapEntries += [pscustomobject]@{
                Url = $russianUrl
                EnglishUrl = $englishUrl
                RussianUrl = $russianUrl
            }
        } else {
            Set-PageSeoMetadata -Path $_.FullName -CanonicalUrl $englishUrl
        }
    }

Get-ChildItem -LiteralPath $apiRoot -Recurse -Filter '*.html' -File |
    ForEach-Object {
        $relativePath = Get-RelativeSitePath -Root $apiRoot -Path $_.FullName
        $content = Get-Content -LiteralPath $_.FullName -Raw -Encoding UTF8

        if ($content.Contains('<meta name="robots" content="noindex">')) {
            return
        }

        $canonicalUrl = "$siteBaseUrl" + "api/$relativePath"
        Set-PageSeoMetadata -Path $_.FullName -CanonicalUrl $canonicalUrl
        $sitemapEntries += [pscustomobject]@{
            Url = $canonicalUrl
            EnglishUrl = $null
            RussianUrl = $null
        }
    }

Get-ChildItem -LiteralPath $siteRoot -Recurse -Filter '*.html' -File |
    Where-Object {
        -not $_.FullName.StartsWith(
            "$englishRoot$([System.IO.Path]::DirectorySeparatorChar)") -and
        -not $_.FullName.StartsWith(
            "$russianRoot$([System.IO.Path]::DirectorySeparatorChar)") -and
        -not $_.FullName.StartsWith(
            "$apiRoot$([System.IO.Path]::DirectorySeparatorChar)")
    } |
    ForEach-Object {
        $content = Get-Content -LiteralPath $_.FullName -Raw -Encoding UTF8

        if (-not $content.Contains('<meta name="robots" content="noindex">')) {
            $relativePath = Get-RelativeSitePath -Root $siteRoot -Path $_.FullName
            Set-PageSeoMetadata `
                -Path $_.FullName `
                -CanonicalUrl ("$siteBaseUrl" + "en/$relativePath")
        }
    }

$sitemapLines = @(
    '<?xml version="1.0" encoding="utf-8"?>',
    '<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9"',
    '        xmlns:xhtml="http://www.w3.org/1999/xhtml">'
)

foreach ($entry in $sitemapEntries | Sort-Object Url -Unique) {
    $sitemapLines += '  <url>'
    $sitemapLines += "    <loc>$([System.Security.SecurityElement]::Escape($entry.Url))</loc>"

    if ($entry.EnglishUrl -and $entry.RussianUrl) {
        $escapedEnglishUrl =
            [System.Security.SecurityElement]::Escape($entry.EnglishUrl)
        $escapedRussianUrl =
            [System.Security.SecurityElement]::Escape($entry.RussianUrl)
        $sitemapLines +=
            "    <xhtml:link rel=`"alternate`" hreflang=`"en`" href=`"$escapedEnglishUrl`" />"
        $sitemapLines +=
            "    <xhtml:link rel=`"alternate`" hreflang=`"ru`" href=`"$escapedRussianUrl`" />"
        $sitemapLines +=
            "    <xhtml:link rel=`"alternate`" hreflang=`"x-default`" href=`"$escapedEnglishUrl`" />"
    }

    $sitemapLines += '  </url>'
}

$sitemapLines += '</urlset>'
[System.IO.File]::WriteAllLines(
    (Join-Path $siteRoot 'sitemap.xml'),
    $sitemapLines,
    [System.Text.UTF8Encoding]::new($false))

if ($Serve) {
    & $docfx serve (Join-Path $PSScriptRoot '_site') --port $Port
    exit $LASTEXITCODE
}
