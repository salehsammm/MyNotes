const assert = require('node:assert/strict');
const fs = require('node:fs');
const vm = require('node:vm');

const listeners = {};
const blazorListeners = {};
let observe;
let marker = null;
const nodes = {
    'app-busy-overlay': { hidden: true },
    'app-busy-message': { textContent: '' },
    'app-workspace': { inert: false, setAttribute() {}, querySelector: () => marker },
    'blazor-error-ui': { style: { display: 'none' } }
};
const context = {
    URL,
    location: new URL('https://localhost:7048/moviereview/scenes'),
    document: {
        body: {},
        getElementById: id => nodes[id],
        querySelector: () => null,
        addEventListener: (name, handler) => listeners[name] = handler
    },
    MutationObserver: class { constructor(callback) { observe = callback; } observe() {} },
    window: {
        Blazor: { addEventListener: (name, handler) => blazorListeners[name] = handler },
        addEventListener: (name, handler) => listeners[name] = handler
    }
};
vm.runInNewContext(fs.readFileSync(require('node:path').join(__dirname, '../wwwroot/busy.js'), 'utf8'), context);
const event = (extra = {}) => ({
    target: { closest: () => null }, button: 0,
    preventDefault() { this.prevented = true; },
    stopImmediatePropagation() { this.stopped = true; }, ...extra
});
const link = { href: 'https://localhost:7048/moviereview/scenes/1', target: '', hasAttribute: () => false };
listeners.click(event({ ctrlKey: true, target: { closest: () => link } }));
assert.equal(context.window.appBusy.isBusy(), false, 'Opening a new tab must not lock this page');
listeners.click(event({ target: { closest: () => link } }));
assert.equal(nodes['app-workspace'].inert, true, 'Navigation must lock inputs immediately');
const secondClick = event();
listeners.click(secondClick);
assert.ok(secondClick.prevented && secondClick.stopped, 'A second action must be blocked');
const saveShortcut = event({ key: 's', ctrlKey: true });
listeners.keydown(saveShortcut);
assert.ok(saveShortcut.prevented, 'Ctrl+S must not start another save while busy');
marker = { getAttribute: () => 'Loading scene…' };
blazorListeners.enhancedload();
assert.equal(context.window.appBusy.isBusy(), true, 'Page arrival must not unlock unfinished loading');
assert.equal(nodes['app-busy-message'].textContent, 'Loading scene…');
marker = null;
observe();
assert.equal(nodes['app-workspace'].inert, false, 'Finished loading must unlock the page');
nodes['app-workspace'] = { inert: false, setAttribute() {}, querySelector: () => marker };
marker = { getAttribute: () => 'Saving…' };
observe();
assert.equal(nodes['app-workspace'].inert, true, 'Enhanced navigation may replace DOM nodes');
nodes['blazor-error-ui'].style.display = 'block';
observe();
assert.equal(context.window.appBusy.isBusy(), false, 'An app failure must leave recovery controls reachable');
console.log('Busy navigation, input blocking, page loading, DOM replacement, and error recovery passed.');
