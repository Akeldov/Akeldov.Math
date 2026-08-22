const selectorId = 'akeldov-docs-version';
const languageSelectorId = 'akeldov-docs-language';
const languagePreferenceKey = 'akeldov-docs-language-preference';
const repositoryLinkId = 'akeldov-repository-link';
const contextNavigationId = 'akeldov-library-navigation';
const contextNavigationPlaceholderId = `${contextNavigationId}-placeholder`;
const versionSelectorPlaceholderId = `${selectorId}-placeholder`;

const russianUiTranslations = new Map([
    ['About', 'О проекте'],
    ['API References', 'Справочник API'],
    ['Auto', 'Системная'],
    ['Dark', 'Тёмная'],
    ['Edit this page', 'Редактировать страницу'],
    ['Filter by title', 'Фильтр по заголовку'],
    ['Home', 'Главная'],
    ['Libraries', 'Библиотеки'],
    ['Light', 'Светлая'],
    ['Made with', 'Создано с помощью'],
    ['Next', 'Далее'],
    ['Package version', 'Версия пакета'],
    ['Previous', 'Назад'],
    ['Search', 'Поиск']
]);

async function fetchJson(url) {
    try {
        const response = await fetch(url);
        return response.ok ? await response.json() : null;
    } catch {
        return null;
    }
}

function isRussianPage() {
    return window.location.pathname
        .split('/')
        .filter(Boolean)
        .includes('ru');
}

function isApiPage() {
    return window.location.pathname
        .split('/')
        .filter(Boolean)
        .includes('api');
}

function getLanguagePreference() {
    try {
        return localStorage.getItem(languagePreferenceKey);
    } catch {
        return null;
    }
}

function setLanguagePreference(languageCode) {
    try {
        localStorage.setItem(languagePreferenceKey, languageCode);
    } catch {
        // The URL still carries the language context when storage is unavailable.
    }
}

function synchronizeLanguagePreference() {
    const requestedLanguage = new URLSearchParams(window.location.search).get('lang');

    if (isRussianPage() || requestedLanguage === 'ru') {
        setLanguagePreference('ru');
    } else if (!isApiPage()) {
        setLanguagePreference('en');
    }
}

function hasRussianLanguageContext() {
    return isRussianPage()
        || new URLSearchParams(window.location.search).get('lang') === 'ru'
        || (isApiPage() && getLanguagePreference() === 'ru');
}

function localizeReferenceOverview() {
    const pathSegments = window.location.pathname.split('/').filter(Boolean);
    const apiIndex = pathSegments.indexOf('api');
    const library = apiIndex >= 0 ? pathSegments[apiIndex + 1] : null;
    const isReferenceOverview = apiIndex >= 0
        && (library === 'Spatial2D' || library === 'Hexes')
        && pathSegments[pathSegments.length - 1] === 'index.html';

    if (!isReferenceOverview) {
        return;
    }

    const russian = hasRussianLanguageContext();
    for (const section of document.querySelectorAll('[data-reference-language]')) {
        section.hidden = section.dataset.referenceLanguage !== (russian ? 'ru' : 'en');
    }

    if (!russian) {
        return;
    }

    const englishTitle = `${library} API Reference`;
    const russianTitle = `Справочник API ${library}`;
    const heading = document.querySelector('article h1');
    if (heading) {
        heading.textContent = russianTitle;
    }

    document.title = document.title.replace(
        englishTitle,
        russianTitle);

    const titleMetadata = document.querySelector('meta[name="title"]');
    if (titleMetadata) {
        titleMetadata.content = titleMetadata.content.replace(
            englishTitle,
            russianTitle);
    }
}

function preserveRussianLanguageContext() {
    const pathname = window.location.pathname;
    const languageMarker = '/ru/';
    const apiMarker = '/api/';
    const markerIndex = pathname.indexOf(
        isApiPage() ? apiMarker : languageMarker);

    if (markerIndex < 0) {
        return;
    }

    const rootPath = pathname.substring(0, markerIndex + 1);
    const apiPath = `${rootPath}api/`;
    const russianPath = `${rootPath}ru/`;

    for (const link of document.querySelectorAll('a[href]')) {
        if (link.closest(`#${languageSelectorId}-container`)) {
            continue;
        }

        const url = new URL(link.href, window.location.href);

        if (url.origin !== window.location.origin
            || !url.pathname.startsWith(rootPath)) {
            continue;
        }

        if (url.pathname.startsWith(apiPath)) {
            url.searchParams.set('lang', 'ru');
            link.href = url;
        } else if (!url.pathname.startsWith(russianPath)
            && !(link.closest('#search-results')
                && url.pathname.startsWith(`${rootPath}en/`))) {
            const relativePath = url.pathname.substring(rootPath.length);
            url.pathname = `${russianPath}${relativePath}`;
            link.href = url;
        }
    }
}

