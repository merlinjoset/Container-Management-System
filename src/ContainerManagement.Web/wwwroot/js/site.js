// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Initialize Bootstrap tooltips on demand
document.addEventListener('DOMContentLoaded', function () {
  if (window.bootstrap) {
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.forEach(function (el) {
      try { new bootstrap.Tooltip(el); } catch (e) { }
    });
  }
});

(function () {
  function getLoader() {
    return document.getElementById('globalPageLoader');
  }

  function showLoader() {
    var loader = getLoader();
    if (!loader) return;
    loader.classList.add('is-active');
    loader.setAttribute('aria-busy', 'true');
  }

  function hideLoader() {
    var loader = getLoader();
    if (!loader) return;
    loader.classList.remove('is-active');
    loader.setAttribute('aria-busy', 'false');
  }

  function shouldIgnoreLink(anchor) {
    if (!anchor) return true;
    if (anchor.hasAttribute('download')) return true;
    if (anchor.target && anchor.target.toLowerCase() === '_blank') return true;

    var href = anchor.getAttribute('href');
    if (!href) return true;

    var normalized = href.trim().toLowerCase();
    if (
      normalized.startsWith('#') ||
      normalized.startsWith('javascript:') ||
      normalized.startsWith('mailto:') ||
      normalized.startsWith('tel:')
    ) {
      return true;
    }

    return false;
  }

  document.addEventListener('click', function (event) {
    var anchor = event.target.closest('a[href]');
    if (!anchor) return;
    if (event.defaultPrevented) return;
    if (shouldIgnoreLink(anchor)) return;
    if (event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) return;
    if (event.button && event.button !== 0) return;
    showLoader();
  }, true);

  document.addEventListener('submit', function (event) {
    if (event.defaultPrevented) return;
    showLoader();
  }, true);

  window.addEventListener('pageshow', hideLoader);
  window.addEventListener('DOMContentLoaded', hideLoader);
  window.addEventListener('beforeunload', showLoader);
})();
