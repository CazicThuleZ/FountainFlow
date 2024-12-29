function showToast(title, message, type = 'success') {
    const toastId = 'toast-' + Date.now();
    const backgroundColor = type === 'success' ? 'bg-success' : 'bg-danger';
    const icon = type === 'success' ? 'fa-check-circle' : 'fa-exclamation-circle';

    const toastHtml = `
        <div id="${toastId}" class="toast" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="toast-header ${backgroundColor} text-white">
                <i class="fas ${icon} me-2"></i>
                <strong class="me-auto">${title}</strong>
                <button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
            <div class="toast-body">
                ${message}
            </div>
        </div>`;

    // Ensure toast container exists
    if ($('.toast-container').length === 0) {
        $('body').append('<div class="toast-container position-fixed top-0 end-0 p-3"></div>');
    }

    $('.toast-container').append(toastHtml);
    const toastElement = document.getElementById(toastId);
    const toast = new bootstrap.Toast(toastElement, { delay: 3000 });
    toast.show();

    // Remove toast element after it's hidden
    toastElement.addEventListener('hidden.bs.toast', function () {
        $(this).remove();
    });
}