function localizeRussianUi() {
    const roots = [
        document.querySelector('header'),
        document.getElementById('breadcrumb'),
        document.getElementById('toc'),
        document.querySelector('.contribution'),
        document.querySelector('.next-article'),
        document.querySelector('footer')
    ].filter(Boolean);

    if (isRussianPage()) {
        document.documentElement.lang = 'ru';
    } else {
        for (const root of roots) {
            root.lang = 'ru';
        }
    }

    for (const root of roots) {
        const walker = document.createTreeWalker(root, NodeFilter.SHOW_TEXT);
        let node;

        while ((node = walker.nextNode())) {
            const value = node.nodeValue.trim();
            const translation = russianUiTranslations.get(value);

            if (translation) {
                node.nodeValue = node.nodeValue.replace(value, translation);
            }
        }
    }

    for (const input of document.querySelectorAll('input[placeholder]')) {
        const translation = russianUiTranslations.get(input.placeholder);
        if (translation) {
            input.placeholder = translation;
        }
    }

    preserveRussianLanguageContext();
}

function startRussianLocalization() {
    if (!hasRussianLanguageContext()) {
        return;
    }

    localizeRussianUi();

    const observer = new MutationObserver(localizeRussianUi);
    observer.observe(document.body, { childList: true, subtree: true });
}

async function getVersionContext() {
    const moduleUrl = new URL(import.meta.url);
    const rootUrl = new URL('../', moduleUrl);
    const registry = await fetchJson(new URL('versions.json', rootUrl));

    if (!Array.isArray(registry?.libraries)) {
        return null;
    }

    const relativePath = window.location.pathname.startsWith(rootUrl.pathname)
        ? window.location.pathname.substring(rootUrl.pathname.length)
        : '';
    const segments = relativePath.split('/').filter(Boolean);
    const libraryIndex = segments.findIndex(segment => registry.libraries.includes(segment));

    if (libraryIndex < 0 || libraryIndex + 1 >= segments.length) {
        return null;
    }

    const library = segments[libraryIndex];
    const versionPath = segments[libraryIndex + 1];
    const libraryRootUrl = new URL(`${library}/`, rootUrl);
    const versionRootUrl = new URL(
        `${segments.slice(0, libraryIndex + 1).join('/')}/`,
        rootUrl);
    const versionData = await fetchJson(new URL('versions.json', libraryRootUrl));
    const navigationData = await fetchJson(
        new URL('library-navigation.json', rootUrl));

    if (!Array.isArray(versionData?.versions) || versionData.versions.length === 0) {
        return null;
    }

    const apiPage = segments[libraryIndex - 1] === 'api';
    const libraryNavigation = navigationData?.libraries?.find(
        item => item.path === library);

    return {
        apiPage,
        apiVersionRootUrl: new URL(`api/${library}/`, rootUrl),
        fallbackPagePath: apiPage
            ? (libraryNavigation?.referencePage ?? 'index.html')
            : 'index.html',
        language: segments[0] === 'ru' ||
            new URLSearchParams(window.location.search).get('lang') === 'ru'
            ? 'ru'
            : 'en',
        referencePage: libraryNavigation?.referencePage ?? 'index.html',
        versionRootUrl,
        versions: versionData.versions,
        versionPath,
        pagePath: segments.slice(libraryIndex + 2).join('/') || 'index.html'
    };
}

