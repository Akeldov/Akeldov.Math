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

function Get-RussianOutputRelativePath {
    param(
        [Parameter(Mandatory)]
        [string] $Root,

        [Parameter(Mandatory)]
        [string] $Path,

        [string[]] $VersionedLibraries,

        [string] $OutputPrefix
    )

    $relativePath = $Path.Substring($Root.Length)
    $relativePath = $relativePath.TrimStart(
        [char][System.IO.Path]::DirectorySeparatorChar)
    $separator = [System.IO.Path]::DirectorySeparatorChar

    if (-not [string]::IsNullOrWhiteSpace($OutputPrefix)) {
        return Join-Path $OutputPrefix $relativePath
    }

    foreach ($library in $VersionedLibraries) {
        $libraryPrefix = "$library$separator"
        if ($relativePath.StartsWith(
            $libraryPrefix,
            [System.StringComparison]::OrdinalIgnoreCase)) {
            $libraryPath = Join-Path $library 'latest'
            return Join-Path $libraryPath $relativePath.Substring(
                $libraryPrefix.Length)
        }
    }

    return $relativePath
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

function Merge-SearchIndexEntries {
    param(
        [Parameter(Mandatory)]
        [string] $CurrentPath,

        [Parameter(Mandatory)]
        [string] $FragmentPath,

        [Parameter(Mandatory)]
        [string] $PathPrefix
    )

    if (-not (Test-Path -LiteralPath $CurrentPath) -or
        -not (Test-Path -LiteralPath $FragmentPath)) {
        throw 'A search index required for version merging is missing.'
    }

    $currentIndex = Get-Content -LiteralPath $CurrentPath -Raw -Encoding UTF8 |
        ConvertFrom-Json
    $fragmentIndex = Get-Content -LiteralPath $FragmentPath -Raw -Encoding UTF8 |
        ConvertFrom-Json
    $mergedIndex = [ordered]@{}

    foreach ($property in $currentIndex.PSObject.Properties) {
        $mergedIndex[$property.Name] = $property.Value
    }

    foreach ($property in $fragmentIndex.PSObject.Properties) {
        if ($property.Name.StartsWith(
            $PathPrefix,
            [System.StringComparison]::OrdinalIgnoreCase)) {
            $mergedIndex[$property.Name] = $property.Value
        }
    }

    $content = $mergedIndex | ConvertTo-Json -Depth 10
    [System.IO.File]::WriteAllText(
        $CurrentPath,
        $content,
        [System.Text.UTF8Encoding]::new($false))
}

function Add-Spatial2D08Documentation {
    param(
        [Parameter(Mandatory)]
        [string] $RepositoryRoot,

        [Parameter(Mandatory)]
        [string] $Docfx,

        [Parameter(Mandatory)]
        [string] $SiteRoot,

        [Parameter(Mandatory)]
        [string] $VersionAdapterRoot
    )

    # API versions have overlapping UIDs, so the 0.8.0 articles and API must
    # be rendered together in an independent graph before their static output
    # joins the current site.
    $library = 'Spatial2D'
    $targetVersionPath = '0.8.0'
    $temporaryParent = [System.IO.Path]::GetFullPath(
        (Join-Path $RepositoryRoot '.tmp\docfx-versioned'))
    $fragmentRoot = [System.IO.Path]::GetFullPath(
        (Join-Path $temporaryParent "$library-$targetVersionPath"))
    $versionedSourceRoot = [System.IO.Path]::GetFullPath(
        (Join-Path $VersionAdapterRoot 'source'))
    $packagePath = [System.IO.Path]::GetFullPath(
        (Join-Path $VersionAdapterRoot `
            'source\Akeldov.Math.Spatial2D.0.8.0.nupkg'))
    $packageExtractRoot = Join-Path $fragmentRoot 'package'
    $packageAssembly = Join-Path `
        $packageExtractRoot 'lib\net6.0\Akeldov.Math.Spatial2D.dll'
    $packageDocumentation = Join-Path `
        $packageExtractRoot 'lib\net6.0\Akeldov.Math.Spatial2D.xml'
    $expectedPackageHash =
        '293179161CFEA2D649CCECBD770863E9504D95FF0984F095E187FA9809D8975E'
    $temporaryPrefix = $temporaryParent.TrimEnd(
        [System.IO.Path]::DirectorySeparatorChar) +
        [System.IO.Path]::DirectorySeparatorChar
    $versionedSourcePrefix = $versionedSourceRoot.TrimEnd(
        [System.IO.Path]::DirectorySeparatorChar) +
        [System.IO.Path]::DirectorySeparatorChar

    if (-not $fragmentRoot.StartsWith(
        $temporaryPrefix,
        [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Invalid versioned API output path: $fragmentRoot"
    }

    if (-not $packagePath.StartsWith(
        $versionedSourcePrefix,
        [System.StringComparison]::OrdinalIgnoreCase) -or
        [System.IO.Path]::GetFileName($packagePath) -ne
            'Akeldov.Math.Spatial2D.0.8.0.nupkg') {
        throw "Invalid versioned package path: $packagePath"
    }

    if (-not (Test-Path -LiteralPath $packagePath)) {
        throw "The $library $targetVersionPath package is missing."
    }

    $packageHash = (Get-FileHash -LiteralPath $packagePath -Algorithm SHA256).Hash
    if ($packageHash -ne $expectedPackageHash) {
        throw "The $library $targetVersionPath package hash is invalid."
    }

    New-Item -ItemType Directory -Path $temporaryParent -Force | Out-Null

    try {
        if (Test-Path -LiteralPath $fragmentRoot) {
            Remove-Item -LiteralPath $fragmentRoot -Recurse -Force
        }

        Add-Type -AssemblyName System.IO.Compression.FileSystem
        [System.IO.Compression.ZipFile]::ExtractToDirectory(
            $packagePath,
            $packageExtractRoot)

        if (-not (Test-Path -LiteralPath $packageAssembly) -or
            -not (Test-Path -LiteralPath $packageDocumentation)) {
            throw "The $library $targetVersionPath package API files are missing."
        }

        & $Docfx (Join-Path $VersionAdapterRoot 'api.docfx.json')
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to build the $library $targetVersionPath documentation."
        }

        $fragmentSiteRoot = Join-Path $fragmentRoot 'site\Akeldov.Math'
        $outputPaths = @(
            "$library\$targetVersionPath",
            "ru\$library\$targetVersionPath",
            "api\$library\$targetVersionPath"
        )

        foreach ($outputPath in $outputPaths) {
            $sourceRoot = Join-Path $fragmentSiteRoot $outputPath
            $destinationRoot = Join-Path $SiteRoot $outputPath

            if (-not (Test-Path -LiteralPath $sourceRoot)) {
                throw "The $library $targetVersionPath output is missing: $outputPath"
            }

            if (Test-Path -LiteralPath $destinationRoot) {
                throw "The $library $targetVersionPath output already exists: $outputPath"
            }

            $destinationParent = Split-Path -Parent $destinationRoot
            New-Item -ItemType Directory -Path $destinationParent -Force |
                Out-Null
            Copy-Item -LiteralPath $sourceRoot `
                -Destination $destinationRoot -Recurse

            Merge-SearchIndexEntries `
                -CurrentPath (Join-Path $SiteRoot 'index.json') `
                -FragmentPath (Join-Path $fragmentSiteRoot 'index.json') `
                -PathPrefix ($outputPath.Replace(
                    [System.IO.Path]::DirectorySeparatorChar,
                    '/') + '/')
        }
    } finally {
        if (Test-Path -LiteralPath $fragmentRoot) {
            Remove-Item -LiteralPath $fragmentRoot -Recurse -Force
        }

    }
}

function Update-LanguageBranchRelativeLinks {
    param(
        [Parameter(Mandatory)]
        [string] $LanguageRoot
    )

    Get-ChildItem -LiteralPath $LanguageRoot -Recurse -Filter '*.html' -File |
        ForEach-Object {
            $content = Get-Content -LiteralPath $_.FullName -Raw -Encoding UTF8
            $updatedContent = [System.Text.RegularExpressions.Regex]::Replace(
                $content,
                '(?<attribute>(?:href|src)=")(?<prefix>(?:\./)?(?:\.\./)*)public/',
                '${attribute}${prefix}../public/')
            $updatedContent = [System.Text.RegularExpressions.Regex]::Replace(
                $updatedContent,
                '(?<attribute>href=")(?<prefix>(?:\./)?(?:\.\./)*)api/',
                '${attribute}${prefix}../api/')
            $updatedContent = [System.Text.RegularExpressions.Regex]::Replace(
                $updatedContent,
                '(?<attribute><meta name="docfx:rel" content=")(?<relative>[^"]*)"',
                '${attribute}../${relative}"')

            if ($updatedContent -ne $content) {
                [System.IO.File]::WriteAllText(
                    $_.FullName,
                    $updatedContent,
                    [System.Text.UTF8Encoding]::new($false))
            }
        }
}

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$localDocfx = Join-Path $repositoryRoot '.tmp\docfx-tool\docfx.exe'
$siteRoot = Join-Path $PSScriptRoot '_site\Akeldov.Math'

if (Test-Path -LiteralPath $localDocfx) {
    $docfx = $localDocfx
} else {
    $docfxCommand = Get-Command docfx -ErrorAction SilentlyContinue
    if (-not $docfxCommand) {
        throw 'DocFX is not installed. Run: dotnet tool install --global docfx'
    }

    $docfx = $docfxCommand.Source
}

if (Test-Path -LiteralPath $siteRoot) {
    Remove-Item -LiteralPath $siteRoot -Recurse -Force
}

& $docfx (Join-Path $PSScriptRoot 'docfx.json')
$docfxExitCode = $LASTEXITCODE

if ($docfxExitCode -ne 0) {
    exit $docfxExitCode
}

Add-Spatial2D08Documentation `
    -RepositoryRoot $repositoryRoot `
    -Docfx $docfx `
    -SiteRoot $siteRoot `
    -VersionAdapterRoot (
        Join-Path $PSScriptRoot 'versioned\Spatial2D\0.8.0')

$englishRoot = Join-Path $siteRoot 'en'
$russianRoot = Join-Path $siteRoot 'ru'
$russianSourceRoot = Join-Path $PSScriptRoot 'ru'
$spatial2D08RussianSourceRoot = Join-Path `
    $PSScriptRoot 'versioned\Spatial2D\0.8.0\ru'
$russianSourceMappings = @(
    [pscustomobject]@{
        Root = $russianSourceRoot
        OutputPrefix = $null
    },
    [pscustomobject]@{
        Root = $spatial2D08RussianSourceRoot
        OutputPrefix = Join-Path 'Spatial2D' '0.8.0'
    }
)
$russianNavigationFile = Join-Path $russianSourceRoot 'navigation.json'
$russianNavigation = Get-Content -LiteralPath $russianNavigationFile -Raw -Encoding UTF8 |
    ConvertFrom-Json
$russianVersionedLibraries = (
    Get-Content -LiteralPath (Join-Path $PSScriptRoot 'versions.json') `
        -Raw -Encoding UTF8 |
        ConvertFrom-Json
).libraries
$russianOverrides = @{}
$russianOutputOverrides = @{}

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
        'manifest.json',
        'public',
        'robots.txt',
        'sitemap.xml',
        'xrefmap.yml') |
    ForEach-Object {
        Copy-Item -LiteralPath $_.FullName -Destination $englishRoot -Recurse -Force
    }

$englishTocJson = Join-Path $englishRoot 'toc.json'
$englishSearchIndex = Join-Path $englishRoot 'index.json'

foreach ($jsonFile in @($englishTocJson, $englishSearchIndex)) {
    if (Test-Path -LiteralPath $jsonFile) {
        $content = Get-Content -LiteralPath $jsonFile -Raw -Encoding UTF8
        $content = [System.Text.RegularExpressions.Regex]::Replace(
            $content,
            '("href"\s*:\s*")api/',
            '${1}../api/')
        [System.IO.File]::WriteAllText(
            $jsonFile,
            $content,
            [System.Text.UTF8Encoding]::new($false))
    }
}

if (Test-Path -LiteralPath $russianRoot) {
    foreach ($sourceMapping in $russianSourceMappings) {
        if (-not (Test-Path -LiteralPath $sourceMapping.Root)) {
            continue
        }

        Get-ChildItem -LiteralPath $sourceMapping.Root -Recurse -Filter '*.md' -File |
            ForEach-Object {
                $relativePath = Get-RussianOutputRelativePath `
                    -Root $sourceMapping.Root `
                    -Path $_.FullName `
                    -VersionedLibraries $russianVersionedLibraries `
                    -OutputPrefix $sourceMapping.OutputPrefix
                $relativePath = [System.IO.Path]::ChangeExtension(
                    $relativePath,
                    '.html')
                $generatedPage = Join-Path $russianRoot $relativePath

                if (Test-Path -LiteralPath $generatedPage) {
                    $generatedBytes = [System.IO.File]::ReadAllBytes($generatedPage)
                    $russianOverrides[$relativePath] = $generatedBytes
                    $russianOutputOverrides[$relativePath] = $generatedBytes
                }
            }

        Get-ChildItem -LiteralPath $sourceMapping.Root -Recurse -Filter 'toc.yml' -File |
            ForEach-Object {
                $relativeTocPath = Get-RussianOutputRelativePath `
                    -Root $sourceMapping.Root `
                    -Path $_.FullName `
                    -VersionedLibraries $russianVersionedLibraries `
                    -OutputPrefix $sourceMapping.OutputPrefix
                $relativeDirectory = Split-Path -Parent $relativeTocPath

                foreach ($outputName in @('toc.html', 'toc.json')) {
                    $relativePath = if ($relativeDirectory) {
                        Join-Path $relativeDirectory $outputName
                    } else {
                        $outputName
                    }
                    $generatedToc = Join-Path $russianRoot $relativePath

                    if (Test-Path -LiteralPath $generatedToc) {
                        $russianOutputOverrides[$relativePath] =
                            [System.IO.File]::ReadAllBytes($generatedToc)
                    }
                }
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
        'manifest.json',
        'public',
        'robots.txt',
        'sitemap.xml',
        'xrefmap.yml') |
    ForEach-Object {
        Copy-Item -LiteralPath $_.FullName -Destination $russianRoot -Recurse -Force
    }

$russianTocHtml = Join-Path $russianRoot 'toc.html'
$russianTocJson = Join-Path $russianRoot 'toc.json'
$russianSearchIndex = Join-Path $russianRoot 'index.json'

if (Test-Path -LiteralPath $russianTocHtml) {
    $content = Get-Content -LiteralPath $russianTocHtml -Raw -Encoding UTF8
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
        $content = [System.Text.RegularExpressions.Regex]::Replace(
            $content,
            '("href"\s*:\s*")api/',
            '${1}../api/')
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

Update-LanguageBranchRelativeLinks -LanguageRoot $russianRoot

foreach ($override in $russianOutputOverrides.GetEnumerator()) {
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

Update-LanguageBranchRelativeLinks -LanguageRoot $englishRoot

$publicRoot = Join-Path $siteRoot 'public'
Get-ChildItem -LiteralPath $publicRoot -Recurse -Filter '*.map' -File |
    Remove-Item -Force

Get-ChildItem -LiteralPath $publicRoot -Recurse -File |
    Where-Object Extension -in @('.css', '.js') |
    ForEach-Object {
        $content = Get-Content -LiteralPath $_.FullName -Raw -Encoding UTF8
        $updatedContent = [System.Text.RegularExpressions.Regex]::Replace(
            $content,
            '(?m)^\s*//# sourceMappingURL=.*\r?\n?',
            '')
        $updatedContent = [System.Text.RegularExpressions.Regex]::Replace(
            $updatedContent,
            '(?m)^\s*/\*# sourceMappingURL=.*\*/\s*\r?\n?',
            '')

        if ($updatedContent -ne $content) {
            [System.IO.File]::WriteAllText(
                $_.FullName,
                $updatedContent,
                [System.Text.UTF8Encoding]::new($false))
        }
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
