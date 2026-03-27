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
  var isFileDownload = false;

  function getLoader() {
    return document.getElementById('globalPageLoader');
  }

  function showLoader() {
    if (isFileDownload) return;
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

  function isDownloadLink(anchor) {
    if (!anchor) return false;
    if (anchor.hasAttribute('download')) return true;
    if (anchor.hasAttribute('data-no-loader')) return true;
    var href = (anchor.getAttribute('href') || '').trim().toLowerCase();
    return href.indexOf('/export') !== -1 || href.indexOf('/template') !== -1;
  }

  function shouldIgnoreLink(anchor) {
    if (!anchor) return true;
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
    if (event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) return;
    if (event.button && event.button !== 0) return;

    // If it's a file download link, set flag and skip loader
    if (isDownloadLink(anchor)) {
      isFileDownload = true;
      setTimeout(function () { isFileDownload = false; hideLoader(); }, 3000);
      return;
    }

    if (shouldIgnoreLink(anchor)) return;
    showLoader();
  }, true);

  document.addEventListener('submit', function (event) {
    if (event.defaultPrevented) return;
    showLoader();
  }, true);

  window.addEventListener('pageshow', function () {
    isFileDownload = false;
    hideLoader();
  });
  window.addEventListener('DOMContentLoaded', hideLoader);
  window.addEventListener('beforeunload', function () {
    if (isFileDownload) return;
    showLoader();
  });
})();