async function getLanguageContext() {
    const moduleUrl = new URL(import.meta.url);
    const registryUrls = [
        new URL('../../languages.json', moduleUrl),
        new URL('../languages.json', moduleUrl)
    ];

    for (const registryUrl of registryUrls) {
        const registry = await fetchJson(registryUrl);
        if (!Array.isArray(registry?.languages) || registry.languages.length === 0) {
            continue;
        }

        const rootUrl = new URL('./', registryUrl);
        if (!window.location.pathname.startsWith(rootUrl.pathname)) {
            continue;
        }

        const relativePath = window.location.pathname.substring(rootUrl.pathname.length);
        const segments = relativePath.split('/').filter(Boolean);
        const localizedLanguage = registry.languages.find(
            language => language.path && language.path === segments[0]);
        const pagePath = localizedLanguage
            ? segments.slice(1).join('/')
            : segments.join('/');
        const apiPage = pagePath === 'api' || pagePath.startsWith('api/');
        const preferredLanguageCode = apiPage && hasRussianLanguageContext()
            ? 'ru'
            : 'en';
        const currentLanguage = apiPage
            ? registry.languages.find(
                language => language.code === preferredLanguageCode)
                ?? registry.languages[0]
            : localizedLanguage
                ?? registry.languages.find(language => !language.path)
                ?? registry.languages[0];

        return {
            apiPage,
            currentLanguage,
            languages: registry.languages,
            pagePath: pagePath || 'index.html',
            rootUrl
        };
    }

    return null;
}

async function getLibraryNavigationContext() {
    const moduleUrl = new URL(import.meta.url);
    const registryUrls = [
        new URL('../../library-navigation.json', moduleUrl),
        new URL('../library-navigation.json', moduleUrl)
    ];
    let registry = null;
    let rootUrl = null;

    for (const registryUrl of registryUrls) {
        const candidate = await fetchJson(registryUrl);
        if (Array.isArray(candidate?.libraries)) {
            registry = candidate;
            rootUrl = new URL('./', registryUrl);
            break;
        }
    }

    if (!registry || !rootUrl) {
        return null;
    }

    const relativePath = window.location.pathname.startsWith(rootUrl.pathname)
        ? window.location.pathname.substring(rootUrl.pathname.length)
        : '';
    const segments = relativePath.split('/').filter(Boolean);
    const languagePath = segments[0] === 'en' || segments[0] === 'ru'
        ? segments.shift()
        : null;
    const apiPage = segments[0] === 'api';

    if (apiPage) {
        segments.shift();
    }

    const library = registry.libraries.find(item => item.path === segments[0]);
    if (!library) {
        return null;
    }

    segments.shift();

    const versionPath = library.versioned ? segments.shift() : null;
    if (library.versioned && !versionPath) {
        return null;
    }

    const language = hasRussianLanguageContext() ? 'ru' : (languagePath ?? 'en');
    const versionPrefix = versionPath ? `${versionPath}/` : '';
    const conceptualRootUrl = new URL(
        `${language}/${library.path}/${versionPrefix}`,
        rootUrl);
    const referenceRootUrl = new URL(
        `api/${library.path}/${versionPrefix}`,
        rootUrl);

    return {
        activeSection: apiPage
            ? 'reference'
            : segments[0]?.replace(/\.html$/, ''),
        conceptualRootUrl,
        library,
        referenceRootUrl,
        rootUrl,
        russian: language === 'ru',
        versionPath
    };
}

function reserveLibraryNavigationSpace() {
    if (document.getElementById(contextNavigationId)
        || document.getElementById(contextNavigationPlaceholderId)) {
        return;
    }

    const segments = window.location.pathname.split('/').filter(Boolean);
    const libraryIndex = segments.findIndex(
        segment => segment === 'Spatial2D' || segment === 'Hexes');
    const versionPath = libraryIndex >= 0 ? segments[libraryIndex + 1] : null;

    if (!versionPath || versionPath === 'index.html') {
        return;
    }

    const header = document.querySelector('body > header');
    if (!header) {
        return;
    }

    const placeholder = document.createElement('div');
    placeholder.id = contextNavigationPlaceholderId;
    placeholder.classList.add(
        'docs-context-navigation',
        'docs-context-navigation-placeholder');
    placeholder.setAttribute('aria-hidden', 'true');

    header.appendChild(placeholder);
    document.body.classList.add('docs-has-context-navigation');
    document.body.style.setProperty(
        '--docs-header-height',
        `${header.offsetHeight}px`);
}

