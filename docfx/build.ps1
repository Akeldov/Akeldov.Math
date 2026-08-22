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

function New-MergedArticleSource {
    param(
        [Parameter(Mandatory)]
        [string] $RepositoryRoot,

        [Parameter(Mandatory)]
        [string] $BaseRoot,

        [Parameter(Mandatory)]
        [string] $OverrideRoot,

        [Parameter(Mandatory)]
        [string] $StageRoot
    )

    $temporaryRoot = [System.IO.Path]::GetFullPath(
        (Join-Path $RepositoryRoot '.tmp'))
    $temporaryPrefix = $temporaryRoot.TrimEnd(
        [System.IO.Path]::DirectorySeparatorChar) +
        [System.IO.Path]::DirectorySeparatorChar
    $stageFullPath = [System.IO.Path]::GetFullPath($StageRoot)

    if (-not $stageFullPath.StartsWith(
            $temporaryPrefix,
            [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "The article staging directory is outside .tmp: $stageFullPath"
    }

    if (Test-Path -LiteralPath $stageFullPath) {
        Remove-Item -LiteralPath $stageFullPath -Recurse -Force
    }

    New-Item -ItemType Directory -Path $stageFullPath -Force | Out-Null

    foreach ($language in @('en', 'ru')) {
        $languageBaseRoot = Join-Path $BaseRoot $language
        $languageOverrideRoot = Join-Path $OverrideRoot $language

        if (-not (Test-Path -LiteralPath $languageBaseRoot)) {
            throw "The article source is missing: $languageBaseRoot"
        }

        Copy-Item -LiteralPath $languageBaseRoot `
            -Destination $stageFullPath -Recurse

        if (-not (Test-Path -LiteralPath $languageOverrideRoot)) {
            continue
        }

        $languageStageRoot = Join-Path $stageFullPath $language
        Get-ChildItem -LiteralPath $languageOverrideRoot -Recurse -File |
            ForEach-Object {
                $relativePath = $_.FullName.Substring(
                    $languageOverrideRoot.Length).TrimStart(
                        [char][System.IO.Path]::DirectorySeparatorChar)
                $destination = Join-Path $languageStageRoot $relativePath
                $destinationDirectory = Split-Path -Parent $destination
                New-Item -ItemType Directory `
                    -Path $destinationDirectory -Force | Out-Null
                Copy-Item -LiteralPath $_.FullName `
                    -Destination $destination -Force
            }
    }
}

function Update-MergedArticleContributionLinks {
    param(
        [Parameter(Mandatory)]
        [string] $RepositoryRoot,

        [Parameter(Mandatory)]
        [string] $SiteRoot,

        [Parameter(Mandatory)]
        [string] $BaseRoot,

        [Parameter(Mandatory)]
        [string] $OverrideRoot,

        [Parameter(Mandatory)]
        [string] $StageRoot,

        [Parameter(Mandatory)]
        [string] $Library,

        [Parameter(Mandatory)]
        [string] $VersionPath
    )

    foreach ($language in @('en', 'ru')) {
        $outputRoot = if ($language -eq 'ru') {
            Join-Path $SiteRoot "ru\$Library\$VersionPath"
        } else {
            Join-Path $SiteRoot "$Library\$VersionPath"
        }
        $languageBaseRoot = Join-Path $BaseRoot $language
        $languageOverrideRoot = Join-Path $OverrideRoot $language
        $languageStageRoot = Join-Path $StageRoot $language

        Get-ChildItem -LiteralPath $outputRoot -Recurse -Filter '*.html' -File |
            ForEach-Object {
                $relativeArticlePath = [System.IO.Path]::ChangeExtension(
                    (Get-RelativeSitePath -Root $outputRoot -Path $_.FullName),
                    '.md')
                $overridePath = Join-Path `
                    $languageOverrideRoot $relativeArticlePath
                $sourcePath = if (Test-Path -LiteralPath $overridePath) {
                    $overridePath
                } else {
                    Join-Path $languageBaseRoot $relativeArticlePath
                }

                if (-not (Test-Path -LiteralPath $sourcePath)) {
                    return
                }

                $stagedPath = Join-Path $languageStageRoot $relativeArticlePath
                $stagedRepositoryPath = Get-RelativeSitePath `
                    -Root $RepositoryRoot -Path $stagedPath
                $sourceRepositoryPath = Get-RelativeSitePath `
                    -Root $RepositoryRoot -Path $sourcePath
                $content = Get-Content `
                    -LiteralPath $_.FullName -Raw -Encoding UTF8
                $updatedContent = $content.Replace(
                    $stagedRepositoryPath,
                    $sourceRepositoryPath)

                if ($updatedContent -ne $content) {
                    [System.IO.File]::WriteAllText(
                        $_.FullName,
                        $updatedContent,
                        [System.Text.UTF8Encoding]::new($false))
                }
            }
    }
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

function Test-PageHasNoIndex {
    param(
        [Parameter(Mandatory)]
        [string] $Path
    )

    $content = Get-Content -LiteralPath $Path -Raw -Encoding UTF8
    return [System.Text.RegularExpressions.Regex]::IsMatch(
        $content,
        '<meta\b(?=[^>]*\bname=["'']robots["''])(?=[^>]*\bcontent=["''][^"'']*\bnoindex\b)[^>]*>',
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
}

function Test-IsP0NonIndexableSitePath {
    param(
        [Parameter(Mandatory)]
        [string] $RelativePath
    )

    $pathSegments = $RelativePath.Split('/')
    return $pathSegments -contains 'upcoming' -or
        $pathSegments[-1].Equals(
            'toc.html',
            [System.StringComparison]::OrdinalIgnoreCase)
}

function Test-PageHasHeadingOnlyArticle {
    param(
        [Parameter(Mandatory)]
        [string] $Path
    )

    $content = Get-Content -LiteralPath $Path -Raw -Encoding UTF8
    $articleMatch = [System.Text.RegularExpressions.Regex]::Match(
        $content,
        '<article\b[^>]*>(?<body>.*?)</article>',
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor
            [System.Text.RegularExpressions.RegexOptions]::Singleline)

    if (-not $articleMatch.Success) {
        return $false
    }

    $articleBody = [System.Text.RegularExpressions.Regex]::Replace(
        $articleMatch.Groups['body'].Value,
        '<h1\b[^>]*>.*?</h1>',
        '',
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor
            [System.Text.RegularExpressions.RegexOptions]::Singleline)
    $articleBody = [System.Text.RegularExpressions.Regex]::Replace(
        $articleBody,
        '<!--.*?-->',
        '',
        [System.Text.RegularExpressions.RegexOptions]::Singleline)

    if ([System.Text.RegularExpressions.Regex]::IsMatch(
            $articleBody,
            '<(audio|blockquote|canvas|dl|iframe|img|ol|pre|svg|table|ul|video)\b',
            [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)) {
        return $false
    }

    $plainText = [System.Text.RegularExpressions.Regex]::Replace(
        $articleBody,
        '<[^>]+>',
        '')
    $plainText = [System.Net.WebUtility]::HtmlDecode($plainText).Trim()
    return [string]::IsNullOrWhiteSpace($plainText)
}

function Set-PageNoIndexMetadata {
    param(
        [Parameter(Mandatory)]
        [string] $Path
    )

    $content = Get-Content -LiteralPath $Path -Raw -Encoding UTF8
    $robotsPattern = '<meta\b(?=[^>]*\bname=["'']robots["''])[^>]*>'
    $robotsTag = '<meta name="robots" content="noindex, follow">'

    if ([System.Text.RegularExpressions.Regex]::IsMatch(
            $content,
            $robotsPattern,
            [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)) {
        $content = [System.Text.RegularExpressions.Regex]::Replace(
            $content,
            $robotsPattern,
            $robotsTag,
            [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    } else {
        $content = $content.Replace(
            '  </head>',
            "      $robotsTag$([Environment]::NewLine)  </head>")
    }

    [System.IO.File]::WriteAllText(
        $Path,
        $content,
        [System.Text.UTF8Encoding]::new($false))
}

function Set-PageLanguage {
    param(
        [Parameter(Mandatory)]
        [string] $Path,

        [Parameter(Mandatory)]
        [ValidateSet('en', 'ru')]
        [string] $Language
    )

    $content = Get-Content -LiteralPath $Path -Raw -Encoding UTF8
    $htmlTagPattern = '<html\b(?<attributes>[^>]*)>'
    $htmlTagMatch = [System.Text.RegularExpressions.Regex]::Match(
        $content,
        $htmlTagPattern,
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)

    if (-not $htmlTagMatch.Success) {
        return
    }

    $htmlTag = $htmlTagMatch.Value
    if ([System.Text.RegularExpressions.Regex]::IsMatch(
            $htmlTag,
            '\blang=["''][^"'']*["'']',
            [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)) {
        $updatedHtmlTag = [System.Text.RegularExpressions.Regex]::Replace(
            $htmlTag,
            '\blang=["''][^"'']*["'']',
            "lang=`"$Language`"",
            [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    } else {
        $updatedHtmlTag = "<html lang=`"$Language`"$($htmlTagMatch.Groups['attributes'].Value)>"
    }

    if ($updatedHtmlTag -eq $htmlTag) {
        return
    }

    $content = $content.Remove(
        $htmlTagMatch.Index,
        $htmlTagMatch.Length).Insert(
            $htmlTagMatch.Index,
            $updatedHtmlTag)
    [System.IO.File]::WriteAllText(
        $Path,
        $content,
        [System.Text.UTF8Encoding]::new($false))
}

function Get-PageArticleHeading {
    param(
        [Parameter(Mandatory)]
        [string] $Content
    )

    $headingMatch = [System.Text.RegularExpressions.Regex]::Match(
        $Content,
        '<h1\b[^>]*>(?<content>.*?)</h1>',
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor
            [System.Text.RegularExpressions.RegexOptions]::Singleline)

    if (-not $headingMatch.Success) {
        return $null
    }

    $heading = [System.Text.RegularExpressions.Regex]::Replace(
        $headingMatch.Groups['content'].Value,
        '<[^>]+>',
        ' ')
    $heading = [System.Net.WebUtility]::HtmlDecode($heading)
    return [System.Text.RegularExpressions.Regex]::Replace(
        $heading,
        '\s+',
        ' ').Trim()
}

function ConvertTo-SearchSnippetText {
    param(
        [Parameter(Mandatory)]
        [string] $Text,

        [ValidateRange(20, 1000)]
        [int] $MaximumLength = 240
    )

    $textValue = [System.Net.WebUtility]::HtmlDecode($Text)
    $textValue = [System.Text.RegularExpressions.Regex]::Replace(
        $textValue,
        '\s+',
        ' ').Trim()
    $textValue = [System.Text.RegularExpressions.Regex]::Replace(
        $textValue,
        '\s+([,;:!?]|\.(?![A-Za-z0-9]))',
        '$1')

    if ($textValue.Length -le $MaximumLength) {
        return $textValue
    }

    $truncated = $textValue.Substring(0, $MaximumLength - 3)
    $lastSpace = $truncated.LastIndexOf(' ')
    if ($lastSpace -ge [Math]::Floor($MaximumLength * 0.7)) {
        $truncated = $truncated.Substring(0, $lastSpace)
    }

    $truncated = [System.Text.RegularExpressions.Regex]::Replace(
        $truncated.TrimEnd(),
        '[,;:]+$',
        '')
    return $truncated + '...'
}

function Get-PageSearchTitle {
    param(
        [Parameter(Mandatory)]
        [string] $Url,

        [Parameter(Mandatory)]
        [string] $SiteBaseUrl,

        [Parameter(Mandatory)]
        [string] $Heading
    )

    $relativeUrl = $Url.Substring($SiteBaseUrl.Length)
    $segments = $relativeUrl.Split('/')

    if ($segments[0] -eq 'api' -and $segments.Count -ge 4) {
        $library = $segments[1]
        $version = $segments[2]
        $displayName = [System.Text.RegularExpressions.Regex]::Replace(
            $Heading,
            '^(Class|Delegate|Enum|Interface|Namespace|Struct)\s+',
            '')
        $fileName = [System.IO.Path]::GetFileNameWithoutExtension(
            [System.Uri]::UnescapeDataString($segments[-1]))

        if ($Heading -notmatch '^Namespace\s+' -and $fileName -ne 'index') {
            $rootNamespace = "Akeldov.Math.$library"
            $relativeUid = if ($fileName.StartsWith(
                    "$rootNamespace.",
                    [System.StringComparison]::Ordinal)) {
                $fileName.Substring($rootNamespace.Length + 1)
            } else {
                $fileName
            }
            $lastSeparator = $relativeUid.LastIndexOf('.')
            if ($lastSeparator -gt 0) {
                $parentNamespace = $relativeUid.Substring(0, $lastSeparator)
                $displayName = "$displayName ($parentNamespace)"
            }
        }

        return "$displayName - $library API $version | Akeldov.Math"
    }

    if ($segments[0] -in @('en', 'ru') -and $segments.Count -ge 4 -and
        $segments[2] -match '^\d+\.\d+\.\d+') {
        $language = $segments[0]
        $library = $segments[1]
        $version = $segments[2]
        $sectionLabels = @{
            'concepts' = 'Concepts'
            'tutorials' = 'Tutorials'
            'how-to-guides' = 'How-to Guides'
        }
        $section = $segments[3]
        $languageSuffix = if ($language -eq 'ru') { ' (RU)' } else { '' }

        if ($sectionLabels.ContainsKey($section)) {
            return "$Heading - $($sectionLabels[$section]) - $library $version$languageSuffix | Akeldov.Math"
        }

        return "$Heading - $library $version$languageSuffix | Akeldov.Math"
    }

    if ($relativeUrl -eq 'en/index.html') {
        return 'Akeldov.Math - .NET Math Libraries'
    }

    if ($relativeUrl -eq 'ru/index.html') {
        return 'Akeldov.Math - .NET Math Libraries (RU)'
    }

    return "$Heading | Akeldov.Math"
}

function Set-PageSearchMetadata {
    param(
        [Parameter(Mandatory)]
        [string] $Path,

        [Parameter(Mandatory)]
        [string] $Url,

        [Parameter(Mandatory)]
        [string] $SiteBaseUrl
    )

    $content = Get-Content -LiteralPath $Path -Raw -Encoding UTF8
    $heading = Get-PageArticleHeading -Content $content
    if ([string]::IsNullOrWhiteSpace($heading)) {
        throw "The page has no H1 for search metadata: $Path"
    }

    $title = Get-PageSearchTitle `
        -Url $Url `
        -SiteBaseUrl $SiteBaseUrl `
        -Heading $heading
    $encodedTitle = [System.Net.WebUtility]::HtmlEncode($title)
    $content = [System.Text.RegularExpressions.Regex]::Replace(
        $content,
        '<title>.*?</title>',
        "<title>$encodedTitle</title>",
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor
            [System.Text.RegularExpressions.RegexOptions]::Singleline)
    $content = [System.Text.RegularExpressions.Regex]::Replace(
        $content,
        '<meta\b(?=[^>]*\bname=["'']title["''])[^>]*>',
        "<meta name=`"title`" content=`"$encodedTitle`">",
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)

    $descriptionMatch = [System.Text.RegularExpressions.Regex]::Match(
        $content,
        '<meta\b(?=[^>]*\bname=["'']description["''])[^>]*\bcontent=["''](?<content>[^"'']*)["''][^>]*>',
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    $namespaceMatch = [System.Text.RegularExpressions.Regex]::Match(
        $heading,
        '^Namespace\s+(?<namespace>.+)$')
    $apiUrlMatch = [System.Text.RegularExpressions.Regex]::Match(
        $Url,
        '/api/(?<library>[^/]+)/(?<version>[^/]+)/')
    $description = if ($descriptionMatch.Success -and
        -not [string]::IsNullOrWhiteSpace(
            $descriptionMatch.Groups['content'].Value)) {
        $descriptionMatch.Groups['content'].Value
    } elseif ($namespaceMatch.Success -and $apiUrlMatch.Success) {
        "API reference for the $($namespaceMatch.Groups['namespace'].Value) " +
            "namespace in $($apiUrlMatch.Groups['library'].Value) " +
            "$($apiUrlMatch.Groups['version'].Value)."
    } else {
        $articleMatch = [System.Text.RegularExpressions.Regex]::Match(
            $content,
            '<article\b[^>]*>(?<content>.*?)</article>',
            [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor
                [System.Text.RegularExpressions.RegexOptions]::Singleline)
        $paragraphMatches = [System.Text.RegularExpressions.Regex]::Matches(
            $articleMatch.Groups['content'].Value,
            '<p(?:\s[^>]*)?>(?<content>.*?)</p>',
            [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor
                [System.Text.RegularExpressions.RegexOptions]::Singleline)
        $paragraph = $paragraphMatches |
            ForEach-Object {
                [System.Text.RegularExpressions.Regex]::Replace(
                    $_.Groups['content'].Value,
                    '<[^>]+>',
                    ' ')
            } |
            Where-Object {
                -not [string]::IsNullOrWhiteSpace($_) -and $_.Length -ge 20
            } |
            Select-Object -First 1
        $paragraph
    }

    if ([string]::IsNullOrWhiteSpace($description)) {
        throw "The page has no description source: $Path"
    }

    $description = ConvertTo-SearchSnippetText -Text $description
    $encodedDescription = [System.Net.WebUtility]::HtmlEncode($description)
    $descriptionTag = "<meta name=`"description`" content=`"$encodedDescription`">"
    if ($descriptionMatch.Success) {
        $content = $content.Remove(
            $descriptionMatch.Index,
            $descriptionMatch.Length).Insert(
                $descriptionMatch.Index,
                $descriptionTag)
    } else {
        $content = $content.Replace(
            '  </head>',
            "      $descriptionTag$([Environment]::NewLine)  </head>")
    }

    [System.IO.File]::WriteAllText(
        $Path,
        $content,
        [System.Text.UTF8Encoding]::new($false))

    return [pscustomobject]@{
        Title = $title
        Description = $description
    }
}

function ConvertTo-SearchIndexKeywords {
    param(
        [Parameter(Mandatory)]
        [string] $Text,

        [ValidateRange(100, 10000)]
        [int] $MaximumLength = 2000
    )

    $decodedText = [System.Net.WebUtility]::HtmlDecode($Text)
    $matches = [System.Text.RegularExpressions.Regex]::Matches(
        $decodedText,
        '[\p{L}_][\p{L}\p{Nd}_]{2,}')
    $seen = [System.Collections.Generic.HashSet[string]]::new(
        [System.StringComparer]::OrdinalIgnoreCase)
    $keywords = [System.Collections.Generic.List[string]]::new()
    $currentLength = 0

    foreach ($match in $matches) {
        $keyword = $match.Value
        if (-not $seen.Add($keyword)) {
            continue
        }

        $additionalLength = $keyword.Length
        if ($keywords.Count -gt 0) {
            $additionalLength++
        }
        if ($currentLength + $additionalLength -gt $MaximumLength) {
            break
        }

        $keywords.Add($keyword)
        $currentLength += $additionalLength
    }

    return $keywords -join ' '
}

function Write-SearchIndex {
    param(
        [Parameter(Mandatory)]
        [string] $Path,

        [Parameter(Mandatory)]
        [System.Collections.IDictionary] $Index
    )

    $content = $Index | ConvertTo-Json -Depth 5 -Compress
    [System.IO.File]::WriteAllText(
        $Path,
        $content,
        [System.Text.UTF8Encoding]::new($false))
}

function Set-SiteSearchIndexes {
    param(
        [Parameter(Mandatory)]
        [string] $SiteRoot,

        [Parameter(Mandatory)]
        [string] $SiteBaseUrl,

        [Parameter(Mandatory)]
        [object[]] $SitemapEntries
    )

    $sourceIndexPath = Join-Path $SiteRoot 'index.json'
    $sourceIndex = Get-Content -LiteralPath $sourceIndexPath -Raw -Encoding UTF8 |
        ConvertFrom-Json
    $allIndex = [ordered]@{}
    $englishIndex = [ordered]@{}
    $russianIndex = [ordered]@{}
    $scopedIndexes = [ordered]@{}
    $sitemapUrls = [System.Collections.Generic.HashSet[string]]::new(
        [System.StringComparer]::OrdinalIgnoreCase)

    foreach ($entry in $SitemapEntries) {
        [void] $sitemapUrls.Add($entry.Url)
        $relativeUrl = $entry.Url.Substring($SiteBaseUrl.Length)
        $sourceKey = if ($relativeUrl.StartsWith(
                'en/',
                [System.StringComparison]::OrdinalIgnoreCase)) {
            $relativeUrl.Substring(3)
        } else {
            $relativeUrl
        }
        $sourceProperty = $sourceIndex.PSObject.Properties[$sourceKey]
        if ($null -eq $sourceProperty) {
            throw "The canonical page has no source search entry: $($entry.Url)"
        }

        $sourceSummary = [string] $sourceProperty.Value.summary
        $summarySource = if ([string]::IsNullOrWhiteSpace($sourceSummary)) {
            $entry.SearchDescription
        } else {
            $sourceSummary
        }
        $summary = ConvertTo-SearchSnippetText `
            -Text $summarySource `
            -MaximumLength 1000
        $keywords = ConvertTo-SearchIndexKeywords -Text (
            "$($entry.SearchTitle) $relativeUrl $sourceSummary")
        if ([string]::IsNullOrWhiteSpace($keywords)) {
            throw "The page has no search keywords: $($entry.Url)"
        }

        $searchEntry = [ordered]@{
            href = $relativeUrl
            title = $entry.SearchTitle
            summary = $summary
            keywords = $keywords
        }

        if ($relativeUrl.StartsWith(
                'en/',
                [System.StringComparison]::OrdinalIgnoreCase)) {
            $englishIndex[$relativeUrl] = $searchEntry
            $allIndex[$relativeUrl] = $searchEntry
        } elseif ($relativeUrl.StartsWith(
                'ru/',
                [System.StringComparison]::OrdinalIgnoreCase)) {
            $russianIndex[$relativeUrl] = $searchEntry
            $allIndex[$relativeUrl] = $searchEntry
        } elseif ($relativeUrl.StartsWith(
                'api/',
                [System.StringComparison]::OrdinalIgnoreCase)) {
            $englishIndex[$relativeUrl] = $searchEntry
            $russianIndex[$relativeUrl] = $searchEntry
            $allIndex[$relativeUrl] = $searchEntry
        } else {
            throw "The canonical page has no search language: $($entry.Url)"
        }

        $pathSegments = $relativeUrl.Split('/')
        $scopeLanguages = @()
        $library = $null
        $version = $null
        if ($pathSegments[0] -in @('en', 'ru') -and
            $pathSegments.Count -ge 4) {
            $scopeLanguages = @($pathSegments[0])
            $library = $pathSegments[1]
            $version = $pathSegments[2]
        } elseif ($pathSegments[0] -eq 'api' -and
            $pathSegments.Count -ge 4) {
            $scopeLanguages = @('en', 'ru')
            $library = $pathSegments[1]
            $version = $pathSegments[2]
        }

        if ($library -in @('Hexes', 'Spatial2D') -and
            $version -match '^\d+\.\d+\.\d+$') {
            foreach ($language in $scopeLanguages) {
                $scopeKey = "$language/$library/$version"
                if (-not $scopedIndexes.Contains($scopeKey)) {
                    $scopedIndexes[$scopeKey] = [ordered]@{}
                }
                $scopedIndexes[$scopeKey][$relativeUrl] = $searchEntry
            }
        }
    }

    $indexesToValidate = [ordered]@{
        'all' = $allIndex
        'en' = $englishIndex
        'ru' = $russianIndex
    }
    foreach ($scope in $scopedIndexes.GetEnumerator()) {
        $indexesToValidate[$scope.Key] = $scope.Value
    }

    foreach ($indexToValidate in $indexesToValidate.GetEnumerator()) {
        $index = $indexToValidate.Value
        $duplicateTitles = @(
            $index.Values |
                Group-Object { $_['title'] } |
                Where-Object Count -gt 1)
        $invalidEntries = @(
            $index.Values |
                Where-Object {
                    $_['href'] -match '(^|/)(upcoming|latest)/' -or
                    $_['href'] -match '(^|/)toc\.html$' -or
                    -not $sitemapUrls.Contains(
                        "$SiteBaseUrl$($_['href'])") -or
                    [string]::IsNullOrWhiteSpace($_['keywords']) -or
                    $_['summary'].Length -gt 1000
                })
        if ($duplicateTitles.Count -gt 0 -or $invalidEntries.Count -gt 0) {
            $duplicateTitleList = $duplicateTitles.Name -join '; '
            $invalidEntryList = @(
                $invalidEntries | ForEach-Object { $_['href'] }) -join '; '
            throw (
                "The $($indexToValidate.Key) search index is invalid: " +
                "$($duplicateTitles.Count) duplicate title groups and " +
                "$($invalidEntries.Count) invalid entries. " +
                "Duplicate titles: $duplicateTitleList. " +
                "Invalid entries: $invalidEntryList.")
        }
    }

    $searchRoot = Join-Path $SiteRoot 'search'
    [void] [System.IO.Directory]::CreateDirectory($searchRoot)

    Write-SearchIndex -Path $sourceIndexPath -Index $englishIndex
    Write-SearchIndex `
        -Path (Join-Path $SiteRoot 'en\index.json') `
        -Index $englishIndex
    Write-SearchIndex `
        -Path (Join-Path $SiteRoot 'ru\index.json') `
        -Index $russianIndex
    Write-SearchIndex `
        -Path (Join-Path $SiteRoot 'search\all.json') `
        -Index $allIndex

    foreach ($scope in $scopedIndexes.GetEnumerator()) {
        $relativePath = "$($scope.Key).json".Replace(
            '/',
            [System.IO.Path]::DirectorySeparatorChar)
        $scopePath = Join-Path $searchRoot $relativePath
        [void] [System.IO.Directory]::CreateDirectory(
            [System.IO.Path]::GetDirectoryName($scopePath))
        Write-SearchIndex -Path $scopePath -Index $scope.Value
    }

    return [pscustomobject]@{
        AllCount = $allIndex.Count
        EnglishCount = $englishIndex.Count
        RussianCount = $russianIndex.Count
        ScopedCount = $scopedIndexes.Count
    }
}

function Set-SearchRuntimeIndexes {
    param(
        [Parameter(Mandatory)]
        [string] $SiteRoot
    )

    $workerPath = Join-Path $SiteRoot 'public\search-worker.min.js'
    $content = Get-Content -LiteralPath $workerPath -Raw -Encoding UTF8
    $sourceSignature = 'async function ve({lunrLanguages:t})'
    $replacementSignature =
        'async function ve({lunrLanguages:t,searchIndexPath:p,' +
        'searchPriorityPrefixes:_searchPriorityPrefixes})'
    $sourceFetch = 'fetch("../index.json")'
    $replacementFetch = 'fetch(p||"../index.json")'
    $sourceCacheStore = 'u=X("docfx","lunr");if(t&&t.length>0'
    $replacementCacheStore =
        'u=X("docfx","lunr"),_searchCacheKey="v2|"+' +
        '(p||"../index.json")+"|"+(t||[]).join(",");' +
        'if(t&&t.length>0'
    $sourceCacheRead = 'ae("index",u)'
    $replacementCacheRead = 'ae(_searchCacheKey,u)'
    $sourceCacheWrite = 'he("index",JSON.stringify'
    $replacementCacheWrite = 'he(_searchCacheKey,JSON.stringify'
    $sourceQuery = 'G=i=>e.search(i).map(({ref:s})=>r[s])'
    $replacementQuery =
        'G=i=>{let s=e.search(i),o=i.split(/\s+/).map(u=>' +
        'u.replace(/^[+-]/,"").length>=3&&e.search(u).length===0?' +
        'u+"*":u).join(" ");if(o!==i){let a=new Set(s.map(u=>u.ref));' +
        'for(let u of e.search(o))a.has(u.ref)||(a.add(u.ref),s.push(u))}' +
        'let a=s.map(({ref:u})=>r[u]),l=u=>{let f=' +
        '(_searchPriorityPrefixes||[]).findIndex(d=>d.some(h=>' +
        'u.href.startsWith(h)));return f<0?' +
        '(_searchPriorityPrefixes||[]).length:f},c=u=>{let f=' +
        'u.href.match(/^(en|ru|api)\/(Hexes|Spatial2D)\/' +
        '(\d+)\.(\d+)\.(\d+)\/(.*)$/);return f?' +
        '{k:f[1]+"/"+f[2]+"/"+f[6]+"\\0"+u.summary,' +
        'v:[+f[3],+f[4],+f[5]]}:{k:u.href,v:[0,0,0]}},' +
        'd=(u,f)=>u[0]-f[0]||u[1]-f[1]||u[2]-f[2];' +
        'a.sort((u,f)=>l(u)-l(f));let h=new Map;' +
        'for(let u of a){let f=c(u),v=h.get(f.k),m=l(u);' +
        'v?m===v.r&&d(f.v,v.v)>0&&(v.e=u,v.v=f.v):' +
        'h.set(f.k,{e:u,r:m,v:f.v})}' +
        'return[...h.values()].map(u=>u.e)}'

    foreach ($replacement in @(
            [pscustomobject]@{
                Source = $sourceSignature
                Target = $replacementSignature
            },
            [pscustomobject]@{
                Source = $sourceFetch
                Target = $replacementFetch
            },
            [pscustomobject]@{
                Source = $sourceCacheStore
                Target = $replacementCacheStore
            },
            [pscustomobject]@{
                Source = $sourceCacheRead
                Target = $replacementCacheRead
            },
            [pscustomobject]@{
                Source = $sourceCacheWrite
                Target = $replacementCacheWrite
            },
            [pscustomobject]@{
                Source = $sourceQuery
                Target = $replacementQuery
            })) {
        $sourceCount = ([System.Text.RegularExpressions.Regex]::Matches(
                $content,
                [System.Text.RegularExpressions.Regex]::Escape(
                    $replacement.Source))).Count
        if ($sourceCount -ne 1) {
            throw 'The DocFX search worker could not be patched safely.'
        }
        $content = $content.Replace($replacement.Source, $replacement.Target)
    }

    [System.IO.File]::WriteAllText(
        $workerPath,
        $content,
        [System.Text.UTF8Encoding]::new($false))

    $workerVersion = (Get-FileHash -LiteralPath $workerPath -Algorithm SHA256).
        Hash.Substring(0, 12).ToLowerInvariant()
    $extensionPath = Join-Path $SiteRoot 'public\main.js'
    $extensionVersion = (
        Get-FileHash -LiteralPath $extensionPath -Algorithm SHA256
    ).Hash.Substring(0, 12).ToLowerInvariant()
    $clientPath = Join-Path $SiteRoot 'public\docfx.min.js'
    $content = Get-Content -LiteralPath $clientPath -Raw -Encoding UTF8
    $sourceWorker =
        'new Worker(t+"public/search-worker.min.js",{type:"module"})'
    $replacementWorker =
        'new Worker(t+"public/search-worker.min.js?v=' + $workerVersion +
        '",{type:"module"})'
    $sourceExtension = 'import("./main.js")'
    $replacementExtension =
        'import("./main.js?v=' + $extensionVersion + '")'
    $sourceInitialization =
        'let{lunrLanguages:b}=await D();i.postMessage({init:{lunrLanguages:b}});'
    $replacementInitialization =
        'let{lunrLanguages:b,searchIndexPath:v,' +
        'searchPriorityPrefixes:_searchPriorityPrefixes}=await D();' +
        'i.postMessage({init:{lunrLanguages:b,searchIndexPath:v,' +
        'searchPriorityPrefixes:_searchPriorityPrefixes}});'

    $workerSourceCount = ([System.Text.RegularExpressions.Regex]::Matches(
            $content,
            [System.Text.RegularExpressions.Regex]::Escape(
                $sourceWorker))).Count
    if ($workerSourceCount -ne 1) {
        throw 'The DocFX search worker URL could not be patched safely.'
    }
    $content = $content.Replace($sourceWorker, $replacementWorker)

    $extensionSourceCount = (
        [System.Text.RegularExpressions.Regex]::Matches(
            $content,
            [System.Text.RegularExpressions.Regex]::Escape(
                $sourceExtension))
    ).Count
    if ($extensionSourceCount -ne 1) {
        throw 'The DocFX extension URL could not be patched safely.'
    }
    $content = $content.Replace($sourceExtension, $replacementExtension)

    $sourceCount = ([System.Text.RegularExpressions.Regex]::Matches(
            $content,
            [System.Text.RegularExpressions.Regex]::Escape(
                $sourceInitialization))).Count
    if ($sourceCount -ne 1) {
        throw 'The DocFX search client could not be patched safely.'
    }

    $content = $content.Replace(
        $sourceInitialization,
        $replacementInitialization)
    [System.IO.File]::WriteAllText(
        $clientPath,
        $content,
        [System.Text.UTF8Encoding]::new($false))

    $clientVersion = (Get-FileHash -LiteralPath $clientPath -Algorithm SHA256).
        Hash.Substring(0, 12).ToLowerInvariant()
    $sourceClientUrl = 'public/docfx.min.js"'
    $replacementClientUrl =
        'public/docfx.min.js?v=' + $clientVersion + '"'
    $patchedPageCount = 0

    Get-ChildItem -LiteralPath $SiteRoot -Filter '*.html' -File -Recurse |
        ForEach-Object {
            $pageContent = Get-Content `
                -LiteralPath $_.FullName `
                -Raw `
                -Encoding UTF8
            if ($pageContent.Contains($sourceClientUrl)) {
                $pageContent = $pageContent.Replace(
                    $sourceClientUrl,
                    $replacementClientUrl)
                [System.IO.File]::WriteAllText(
                    $_.FullName,
                    $pageContent,
                    [System.Text.UTF8Encoding]::new($false))
                $patchedPageCount++
            }
        }

    if ($patchedPageCount -eq 0) {
        throw 'No DocFX client URLs were patched for cache invalidation.'
    }
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

function Add-VersionedLibraryDocumentation {
    param(
        [Parameter(Mandatory)]
        [string] $Library,

        [Parameter(Mandatory)]
        [string] $RepositoryRoot,

        [Parameter(Mandatory)]
        [string] $Docfx,

        [Parameter(Mandatory)]
        [string] $SiteRoot,

        [Parameter(Mandatory)]
        [string] $VersionAdapterRoot,

        [Parameter(Mandatory)]
        [string] $PackageVersion,

        [Parameter(Mandatory)]
        [string] $TargetVersionPath,

        [Parameter(Mandatory)]
        [string] $ExpectedPackageHash,

        [string] $ArticleSourceRoot,

        [string] $ReferencePackagePath,

        [string] $ExpectedReferencePackageHash,

        [string] $ReferenceAssemblyName
    )

    # API versions have overlapping UIDs, so each version's articles and API
    # must be rendered together in an independent graph before their static
    # output joins the current site.
    $packageId = "Akeldov.Math.$Library"

    if ([string]::IsNullOrWhiteSpace($TargetVersionPath) -or
        $TargetVersionPath -in @('.', '..') -or
        $TargetVersionPath.IndexOfAny(
            [System.IO.Path]::GetInvalidFileNameChars()) -ge 0) {
        throw "Invalid versioned output segment: $TargetVersionPath"
    }

    $temporaryParent = [System.IO.Path]::GetFullPath(
        (Join-Path $RepositoryRoot '.tmp\docfx-versioned'))
    $fragmentRoot = [System.IO.Path]::GetFullPath(
        (Join-Path $temporaryParent "$library-$PackageVersion"))
    $versionedSourceRoot = [System.IO.Path]::GetFullPath(
        (Join-Path $VersionAdapterRoot 'source'))
    $packageFileName = "$packageId.$PackageVersion.nupkg"
    $packagePath = [System.IO.Path]::GetFullPath(
        (Join-Path $versionedSourceRoot $packageFileName))
    $packageExtractRoot = Join-Path $fragmentRoot 'package'
    $packageAssembly = Join-Path `
        $packageExtractRoot "lib\net6.0\$packageId.dll"
    $packageDocumentation = Join-Path `
        $packageExtractRoot "lib\net6.0\$packageId.xml"
    $referencePackageExtractRoot = Join-Path $fragmentRoot 'reference-package'
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
            $packageFileName) {
        throw "Invalid versioned package path: $packagePath"
    }

    if (-not (Test-Path -LiteralPath $packagePath)) {
        throw "The $library $PackageVersion package is missing."
    }

    $packageHash = (Get-FileHash -LiteralPath $packagePath -Algorithm SHA256).Hash
    if ($packageHash -ne $ExpectedPackageHash) {
        throw "The $library $PackageVersion package hash is invalid."
    }

    $referenceArguments = @(
        $ReferencePackagePath,
        $ExpectedReferencePackageHash,
        $ReferenceAssemblyName
    )
    $referenceArgumentCount = @(
        $referenceArguments | Where-Object {
            -not [string]::IsNullOrWhiteSpace($_)
        }
    ).Count

    if ($referenceArgumentCount -notin @(0, $referenceArguments.Count)) {
        throw 'Reference package path, hash, and assembly name must be specified together.'
    }

    New-Item -ItemType Directory -Path $temporaryParent -Force | Out-Null

    try {
        if (Test-Path -LiteralPath $fragmentRoot) {
            Remove-Item -LiteralPath $fragmentRoot -Recurse -Force
        }

        if (-not [string]::IsNullOrWhiteSpace($ArticleSourceRoot)) {
            $articleStageRoot = Join-Path $fragmentRoot 'articles'
            New-Item -ItemType Directory -Path $articleStageRoot -Force |
                Out-Null

            foreach ($language in @('en', 'ru')) {
                $languageSourceRoot = Join-Path $ArticleSourceRoot $language

                if (-not (Test-Path -LiteralPath $languageSourceRoot)) {
                    throw "The $library article source is missing: $languageSourceRoot"
                }

                Copy-Item -LiteralPath $languageSourceRoot `
                    -Destination $articleStageRoot -Recurse

                $languageOverrideRoot = Join-Path $VersionAdapterRoot $language
                if (-not (Test-Path -LiteralPath $languageOverrideRoot)) {
                    continue
                }

                $languageStageRoot = Join-Path $articleStageRoot $language
                Get-ChildItem -LiteralPath $languageOverrideRoot -Recurse -File |
                    ForEach-Object {
                        $relativePath = $_.FullName.Substring(
                            $languageOverrideRoot.Length).TrimStart(
                                [char][System.IO.Path]::DirectorySeparatorChar)
                        $destination = Join-Path $languageStageRoot $relativePath
                        $destinationDirectory = Split-Path -Parent $destination
                        New-Item -ItemType Directory `
                            -Path $destinationDirectory -Force | Out-Null
                        Copy-Item -LiteralPath $_.FullName `
                            -Destination $destination -Force
                    }
            }
        }

        Add-Type -AssemblyName System.IO.Compression.FileSystem
        [System.IO.Compression.ZipFile]::ExtractToDirectory(
            $packagePath,
            $packageExtractRoot)

        if ($referenceArgumentCount -gt 0) {
            $referencePackageHash = (
                Get-FileHash -LiteralPath $ReferencePackagePath -Algorithm SHA256
            ).Hash
            if ($referencePackageHash -ne $ExpectedReferencePackageHash) {
                throw "The $ReferenceAssemblyName reference package hash is invalid."
            }

            [System.IO.Compression.ZipFile]::ExtractToDirectory(
                $ReferencePackagePath,
                $referencePackageExtractRoot)
            $referenceAssembly = Join-Path `
                $referencePackageExtractRoot `
                "lib\net6.0\$ReferenceAssemblyName.dll"

            if (-not (Test-Path -LiteralPath $referenceAssembly)) {
                throw "The $ReferenceAssemblyName reference assembly is missing."
            }

            Copy-Item -LiteralPath $referenceAssembly `
                -Destination (Split-Path -Parent $packageAssembly)
        }

        if (-not (Test-Path -LiteralPath $packageAssembly) -or
            -not (Test-Path -LiteralPath $packageDocumentation)) {
            throw "The $library $PackageVersion package API files are missing."
        }

        & $Docfx (Join-Path $VersionAdapterRoot 'api.docfx.json')
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to build the $library $PackageVersion documentation."
        }

        $fragmentSiteRoot = Join-Path $fragmentRoot 'site\Akeldov.Math'

        if (-not [string]::IsNullOrWhiteSpace($ArticleSourceRoot)) {
            $articleOutputs = @(
                [pscustomobject]@{
                    Language = 'en'
                    Root = Join-Path `
                        $fragmentSiteRoot "$library\$TargetVersionPath"
                },
                [pscustomobject]@{
                    Language = 'ru'
                    Root = Join-Path `
                        $fragmentSiteRoot "ru\$library\$TargetVersionPath"
                }
            )

            foreach ($articleOutput in $articleOutputs) {
                $languageSourceRoot = Join-Path `
                    $ArticleSourceRoot $articleOutput.Language
                $languageOverrideRoot = Join-Path `
                    $VersionAdapterRoot $articleOutput.Language
                $languageStageRoot = Join-Path `
                    $articleStageRoot $articleOutput.Language

                Get-ChildItem -LiteralPath $articleOutput.Root `
                    -Recurse -Filter '*.html' -File |
                    ForEach-Object {
                        $relativeArticlePath = [System.IO.Path]::ChangeExtension(
                            (Get-RelativeSitePath `
                                -Root $articleOutput.Root `
                                -Path $_.FullName),
                            '.md')
                        $overridePath = Join-Path `
                            $languageOverrideRoot $relativeArticlePath
                        $sourcePath = if (Test-Path -LiteralPath $overridePath) {
                            $overridePath
                        } else {
                            Join-Path $languageSourceRoot $relativeArticlePath
                        }

                        if (-not (Test-Path -LiteralPath $sourcePath)) {
                            return
                        }

                        $stagedPath = Join-Path `
                            $languageStageRoot $relativeArticlePath
                        $stagedRepositoryPath = Get-RelativeSitePath `
                            -Root $RepositoryRoot `
                            -Path $stagedPath
                        $sourceRepositoryPath = Get-RelativeSitePath `
                            -Root $RepositoryRoot `
                            -Path $sourcePath
                        $content = Get-Content `
                            -LiteralPath $_.FullName -Raw -Encoding UTF8
                        $content = $content.Replace(
                            $stagedRepositoryPath,
                            $sourceRepositoryPath)
                        [System.IO.File]::WriteAllText(
                            $_.FullName,
                            $content,
                            [System.Text.UTF8Encoding]::new($false))
                    }
            }
        }

        $outputPaths = @(
            "$library\$TargetVersionPath",
            "ru\$library\$TargetVersionPath",
            "api\$library\$TargetVersionPath"
        )

        foreach ($outputPath in $outputPaths) {
            $sourceRoot = Join-Path $fragmentSiteRoot $outputPath
            $destinationRoot = Join-Path $SiteRoot $outputPath

            if (-not (Test-Path -LiteralPath $sourceRoot)) {
                throw "The $library $PackageVersion output is missing: $outputPath"
            }

            if (Test-Path -LiteralPath $destinationRoot) {
                throw "The $library $PackageVersion output already exists: $outputPath"
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

function Update-SiteAssetRelativeLinks {
    param(
        [Parameter(Mandatory)]
        [string] $SiteRoot
    )

    Get-ChildItem -LiteralPath $SiteRoot -Recurse -Filter '*.html' -File |
        ForEach-Object {
            $relativePath = Get-RelativeSitePath -Root $SiteRoot -Path $_.FullName
            $relativeDirectory = Split-Path -Parent $relativePath
            $depth = if ($relativeDirectory) {
                ($relativeDirectory -split '[\\/]').Count
            } else {
                0
            }
            $assetPrefix = ('../' * $depth) + 'assets/'
            $content = Get-Content -LiteralPath $_.FullName -Raw -Encoding UTF8
            $updatedContent = [System.Text.RegularExpressions.Regex]::Replace(
                $content,
                '(?<attribute>(?:href|src)=")(?:\./)?(?:\.\./)*assets/',
                [System.Text.RegularExpressions.MatchEvaluator] {
                    param($match)
                    return $match.Groups['attribute'].Value + $assetPrefix
                })

            if ($updatedContent -ne $content) {
                [System.IO.File]::WriteAllText(
                    $_.FullName,
                    $updatedContent,
                    [System.Text.UTF8Encoding]::new($false))
            }
        }
}

function Add-VersionAliasRedirects {
    param(
        [Parameter(Mandatory)]
        [string] $SiteRoot,

        [Parameter(Mandatory)]
        [string] $Library,

        [Parameter(Mandatory)]
        [string] $CanonicalVersion,

        [Parameter(Mandatory)]
        [string] $Alias,

        [Parameter(Mandatory)]
        [string] $SiteBaseUrl
    )

    $siteRootFullPath = [System.IO.Path]::GetFullPath($SiteRoot)
    $siteRootPrefix = $siteRootFullPath.TrimEnd([System.IO.Path]::DirectorySeparatorChar) +
        [System.IO.Path]::DirectorySeparatorChar

    foreach ($prefix in @('', 'en', 'ru', 'api')) {
        $libraryRoot = if ($prefix) {
            Join-Path (Join-Path $SiteRoot $prefix) $Library
        }
        else {
            Join-Path $SiteRoot $Library
        }
        $source = Join-Path $libraryRoot $CanonicalVersion
        $destination = Join-Path $libraryRoot $Alias
        $destinationFullPath = [System.IO.Path]::GetFullPath($destination)

        if (-not (Test-Path -LiteralPath $source)) {
            throw "The canonical version output is missing: $source"
        }

        if (-not $destinationFullPath.StartsWith(
                $siteRootPrefix,
                [System.StringComparison]::OrdinalIgnoreCase)) {
            throw "The version alias output is outside the site root: $destinationFullPath"
        }

        if (Test-Path -LiteralPath $destination) {
            Remove-Item -LiteralPath $destination -Recurse -Force
        }

        $sourceFullPath = [System.IO.Path]::GetFullPath($source)
        $publishedPrefix = if ($prefix) { "$prefix/" } else { '' }

        Get-ChildItem -LiteralPath $source -Filter '*.html' -File -Recurse | ForEach-Object {
            $relativePath = Get-RelativeSitePath `
                -Root $sourceFullPath `
                -Path $_.FullName
            $redirectFile = Join-Path $destination $relativePath
            $redirectDirectory = Split-Path -Parent $redirectFile
            New-Item -ItemType Directory -Path $redirectDirectory -Force | Out-Null

            $publishedRelativePath = $relativePath
            $targetPath = "/Akeldov.Math/$publishedPrefix$Library/$CanonicalVersion/$publishedRelativePath"
            $targetHtml = [System.Net.WebUtility]::HtmlEncode($targetPath)
            $canonicalUrl = [System.Net.WebUtility]::HtmlEncode(
                "$($SiteBaseUrl.TrimEnd('/'))/$publishedPrefix$Library/$CanonicalVersion/$publishedRelativePath")
            $targetJson = $targetPath | ConvertTo-Json -Compress
            $redirectHtml = @"
<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <meta name="robots" content="noindex">
  <link rel="canonical" href="$canonicalUrl">
  <meta http-equiv="refresh" content="0; url=$targetHtml">
  <title>Redirecting...</title>
</head>
<body>
  <p><a href="$targetHtml">Continue to the current version</a></p>
  <script>location.replace($targetJson + location.search + location.hash);</script>
</body>
</html>
"@
            [System.IO.File]::WriteAllText(
                $redirectFile,
                $redirectHtml,
                [System.Text.UTF8Encoding]::new($false))
        }
    }
}

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$localDocfx = Join-Path $repositoryRoot '.tmp\docfx-tool\docfx.exe'
$siteRoot = Join-Path $PSScriptRoot '_site\Akeldov.Math'
$spatial2DArticleBaseRoot = Join-Path `
    $PSScriptRoot 'versioned\Spatial2D\0.8.0'
$spatial2DArticleOverrideRoot = Join-Path `
    $PSScriptRoot 'versioned\Spatial2D\0.9.0'
$spatial2DUpcomingArticleStageRoot = Join-Path `
    $repositoryRoot '.tmp\docfx-upcoming\Spatial2D'
$hexesArticleBaseRoot = Join-Path `
    $PSScriptRoot 'versioned\Hexes\0.1.0'
$hexesArticleOverrideRoot = Join-Path `
    $PSScriptRoot 'versioned\Hexes\0.2.0'
$hexesUpcomingArticleStageRoot = Join-Path `
    $repositoryRoot '.tmp\docfx-upcoming\Hexes'

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

New-MergedArticleSource `
    -RepositoryRoot $repositoryRoot `
    -BaseRoot $spatial2DArticleBaseRoot `
    -OverrideRoot $spatial2DArticleOverrideRoot `
    -StageRoot $spatial2DUpcomingArticleStageRoot

New-MergedArticleSource `
    -RepositoryRoot $repositoryRoot `
    -BaseRoot $hexesArticleBaseRoot `
    -OverrideRoot $hexesArticleOverrideRoot `
    -StageRoot $hexesUpcomingArticleStageRoot

& $docfx (Join-Path $PSScriptRoot 'docfx.json')
$docfxExitCode = $LASTEXITCODE

if ($docfxExitCode -ne 0) {
    exit $docfxExitCode
}

Update-MergedArticleContributionLinks `
    -RepositoryRoot $repositoryRoot `
    -SiteRoot $siteRoot `
    -BaseRoot $spatial2DArticleBaseRoot `
    -OverrideRoot $spatial2DArticleOverrideRoot `
    -StageRoot $spatial2DUpcomingArticleStageRoot `
    -Library 'Spatial2D' `
    -VersionPath 'upcoming'

Remove-Item -LiteralPath $spatial2DUpcomingArticleStageRoot -Recurse -Force

Update-MergedArticleContributionLinks `
    -RepositoryRoot $repositoryRoot `
    -SiteRoot $siteRoot `
    -BaseRoot $hexesArticleBaseRoot `
    -OverrideRoot $hexesArticleOverrideRoot `
    -StageRoot $hexesUpcomingArticleStageRoot `
    -Library 'Hexes' `
    -VersionPath 'upcoming'

Remove-Item -LiteralPath $hexesUpcomingArticleStageRoot -Recurse -Force

Add-VersionedLibraryDocumentation `
    -Library 'Spatial2D' `
    -RepositoryRoot $repositoryRoot `
    -Docfx $docfx `
    -SiteRoot $siteRoot `
    -VersionAdapterRoot (
        Join-Path $PSScriptRoot 'versioned\Spatial2D\0.8.0') `
    -PackageVersion '0.8.0' `
    -TargetVersionPath '0.8.0' `
    -ExpectedPackageHash `
        '293179161CFEA2D649CCECBD770863E9504D95FF0984F095E187FA9809D8975E'

Add-VersionedLibraryDocumentation `
    -Library 'Spatial2D' `
    -RepositoryRoot $repositoryRoot `
    -Docfx $docfx `
    -SiteRoot $siteRoot `
    -VersionAdapterRoot (
        Join-Path $PSScriptRoot 'versioned\Spatial2D\0.9.0') `
    -PackageVersion '0.9.0' `
    -TargetVersionPath '0.9.0' `
    -ExpectedPackageHash `
        '5F03676949B71F79CCCF1A5D35015B3B6BE4C1D7380C03981CEF3E358C483345' `
    -ArticleSourceRoot $spatial2DArticleBaseRoot

Add-VersionedLibraryDocumentation `
    -Library 'Hexes' `
    -RepositoryRoot $repositoryRoot `
    -Docfx $docfx `
    -SiteRoot $siteRoot `
    -VersionAdapterRoot (
        Join-Path $PSScriptRoot 'versioned\Hexes\0.1.0') `
    -PackageVersion '0.1.0' `
    -TargetVersionPath '0.1.0' `
    -ExpectedPackageHash `
        'E36514F70F6A145D60DA353A20E391C996516301CC5B5CF57DB27D3E0DD01A2A' `
    -ReferencePackagePath (
        Join-Path $PSScriptRoot `
            'versioned\Spatial2D\0.8.0\source\Akeldov.Math.Spatial2D.0.8.0.nupkg') `
    -ExpectedReferencePackageHash `
        '293179161CFEA2D649CCECBD770863E9504D95FF0984F095E187FA9809D8975E' `
    -ReferenceAssemblyName 'Akeldov.Math.Spatial2D'

Add-VersionedLibraryDocumentation `
    -Library 'Hexes' `
    -RepositoryRoot $repositoryRoot `
    -Docfx $docfx `
    -SiteRoot $siteRoot `
    -VersionAdapterRoot (
        Join-Path $PSScriptRoot 'versioned\Hexes\0.2.0') `
    -PackageVersion '0.2.0' `
    -TargetVersionPath '0.2.0' `
    -ExpectedPackageHash `
        'ADD2A1BDB4D36059744FEDA328B6F7BD687BED0CC59C5538518B886CF2A236EC' `
    -ReferencePackagePath (
        Join-Path $PSScriptRoot `
            'versioned\Spatial2D\0.9.0\source\Akeldov.Math.Spatial2D.0.9.0.nupkg') `
    -ExpectedReferencePackageHash `
        '5F03676949B71F79CCCF1A5D35015B3B6BE4C1D7380C03981CEF3E358C483345' `
    -ReferenceAssemblyName 'Akeldov.Math.Spatial2D' `
    -ArticleSourceRoot $hexesArticleBaseRoot

$englishRoot = Join-Path $siteRoot 'en'
$russianRoot = Join-Path $siteRoot 'ru'
$russianSourceRoot = Join-Path $PSScriptRoot 'ru'
$spatial2DVersionedRussianSourceRoot = Join-Path `
    $PSScriptRoot 'versioned\Spatial2D\0.8.0\ru'
$spatial2D09RussianOverrideRoot = Join-Path `
    $PSScriptRoot 'versioned\Spatial2D\0.9.0\ru'
$hexes01RussianSourceRoot = Join-Path `
    $PSScriptRoot 'versioned\Hexes\0.1.0\ru'
$hexes02RussianSourceRoot = Join-Path `
    $PSScriptRoot 'versioned\Hexes\0.2.0\ru'
$russianSourceMappings = @(
    [pscustomobject]@{
        Root = $russianSourceRoot
        OutputPrefix = $null
    },
    [pscustomobject]@{
        Root = $spatial2DVersionedRussianSourceRoot
        OutputPrefix = Join-Path 'Spatial2D' '0.8.0'
    },
    [pscustomobject]@{
        Root = $spatial2DVersionedRussianSourceRoot
        OutputPrefix = Join-Path 'Spatial2D' '0.9.0'
    },
    [pscustomobject]@{
        Root = $spatial2D09RussianOverrideRoot
        OutputPrefix = Join-Path 'Spatial2D' '0.9.0'
    },
    [pscustomobject]@{
        Root = $spatial2DVersionedRussianSourceRoot
        OutputPrefix = Join-Path 'Spatial2D' 'upcoming'
    },
    [pscustomobject]@{
        Root = $spatial2D09RussianOverrideRoot
        OutputPrefix = Join-Path 'Spatial2D' 'upcoming'
    },
    [pscustomobject]@{
        Root = $hexes01RussianSourceRoot
        OutputPrefix = Join-Path 'Hexes' '0.1.0'
    },
    [pscustomobject]@{
        Root = $hexes01RussianSourceRoot
        OutputPrefix = Join-Path 'Hexes' '0.2.0'
    },
    [pscustomobject]@{
        Root = $hexes02RussianSourceRoot
        OutputPrefix = Join-Path 'Hexes' '0.2.0'
    },
    [pscustomobject]@{
        Root = $hexes01RussianSourceRoot
        OutputPrefix = Join-Path 'Hexes' 'upcoming'
    },
    [pscustomobject]@{
        Root = $hexes02RussianSourceRoot
        OutputPrefix = Join-Path 'Hexes' 'upcoming'
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
Update-SiteAssetRelativeLinks -SiteRoot $siteRoot

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
$russianFallbackPages = @()
$p0NoIndexPageCount = 0
$p0TocFragmentCount = 0

$russianRootPrefix = [System.IO.Path]::GetFullPath($russianRoot).TrimEnd(
    [System.IO.Path]::DirectorySeparatorChar) +
    [System.IO.Path]::DirectorySeparatorChar
Get-ChildItem -LiteralPath $siteRoot -Recurse -Filter '*.html' -File |
    ForEach-Object {
        $language = 'en'
        if ($_.FullName.StartsWith(
                $russianRootPrefix,
                [System.StringComparison]::OrdinalIgnoreCase)) {
            $relativePath = $_.FullName.Substring($russianRootPrefix.Length)
            if ($russianOverrides.ContainsKey($relativePath)) {
                $language = 'ru'
            }
        }

        Set-PageLanguage -Path $_.FullName -Language $language
    }

Get-ChildItem -LiteralPath $siteRoot -Recurse -Filter '*.html' -File |
    ForEach-Object {
        $relativePath = Get-RelativeSitePath `
            -Root $siteRoot -Path $_.FullName
        $pathSegments = $relativePath.Split('/')
        $isUpcoming = $pathSegments -contains 'upcoming'
        $isTableOfContents = $pathSegments[-1].Equals(
            'toc.html',
            [System.StringComparison]::OrdinalIgnoreCase)
        $isHeadingOnly = Test-PageHasHeadingOnlyArticle -Path $_.FullName

        if ($isUpcoming -or $isHeadingOnly) {
            Set-PageNoIndexMetadata -Path $_.FullName
            $p0NoIndexPageCount++
        }

        if ($isTableOfContents) {
            $p0TocFragmentCount++
        }
    }

Get-ChildItem -LiteralPath $englishRoot -Recurse -Filter '*.html' -File |
    ForEach-Object {
        $relativePath = Get-RelativeSitePath -Root $englishRoot -Path $_.FullName

        if ((Test-IsP0NonIndexableSitePath -RelativePath $relativePath) -or
            (Test-PageHasNoIndex -Path $_.FullName)) {
            return
        }

        $englishUrl = "$siteBaseUrl" + "en/$relativePath"
        $overrideKey = $relativePath.Replace(
            '/',
            [System.IO.Path]::DirectorySeparatorChar)
        $russianPagePath = Join-Path $russianRoot $overrideKey
        $russianUrl = if ($russianOverrides.ContainsKey($overrideKey) -and
            (Test-Path -LiteralPath $russianPagePath) -and
            -not (Test-PageHasNoIndex -Path $russianPagePath)) {
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
            SourcePath = $_.FullName
        }
    }

Get-ChildItem -LiteralPath $russianRoot -Recurse -Filter '*.html' -File |
    ForEach-Object {
        $relativePath = Get-RelativeSitePath -Root $russianRoot -Path $_.FullName

        if ((Test-IsP0NonIndexableSitePath -RelativePath $relativePath) -or
            (Test-PageHasNoIndex -Path $_.FullName)) {
            return
        }

        $overrideKey = $relativePath.Replace(
            '/',
            [System.IO.Path]::DirectorySeparatorChar)
        $englishPagePath = Join-Path $englishRoot $overrideKey
        $englishUrl = if ((Test-Path -LiteralPath $englishPagePath) -and
            -not (Test-PageHasNoIndex -Path $englishPagePath)) {
            "$siteBaseUrl" + "en/$relativePath"
        } else {
            $null
        }

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
                SourcePath = $_.FullName
            }
        } else {
            Set-PageSeoMetadata -Path $_.FullName -CanonicalUrl $englishUrl
            $russianFallbackPages += [pscustomobject]@{
                Path = $_.FullName
                CanonicalUrl = $englishUrl
            }
        }
    }

Get-ChildItem -LiteralPath $apiRoot -Recurse -Filter '*.html' -File |
    ForEach-Object {
        $relativePath = Get-RelativeSitePath -Root $apiRoot -Path $_.FullName

        if ((Test-IsP0NonIndexableSitePath -RelativePath $relativePath) -or
            (Test-PageHasNoIndex -Path $_.FullName)) {
            return
        }

        $canonicalUrl = "$siteBaseUrl" + "api/$relativePath"
        Set-PageSeoMetadata -Path $_.FullName -CanonicalUrl $canonicalUrl
        $sitemapEntries += [pscustomobject]@{
            Url = $canonicalUrl
            EnglishUrl = $null
            RussianUrl = $null
            SourcePath = $_.FullName
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
        $relativePath = Get-RelativeSitePath -Root $siteRoot -Path $_.FullName

        if (-not (Test-IsP0NonIndexableSitePath -RelativePath $relativePath) -and
            -not (Test-PageHasNoIndex -Path $_.FullName)) {
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

$uniqueSitemapEntries = @($sitemapEntries | Sort-Object Url -Unique)
if ($uniqueSitemapEntries.Count -ne $sitemapEntries.Count) {
    throw 'The sitemap source contains duplicate canonical URLs.'
}

foreach ($entry in $uniqueSitemapEntries) {
    $searchMetadata = Set-PageSearchMetadata `
        -Path $entry.SourcePath `
        -Url $entry.Url `
        -SiteBaseUrl $siteBaseUrl
    $entry | Add-Member -NotePropertyName SearchTitle `
        -NotePropertyValue $searchMetadata.Title
    $entry | Add-Member -NotePropertyName SearchDescription `
        -NotePropertyValue $searchMetadata.Description
}

$duplicateSearchTitles = @(
    $uniqueSitemapEntries |
        Group-Object SearchTitle |
        Where-Object Count -gt 1)
if ($duplicateSearchTitles.Count -gt 0) {
    $duplicateTitleList = $duplicateSearchTitles.Name -join '; '
    throw "The sitemap contains duplicate search titles: $duplicateTitleList"
}

foreach ($entry in $uniqueSitemapEntries) {
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

if ($sitemapEntries.Url -match '/upcoming/' -or
    $sitemapEntries.Url -match '/toc\.html$') {
    throw 'The sitemap contains a P0 non-indexable URL.'
}

$nonIndexableSitemapEntries = @(
    $sitemapEntries |
        Where-Object {
            (Test-PageHasNoIndex -Path $_.SourcePath) -or
            (Test-PageHasHeadingOnlyArticle -Path $_.SourcePath)
        })

if ($nonIndexableSitemapEntries.Count -gt 0) {
    throw 'The sitemap contains a noindex or heading-only page.'
}

$sitemapUrlSet = @{}
foreach ($entry in $uniqueSitemapEntries) {
    $sitemapUrlSet[$entry.Url] = $true
}

foreach ($entry in $uniqueSitemapEntries) {
    $content = Get-Content -LiteralPath $entry.SourcePath -Raw -Encoding UTF8
    $expectedLanguage = if ($entry.Url.StartsWith(
            "$siteBaseUrl" + 'ru/',
            [System.StringComparison]::OrdinalIgnoreCase)) {
        'ru'
    } else {
        'en'
    }
    $languageMatch = [System.Text.RegularExpressions.Regex]::Match(
        $content,
        '<html\b[^>]*\blang=["''](?<language>[^"'']+)["'']',
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)

    if (-not $languageMatch.Success -or
        $languageMatch.Groups['language'].Value -ne $expectedLanguage) {
        throw "The page language is invalid for $($entry.Url)."
    }

    $canonicalMatches = [System.Text.RegularExpressions.Regex]::Matches(
        $content,
        '<link\s+rel=["'']canonical["'']\s+href=["''](?<url>[^"'']+)["'']',
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)

    if ($canonicalMatches.Count -ne 1 -or
        $canonicalMatches[0].Groups['url'].Value -ne $entry.Url) {
        throw "The canonical URL is invalid for $($entry.Url)."
    }

    $alternateMatches = [System.Text.RegularExpressions.Regex]::Matches(
        $content,
        '<link\s+rel=["'']alternate["'']\s+hreflang=["''](?<language>[^"'']+)["'']\s+href=["''](?<url>[^"'']+)["'']',
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    $expectedAlternates = if ($entry.EnglishUrl -and $entry.RussianUrl) {
        @{
            'en' = $entry.EnglishUrl
            'ru' = $entry.RussianUrl
            'x-default' = $entry.EnglishUrl
        }
    } else {
        @{}
    }

    if ($alternateMatches.Count -ne $expectedAlternates.Count) {
        throw "The hreflang set is invalid for $($entry.Url)."
    }

    foreach ($alternateMatch in $alternateMatches) {
        $alternateLanguage = $alternateMatch.Groups['language'].Value
        $alternateUrl = $alternateMatch.Groups['url'].Value
        if (-not $expectedAlternates.ContainsKey($alternateLanguage) -or
            $expectedAlternates[$alternateLanguage] -ne $alternateUrl -or
            -not $sitemapUrlSet.ContainsKey($alternateUrl)) {
            throw "The hreflang target is invalid for $($entry.Url)."
        }
    }
}

foreach ($fallbackPage in $russianFallbackPages) {
    $content = Get-Content -LiteralPath $fallbackPage.Path -Raw -Encoding UTF8
    if ($content -notmatch '<html\b[^>]*\blang=["'']en["'']' -or
        $content -notmatch [System.Text.RegularExpressions.Regex]::Escape(
            "<link rel=`"canonical`" href=`"$($fallbackPage.CanonicalUrl)`">") -or
        $content -match '<link\s+rel=["'']alternate["'']\s+hreflang=') {
        throw "The Russian fallback metadata is invalid: $($fallbackPage.Path)"
    }
}

foreach ($entry in $uniqueSitemapEntries) {
    $content = Get-Content -LiteralPath $entry.SourcePath -Raw -Encoding UTF8
    $encodedTitle = [System.Net.WebUtility]::HtmlEncode($entry.SearchTitle)
    $encodedDescription = [System.Net.WebUtility]::HtmlEncode(
        $entry.SearchDescription)
    $titleMatches = [System.Text.RegularExpressions.Regex]::Matches(
        $content,
        "<title>$([System.Text.RegularExpressions.Regex]::Escape($encodedTitle))</title>",
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    $metaTitleMatches = [System.Text.RegularExpressions.Regex]::Matches(
        $content,
        "<meta name=`"title`" content=`"$([System.Text.RegularExpressions.Regex]::Escape($encodedTitle))`">",
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    $descriptionMatches = [System.Text.RegularExpressions.Regex]::Matches(
        $content,
        "<meta name=`"description`" content=`"$([System.Text.RegularExpressions.Regex]::Escape($encodedDescription))`">",
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)

    if ($titleMatches.Count -ne 1 -or
        $metaTitleMatches.Count -ne 1 -or
        $descriptionMatches.Count -ne 1 -or
        $entry.SearchDescription.Length -lt 20 -or
        $entry.SearchDescription.Length -gt 240) {
        throw "The search snippet metadata is invalid for $($entry.Url)."
    }
}

$searchIndexResult = Set-SiteSearchIndexes `
    -SiteRoot $siteRoot `
    -SiteBaseUrl $siteBaseUrl `
    -SitemapEntries $uniqueSitemapEntries
Set-SearchRuntimeIndexes -SiteRoot $siteRoot

Write-Host (
    "SEO P0: marked $p0NoIndexPageCount pages as noindex; " +
    "excluded $p0TocFragmentCount TOC fragments from the sitemap.")
Write-Host (
    "SEO P1: validated $($uniqueSitemapEntries.Count) canonical pages and " +
    "$($russianFallbackPages.Count) Russian fallback pages.")
Write-Host (
    "SEO P2: validated unique titles and valid descriptions for " +
    "$($uniqueSitemapEntries.Count) indexable pages.")
Write-Host (
    "Search: generated $($searchIndexResult.AllCount) global, " +
    "$($searchIndexResult.EnglishCount) English/API, and " +
    "$($searchIndexResult.RussianCount) Russian/API entries across " +
    "$($searchIndexResult.ScopedCount) version scopes.")

Add-VersionAliasRedirects `
    -SiteRoot $siteRoot `
    -Library 'Spatial2D' `
    -CanonicalVersion '0.9.0' `
    -Alias 'latest' `
    -SiteBaseUrl $siteBaseUrl

Add-VersionAliasRedirects `
    -SiteRoot $siteRoot `
    -Library 'Hexes' `
    -CanonicalVersion '0.2.0' `
    -Alias 'latest' `
    -SiteBaseUrl $siteBaseUrl

if ($Serve) {
    & $docfx serve (Join-Path $PSScriptRoot '_site') --port $Port
    exit $LASTEXITCODE
}
