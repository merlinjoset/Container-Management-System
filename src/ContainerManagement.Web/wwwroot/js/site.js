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