async function addLibraryNavigation() {
    if (document.getElementById(contextNavigationId)) {
        return true;
    }

    const header = document.querySelector('body > header');
    if (!header) {
        return false;
    }

    const context = await getLibraryNavigationContext();
    if (!context) {
        document.getElementById(contextNavigationPlaceholderId)?.remove();
        document.body.classList.remove('docs-has-context-navigation');
        document.body.style.removeProperty('--docs-header-height');
        return true;
    }

    const navigation = document.createElement('nav');
    navigation.id = contextNavigationId;
    navigation.classList.add('docs-context-navigation');
    navigation.setAttribute(
        'aria-label',
        `${context.library.name} documentation`);

    const container = document.createElement('div');
    container.classList.add('container-xxl', 'docs-context-navigation-inner');

    const libraryLink = document.createElement('a');
    libraryLink.classList.add('docs-context-library');
    libraryLink.href = new URL('index.html', context.conceptualRootUrl);
    libraryLink.textContent = context.library.name;

    const versionSelectorPlaceholder = document.createElement('div');
    versionSelectorPlaceholder.id = versionSelectorPlaceholderId;
    versionSelectorPlaceholder.classList.add(
        'docs-version-selector-placeholder');
    versionSelectorPlaceholder.setAttribute('aria-hidden', 'true');

    const links = document.createElement('div');
    links.classList.add('docs-context-links');

    const defaultConceptualSections = [
        {
            key: 'concepts',
            label: context.russian ? 'Концепции' : 'Concepts',
            url: new URL('concepts/index.html', context.conceptualRootUrl)
        },
        {
            key: 'tutorials',
            label: context.russian ? 'Учебники' : 'Tutorials',
            url: new URL('tutorials/index.html', context.conceptualRootUrl)
        },
        {
            key: 'how-to-guides',
            label: context.russian ? 'Руководства' : 'How-to Guides',
            url: new URL('how-to-guides/index.html', context.conceptualRootUrl)
        }
    ];
    const configuredConceptualSections =
        context.library.navigationByVersion?.[context.versionPath];
    const conceptualSections = Array.isArray(configuredConceptualSections)
        ? configuredConceptualSections.map(section => ({
            key: section.key,
            label: context.russian
                ? (section.labelRu ?? section.label)
                : section.label,
            url: new URL(section.path, context.conceptualRootUrl)
        }))
        : defaultConceptualSections;
    const sections = [
        ...conceptualSections,
        {
            key: 'reference',
            label: context.russian ? 'Справочник' : 'References',
            url: new URL(context.library.referencePage, context.referenceRootUrl)
        }
    ];

    for (const section of sections) {
        const link = document.createElement('a');
        link.classList.add('docs-context-link');
        link.href = section.url;
        link.textContent = section.label;

        if (section.key === 'reference' && context.russian) {
            const referenceUrl = new URL(link.href);
            referenceUrl.searchParams.set('lang', 'ru');
            link.href = referenceUrl;
        }

        if (context.activeSection === section.key) {
            link.classList.add('active');
            link.setAttribute('aria-current', 'page');
        }

        links.appendChild(link);
    }

    container.append(libraryLink, versionSelectorPlaceholder, links);
    navigation.appendChild(container);
    const navigationPlaceholder = document.getElementById(
        contextNavigationPlaceholderId);
    if (navigationPlaceholder) {
        navigationPlaceholder.replaceWith(navigation);
    } else {
        header.appendChild(navigation);
    }
    document.body.classList.add('docs-has-context-navigation');

    const synchronizeHeaderHeight = () => {
        document.body.style.setProperty(
            '--docs-header-height',
            `${header.offsetHeight}px`);
    };

    synchronizeHeaderHeight();

    if (typeof ResizeObserver !== 'undefined') {
        const observer = new ResizeObserver(synchronizeHeaderHeight);
        observer.observe(header);
    }

    return true;
}

