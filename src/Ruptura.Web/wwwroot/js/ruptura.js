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
    },

    // ── Keyboard shortcuts ───────────────────────────────────────────────────

    bindGlobalEscape: function (dotNetRef) {
        const handler = function (e) {
            if (e.key === 'Escape') {
                dotNetRef.invokeMethodAsync('OnGlobalEscape');
            }
        };
        document.addEventListener('keydown', handler);
        window._rupturaEscapeHandler = handler;
    },

    unbindGlobalEscape: function () {
        if (window._rupturaEscapeHandler) {
            document.removeEventListener('keydown', window._rupturaEscapeHandler);
            delete window._rupturaEscapeHandler;
        }
    },

    bindSearchShortcut: function (inputElement) {
        const handler = function (e) {
            // '/' is a literal character, so it must not hijack typing in any
            // other field. Ctrl+K/Cmd+K's whole purpose is to jump to search
            // from anywhere — including from inside another input — so it is
            // intentionally exempt from the "typing" guard.
            if (e.key === '/') {
                const active = document.activeElement;
                const typing = active && (active.tagName === 'INPUT' || active.tagName === 'TEXTAREA' || active.isContentEditable);
                if (typing) return;
                e.preventDefault();
                inputElement.focus();
                return;
            }
            if (e.key && e.key.toLowerCase() === 'k' && (e.ctrlKey || e.metaKey)) {
                e.preventDefault();
                inputElement.focus();
            }
        };
        document.addEventListener('keydown', handler);
        inputElement._rupturaShortcutHandler = handler;
    },

    unbindSearchShortcut: function (inputElement) {
        if (inputElement && inputElement._rupturaShortcutHandler) {
            document.removeEventListener('keydown', inputElement._rupturaShortcutHandler);
            delete inputElement._rupturaShortcutHandler;
        }
    },

    // ── Lightbox (modal) ──────────────────────────────────────────────────────
    // Locks background scroll and traps Tab focus inside the overlay while open.
    // Only one lightbox exists at a time, so trap state is held module-level and
    // released without needing the (by-then-detached) element reference.

    openLightbox: function (container) {
        document.body.classList.add('lightbox-open');
        window._rupturaLightboxContainer = container;
        window._rupturaLightboxTrap = function (e) {
            if (e.key !== 'Tab') return;
            const focusable = container.querySelectorAll(
                'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])');
            if (focusable.length === 0) { e.preventDefault(); return; }
            const first = focusable[0];
            const last = focusable[focusable.length - 1];
            if (e.shiftKey && document.activeElement === first) { e.preventDefault(); last.focus(); }
            else if (!e.shiftKey && document.activeElement === last) { e.preventDefault(); first.focus(); }
        };
        container.addEventListener('keydown', window._rupturaLightboxTrap);
    },

    closeLightbox: function () {
        document.body.classList.remove('lightbox-open');
        if (window._rupturaLightboxContainer && window._rupturaLightboxTrap) {
            window._rupturaLightboxContainer.removeEventListener('keydown', window._rupturaLightboxTrap);
        }
        window._rupturaLightboxContainer = null;
        window._rupturaLightboxTrap = null;
    },

    // ── Searchable select ─────────────────────────────────────────────────────
    // Pins the open option list to its input with position:fixed, so it isn't
    // clipped by a scrolling ancestor (.ledger-table-wrap is overflow-x:auto),
    // flips it above the input when there's no room below, and keeps the
    // keyboard-active option scrolled into view. Called after every render while
    // the list is open; the scroll/resize listeners remove themselves once the
    // list element has left the DOM.

    placeSearchableList: function (input, list) {
        if (!input || !list) return;

        if (!list._rupturaPlace) {
            list._rupturaPlace = function () {
                if (!list.isConnected) {
                    window.removeEventListener('scroll', list._rupturaPlace, true);
                    window.removeEventListener('resize', list._rupturaPlace);
                    return;
                }
                const rect = input.getBoundingClientRect();
                const below = window.innerHeight - rect.bottom;
                const above = below < list.offsetHeight + 4 && rect.top > below;
                list.style.position = 'fixed';
                list.style.marginTop = '0';
                list.style.left = rect.left + 'px';
                list.style.right = 'auto';
                list.style.width = rect.width + 'px';
                list.style.top = above ? 'auto' : (rect.bottom + 2) + 'px';
                list.style.bottom = above ? (window.innerHeight - rect.top + 2) + 'px' : 'auto';
            };
            window.addEventListener('scroll', list._rupturaPlace, true);
            window.addEventListener('resize', list._rupturaPlace);
        }
        list._rupturaPlace();

        const active = list.querySelector('.autocomplete-item.active');
        if (!active) return;
        if (active.offsetTop < list.scrollTop) {
            list.scrollTop = active.offsetTop;
        } else if (active.offsetTop + active.offsetHeight > list.scrollTop + list.clientHeight) {
            list.scrollTop = active.offsetTop + active.offsetHeight - list.clientHeight;
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
