const selectorId = 'akeldov-docs-version';

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

async function addVersionSelector() {
    if (document.getElementById(selectorId)) {
        return true;
    }

    const toc = document.getElementById('toc');
    if (!toc) {
        return false;
    }

    const context = await getVersionContext();
    if (!context) {
        return true;
    }

    const container = document.createElement('div');
    container.classList.add('docs-version-selector');

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
    toc.prepend(container);

    return true;
}

function start() {
    addVersionSelector().then(added => {
        if (added) {
            return;
        }

        const observer = new MutationObserver(async () => {
            if (await addVersionSelector()) {
                observer.disconnect();
            }
        });

        observer.observe(document.body, { childList: true, subtree: true });
    });
}

export default {
    start
};