async function addVersionSelector() {
    const containerId = `${selectorId}-container`;
    if (document.getElementById(selectorId) || document.getElementById(containerId)) {
        return true;
    }

    const navigation = document.getElementById(contextNavigationId);
    const libraryLink = navigation?.querySelector('.docs-context-library');
    if (!libraryLink) {
        return false;
    }

    const context = await getVersionContext();
    if (!context) {
        document.getElementById(versionSelectorPlaceholderId)?.remove();
        return true;
    }

    const container = document.createElement('div');
    container.id = containerId;
    container.classList.add('docs-version-selector');

    const label = document.createElement('label');
    label.classList.add('visually-hidden');
    label.htmlFor = selectorId;
    label.textContent = 'Package version';

    const select = document.createElement('select');
    select.id = selectorId;
    select.classList.add('form-select', 'form-select-sm');
    select.setAttribute('aria-label', 'Package version');

    for (const version of context.versions) {
        const option = document.createElement('option');
        option.value = version.path;
        option.textContent = version.name;
        option.selected = version.path === context.versionPath
            || version.aliases?.includes(context.versionPath);
        select.appendChild(option);
    }

    select.addEventListener('change', async () => {
        const currentVersion = context.versions.find(version =>
            version.path === context.versionPath
            || version.aliases?.includes(context.versionPath));
        if (select.value === currentVersion?.path) {
            return;
        }

        const selectedVersion = context.versions.find(
            version => version.path === select.value);
        const referenceOnlyTarget = selectedVersion?.referenceOnly && !context.apiPage;
        let targetUrl = new URL(
            referenceOnlyTarget
                ? `${encodeURIComponent(select.value)}/${context.referencePage}`
                : `${encodeURIComponent(select.value)}/${context.pagePath}`,
            referenceOnlyTarget
                ? context.apiVersionRootUrl
                : context.versionRootUrl);
        targetUrl.search = window.location.search;
        targetUrl.hash = window.location.hash;

        if (referenceOnlyTarget && context.language === 'ru' &&
            !targetUrl.searchParams.has('lang')) {
            targetUrl.searchParams.set('lang', 'ru');
        }

        if (referenceOnlyTarget) {
            window.location.assign(targetUrl);
            return;
        }

        const fallbackRootUrl = selectedVersion?.referenceOnly
            ? context.apiVersionRootUrl
            : context.versionRootUrl;
        const fallbackPagePath = selectedVersion?.referenceOnly
            ? context.referencePage
            : context.fallbackPagePath;

        try {
            const response = await fetch(targetUrl, { method: 'HEAD' });
            if (!response.ok) {
                targetUrl = new URL(
                    `${encodeURIComponent(select.value)}/${fallbackPagePath}`,
                    fallbackRootUrl);
                targetUrl.search = window.location.search;
                targetUrl.hash = window.location.hash;
            }
        } catch {
            targetUrl = new URL(
                `${encodeURIComponent(select.value)}/${fallbackPagePath}`,
                fallbackRootUrl);
            targetUrl.search = window.location.search;
            targetUrl.hash = window.location.hash;
        }

        window.location.assign(targetUrl);
    });

    container.append(label, select);
    const placeholder = document.getElementById(versionSelectorPlaceholderId);
    if (placeholder) {
        placeholder.replaceWith(container);
    } else {
        libraryLink.insertAdjacentElement('afterend', container);
    }

    return true;
}

async function addLanguageSelector() {
    const containerId = `${languageSelectorId}-container`;
    if (document.getElementById(languageSelectorId) || document.getElementById(containerId)) {
        return true;
    }

    const iconBar = document.querySelector('#navbar form.icons');
    if (!iconBar) {
        return false;
    }

    const item = document.createElement('div');
    item.id = containerId;
    item.classList.add('dropdown', 'docs-language-selector');
    iconBar.insertBefore(item, iconBar.lastElementChild);

    const context = await getLanguageContext();
    if (!context) {
        item.remove();
        return true;
    }

    const button = document.createElement('button');
    button.id = languageSelectorId;
    button.type = 'button';
    button.classList.add('btn', 'border-0', 'docs-language-button');
    button.dataset.bsToggle = 'dropdown';
    button.setAttribute('aria-expanded', 'false');
    button.setAttribute('aria-label', 'Change language');
    button.title = 'Change language';
    button.innerHTML = '<i class="bi bi-translate"></i>';

    const menu = document.createElement('ul');
    menu.classList.add('dropdown-menu', 'dropdown-menu-end');

    for (const language of context.languages) {
        const menuItem = document.createElement('li');
        const link = document.createElement('a');
        const isCurrentLanguage = language.code === context.currentLanguage.code;
        const languagePrefix = language.path ? `${language.path}/` : '';
        const targetUrl = context.apiPage
            ? new URL(context.pagePath, context.rootUrl)
            : new URL(`${languagePrefix}${context.pagePath}`, context.rootUrl);

        targetUrl.search = window.location.search;
        targetUrl.hash = window.location.hash;

        if (context.apiPage) {
            if (language.code === 'ru') {
                targetUrl.searchParams.set('lang', 'ru');
            } else {
                targetUrl.searchParams.delete('lang');
            }
        }

        link.classList.add('dropdown-item');
        link.href = targetUrl;
        link.hreflang = language.code;
        link.lang = language.code;
        link.textContent = language.name;

        if (isCurrentLanguage) {
            link.classList.add('active');
            link.setAttribute('aria-current', 'page');
        } else {
            link.addEventListener('click', async event => {
                event.preventDefault();
                setLanguagePreference(language.code);
                let resolvedUrl = targetUrl;

                try {
                    const response = await fetch(resolvedUrl, { method: 'HEAD' });
                    if (!response.ok) {
                        resolvedUrl = context.apiPage
                            ? new URL('api/index.html', context.rootUrl)
                            : new URL(`${languagePrefix}index.html`, context.rootUrl);
                    }
                } catch {
                    resolvedUrl = context.apiPage
                        ? new URL('api/index.html', context.rootUrl)
                        : new URL(`${languagePrefix}index.html`, context.rootUrl);
                }

                window.location.assign(resolvedUrl);
            });
        }

        menuItem.appendChild(link);
        menu.appendChild(menuItem);
    }

    item.append(button, menu);

    return true;
}

