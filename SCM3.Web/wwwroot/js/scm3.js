window.scm3 = {
    scrollToElement: function (elementId) {
        document.getElementById(elementId)?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    },

    // Live theme switch (root CLAUDE.md §13): swaps the kendo-theme-* stylesheet that
    // drives Telerik component theming, without a page reload.
    applyTheme: function (cssHref, isDark) {
        const link = document.getElementById('telerik-theme-css');
        if (link) {
            link.href = cssHref;
        }

        const mode = isDark ? 'dark' : 'light';
        document.documentElement.dataset.scm3Theme = mode;
        document.documentElement.dataset.bsTheme = mode;
        document.documentElement.style.colorScheme = mode;
    }
};
