window.ruptura = {

    // ── Theme ─────────────────────────────────────────────────────────────────

    setTheme: function (mode) {
        const html = document.documentElement;
        if (mode === 'system') {
            html.removeAttribute('data-theme');
            html.removeAttribute('data-bs-theme');
        } else {
            html.setAttribute('data-theme', mode);
            html.setAttribute('data-bs-theme', mode);
        }
    },

    getSystemPreference: function () {
        return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
    },

    // ── Clipboard ─────────────────────────────────────────────────────────────

    copyToClipboard: async function (text) {
        try {
            await navigator.clipboard.writeText(text);
            return true;
        } catch {
            return false;
        }
    }
};

// Apply theme immediately on script load to avoid flash-of-wrong-theme.
// (Also done inline in index.html for the earliest possible moment.)
(function () {
    const stored = localStorage.getItem('ruptura_theme') || 'system';
    if (stored !== 'system') {
        document.documentElement.setAttribute('data-theme', stored);
        document.documentElement.setAttribute('data-bs-theme', stored);
    }
})();
