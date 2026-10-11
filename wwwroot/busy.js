(() => {
    if (window.appBusy) return;
    let navigating = false;
    let overlay = document.getElementById('app-busy-overlay');
    let message = document.getElementById('app-busy-message');
    let workspace = document.getElementById('app-workspace');
    if (!overlay || !message || !workspace) return;

    function refresh() {
        overlay = document.getElementById('app-busy-overlay');
        message = document.getElementById('app-busy-message');
        workspace = document.getElementById('app-workspace');
        if (!overlay || !message || !workspace) return;
        const marker = workspace.querySelector('[data-app-busy]');
        // An interactive page's own loading state takes over from navigation.
        if (marker?.getAttribute('data-app-busy')?.startsWith('Loading')) navigating = false;
        const circuitFailed = document.getElementById('blazor-error-ui')?.style.display === 'block';
        const busy = !circuitFailed && (navigating || !!marker);
        const text = marker?.getAttribute('data-app-busy') || 'Loading page…';
        if (message.textContent !== text) message.textContent = text;
        overlay.hidden = !busy;
        workspace.inert = busy;
        workspace.setAttribute('aria-busy', String(busy));
    }

    window.appBusy = {
        isBusy: () => !overlay.hidden,
        beginNavigation: () => { navigating = true; refresh(); },
        endNavigation: () => { navigating = false; refresh(); }
    };

    document.addEventListener('click', event => {
        if (window.appBusy.isBusy()) { event.preventDefault(); event.stopImmediatePropagation(); return; }
        const link = event.target.closest?.('a[href]');
        if (!link || event.defaultPrevented || event.button !== 0 || event.ctrlKey || event.metaKey || event.shiftKey || event.altKey || link.hasAttribute('download') || (link.target && link.target !== '_self')) return;
        const url = new URL(link.href, location.href);
        if (url.origin !== location.origin || !['http:', 'https:'].includes(url.protocol)) return;
        if (url.pathname === location.pathname && url.search === location.search) return;
        window.appBusy.beginNavigation();
    }, true);

    document.addEventListener('keydown', event => {
        if (!window.appBusy.isBusy()) return;
        // Keep browser shortcuts available, but block app shortcuts and form actions.
        if (event.key === 'Escape' || event.key === 'Enter' || event.key === 'Tab' || ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 's')) {
            event.preventDefault(); event.stopImmediatePropagation();
        }
    }, true);

    new MutationObserver(refresh).observe(document.body, { childList: true, subtree: true, attributes: true, attributeFilter: ['data-app-busy', 'style'] });
    function pageArrived() {
        const content = document.querySelector('main.content');
        navigating = !!content && content.children.length === 0;
        refresh();
    }
    window.Blazor?.addEventListener('enhancedload', pageArrived);
    window.addEventListener('pageshow', pageArrived);
    pageArrived();
    refresh();
})();
