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
    // navigator.clipboard requires a secure context (HTTPS or localhost) — it's
    // undefined on plain-HTTP LAN access (e.g. http://192.168.x.x), so fall back
    // to the legacy execCommand technique there.

    copyToClipboard: async function (text) {
        if (window.isSecureContext && navigator.clipboard) {
            try {
                await navigator.clipboard.writeText(text);
                return true;
            } catch {
                // fall through to legacy fallback
            }
        }

        try {
            const textarea = document.createElement('textarea');
            textarea.value = text;
            textarea.style.position = 'fixed';
            textarea.style.left = '-9999px';
            document.body.appendChild(textarea);
            textarea.focus();
            textarea.select();
            const success = document.execCommand('copy');
            document.body.removeChild(textarea);
            return success;
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
