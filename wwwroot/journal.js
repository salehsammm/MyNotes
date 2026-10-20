export function openEpisode(dialog) {
    if (!dialog.open) dialog.showModal();
    // Blazor updates the dialog fields after the click handler finishes.
    requestAnimationFrame(() => {
        const rating = dialog.querySelector('#episode-rating, #season-number');
        rating?.focus();
        rating?.select();
    });
}

export function closeEpisode(dialog) {
    if (dialog.open) dialog.close();
}

const saveShortcuts = new Map();

export function registerSaveShortcut(formId) {
    unregisterSaveShortcut(formId);
    const handler = event => {
        if (event.defaultPrevented || !(event.ctrlKey || event.metaKey) || event.altKey || event.key.toLowerCase() !== 's') return;
        const form = document.getElementById(formId);
        if (!form?.isConnected || document.querySelector('dialog[open]')) return;
        event.preventDefault();
        if (event.repeat || form.querySelector('button[type="submit"]')?.disabled) return;
        // Flush the focused field's current value through Blazor before submitting.
        const focused = document.activeElement;
        if (form.contains(focused)) focused.dispatchEvent(new Event('change', { bubbles: true }));
        form.requestSubmit();
    };
    saveShortcuts.set(formId, handler);
    document.addEventListener('keydown', handler);
}

export function unregisterSaveShortcut(formId) {
    const handler = saveShortcuts.get(formId);
    if (!handler) return;
    document.removeEventListener('keydown', handler);
    saveShortcuts.delete(formId);
}
