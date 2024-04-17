export const showToast = {
    show(status, message) {
        const toastElement = document.getElementById("liveToast");
        const toast = new bootstrap.Toast(toastElement);
        const iconClass = status ? "text-success fa-solid fa-circle-check" : "text-warning fs-6 fa-solid fa-circle-exclamation";
        const toastHtml = `
                                                <div class="toast-header align-items-center">
                                                    <i class="${iconClass}"></i>
                                                    <span class="ms-2 me-auto fs-6">${message}</span>
                                                    <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>
                                                </div>
                                            `;
        toastElement.html("");
        toastElement.append(toastHtml);
        toast.show();
    }
} 