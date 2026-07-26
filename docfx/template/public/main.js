const selectorId = 'akeldov-docs-version';
const languageSelectorId = 'akeldov-docs-language';
const repositoryLinkId = 'akeldov-repository-link';

async function fetchJson(url) {
    try {
        const response = await fetch(url);
        return response.ok ? await response.json() : null;
    } catch {
        return null;
    }
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

    if (!Array.isArray(versionData?.versions) || versionData.versions.length === 0) {
        return null;
    }

    return {
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
        const currentLanguage = localizedLanguage
            ?? registry.languages.find(language => !language.path)
            ?? registry.languages[0];
        const pagePath = localizedLanguage
            ? segments.slice(1).join('/')
            : segments.join('/');

        if (pagePath === 'api' || pagePath.startsWith('api/')) {
            return null;
        }

        return {
            currentLanguage,
            languages: registry.languages,
            pagePath: pagePath || 'index.html',
            rootUrl
        };
    }

    return null;
}

async function addVersionSelector() {
    const containerId = `${selectorId}-container`;
    if (document.getElementById(selectorId) || document.getElementById(containerId)) {
        return true;
    }

    const toc = document.getElementById('toc');
    if (!toc) {
        return false;
    }

    const container = document.createElement('div');
    container.id = containerId;
    container.classList.add('docs-version-selector');
    toc.prepend(container);

    const context = await getVersionContext();
    if (!context) {
        container.remove();
        return true;
    }

    const label = document.createElement('label');
    label.classList.add('form-label');
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
        option.selected = version.path === context.versionPath;
        select.appendChild(option);
    }

    select.addEventListener('change', async () => {
        if (select.value === context.versionPath) {
            return;
        }

        let targetUrl = new URL(
            `${encodeURIComponent(select.value)}/${context.pagePath}`,
            context.versionRootUrl);

        try {
            const response = await fetch(targetUrl, { method: 'HEAD' });
            if (!response.ok) {
                targetUrl = new URL(
                    `${encodeURIComponent(select.value)}/index.html`,
                    context.versionRootUrl);
            }
        } catch {
            targetUrl = new URL(
                `${encodeURIComponent(select.value)}/index.html`,
                context.versionRootUrl);
        }

        window.location.assign(targetUrl);
    });

    container.append(label, select);

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
        const targetUrl = new URL(`${languagePrefix}${context.pagePath}`, context.rootUrl);

        targetUrl.search = window.location.search;
        targetUrl.hash = window.location.hash;

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
                let resolvedUrl = targetUrl;

                try {
                    const response = await fetch(resolvedUrl, { method: 'HEAD' });
                    if (!response.ok) {
                        resolvedUrl = new URL(`${languagePrefix}index.html`, context.rootUrl);
                    }
                } catch {
                    resolvedUrl = new URL(`${languagePrefix}index.html`, context.rootUrl);
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
    const [versionReady, languageReady] = await Promise.all([
        addVersionSelector(),
        addLanguageSelector()
    ]);
    const repositoryReady = addRepositoryLink();

    return versionReady && languageReady && repositoryReady;
}

function start() {
    initializeSelectors().then(ready => {
        if (ready) {
            return;
        }

        const observer = new MutationObserver(async () => {
            if (await initializeSelectors()) {
                observer.disconnect();
            }
        });

        observer.observe(document.body, { childList: true, subtree: true });
    });
}

export default {
    start
};