function addRepositoryLink() {
    if (document.getElementById(repositoryLinkId)) {
        return true;
    }

    const search = document.getElementById('search');
    if (!search) {
        return false;
    }

    const link = document.createElement('a');
    link.id = repositoryLinkId;
    link.classList.add('btn', 'border-0', 'docs-repository-link');
    link.href = 'https://github.com/Akeldov/Akeldov.Math';
    link.target = '_blank';
    link.rel = 'noopener noreferrer';
    link.title = 'Akeldov.Math repository';
    link.setAttribute('aria-label', 'Akeldov.Math repository');
    link.innerHTML = '<i class="bi bi-github"></i>';

    search.insertAdjacentElement('afterend', link);

    return true;
}

async function initializeSelectors() {
    const [versionReady, languageReady, navigationReady] = await Promise.all([
        addVersionSelector(),
        addLanguageSelector(),
        addLibraryNavigation()
    ]);
    const repositoryReady = addRepositoryLink();

    return versionReady
        && languageReady
        && navigationReady
        && repositoryReady;
}

function start() {
    reserveLibraryNavigationSpace();
    synchronizeLanguagePreference();
    localizeReferenceOverview();
    startRussianLocalization();

    let initializationRunning = false;
    let initializationPending = true;

    const observer = new MutationObserver(() => {
        initializationPending = true;
        void initializeDynamicNavigation();
    });

    async function initializeDynamicNavigation() {
        if (initializationRunning) {
            return;
        }

        initializationRunning = true;

        do {
            initializationPending = false;

            if (await initializeSelectors()) {
                observer.disconnect();
                initializationRunning = false;
                return;
            }
        } while (initializationPending);

        initializationRunning = false;
    }

    observer.observe(document.body, { childList: true, subtree: true });
    void initializeDynamicNavigation();
}

function getSearchConfiguration() {
    const segments = window.location.pathname.split('/').filter(Boolean);
    const versionIndex = segments.findIndex(
        segment => /^\d+\.\d+\.\d+$/.test(segment));
    const language = hasRussianLanguageContext() ? 'ru' : 'en';
    const priorityPrefixes = [];

    if (versionIndex > 0) {
        const library = segments[versionIndex - 1];
        if (library === 'Hexes' || library === 'Spatial2D') {
            const version = segments[versionIndex];
            priorityPrefixes.push([
                `${language}/${library}/${version}/`,
                `api/${library}/${version}/`
            ]);
        }
    }

    priorityPrefixes.push([`${language}/`, 'api/']);

    return {
        searchIndexPath: '../search/all.json',
        searchPriorityPrefixes: priorityPrefixes
    };
}

const searchConfiguration = getSearchConfiguration();

export default {
    lunrLanguages: ['en', 'ru'],
    searchIndexPath: searchConfiguration.searchIndexPath,
    searchPriorityPrefixes: searchConfiguration.searchPriorityPrefixes,
    start
};
