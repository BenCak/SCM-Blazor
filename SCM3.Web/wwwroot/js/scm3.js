window.scm3 = {
    scrollToElement: function (elementId) {
        document.getElementById(elementId)?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
};
