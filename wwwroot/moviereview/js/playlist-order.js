const bindings = new WeakMap();

export function attach(root, receiver) {
    detach(root);
    let dragged = null;
    function clearHighlight() {
        root.querySelectorAll('.drop-target').forEach(row => row.classList.remove('drop-target'));
    }
    function target(event) {
        const row = event.target.closest('.playlist-entry');
        const list = row?.closest('[data-playlist-id]');
        return row && root.contains(row) && dragged && list.dataset.playlistId === dragged.playlistId ? row : null;
    }
    function start(event) {
        const handle = event.target.closest('.playlist-grip');
        if (!handle || root.disabled) { event.preventDefault(); return; }
        const row = handle.closest('.playlist-entry');
        dragged = { playlistId: row.closest('[data-playlist-id]').dataset.playlistId, sceneId: row.dataset.sceneId };
        event.dataTransfer.effectAllowed = 'move';
        event.dataTransfer.setData('text/plain', dragged.sceneId);
    }
    function over(event) {
        const row = target(event);
        if (!row || root.disabled) return;
        event.preventDefault();
        event.dataTransfer.dropEffect = 'move';
        clearHighlight();
        row.classList.add('drop-target');
    }
    function end() { dragged = null; clearHighlight(); }
    async function drop(event) {
        const row = target(event);
        if (!row || root.disabled) { end(); return; }
        event.preventDefault();
        const move = dragged;
        const position = Array.from(row.parentElement.children).indexOf(row) + 1;
        end();
        await receiver.invokeMethodAsync('ReorderScene', Number(move.playlistId), Number(move.sceneId), position);
    }
    const listeners = { dragstart: start, dragover: over, dragend: end, drop };
    for (const [name, listener] of Object.entries(listeners)) root.addEventListener(name, listener);
    bindings.set(root, () => {
        end();
        for (const [name, listener] of Object.entries(listeners)) root.removeEventListener(name, listener);
    });
}

export function detach(root) { bindings.get(root)?.(); bindings.delete(root); }
