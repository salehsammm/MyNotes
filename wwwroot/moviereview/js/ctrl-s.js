window.ctrlS = {
    _handlers: new Map(),
    register: function (id, dotNetRef, saveMethod, escMethod) {
        this.unregister(id);
        const handler = function (e) {
            if ((e.ctrlKey || e.metaKey) && (e.key === 's' || e.key === 'S')) {
                e.preventDefault();
                dotNetRef.invokeMethodAsync(saveMethod);
            } else if (e.key === 'Escape') {
                dotNetRef.invokeMethodAsync(escMethod);
            }
        };
        this._handlers.set(id, handler);
        document.addEventListener('keydown', handler);
    },
    unregister: function (id) {
        const h = this._handlers.get(id);
        if (h) {
            document.removeEventListener('keydown', h);
            this._handlers.delete(id);
        }
    }
};