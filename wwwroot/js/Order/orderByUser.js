const toastElement = $('#liveToast');
const toast = new bootstrap.Toast(toastElement.get(0));
const showToast = function (status, message) {
    const iconClass = status == 1 ? "text-success fa-solid fa-circle-check" : "text-warning fs-6 fa-solid fa-circle-exclamation";
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

// Modal hủy đơn hàng
var modal = new bootstrap.Modal('#cancel-order-modal');
var idCancelInput = $("#orderId-canceled");
const openModal = function (Id) {
    idCancelInput.val(Id);
    modal.show();
}

var cancelNoteInput = $("#canceled-note");
const sendRequsetDelete = function () {
    const formData = new FormData();

    if (!cancelNoteInput.val()) {
        cancelNoteInput.focus();
        return;
    }

    formData.append("Id", idCancelInput.val());
    formData.append("note", cancelNoteInput.val());



    $.ajax({
        url: '/OrderManager/CancelOrderAPI',
        data: formData,
        processData: false,
        cache: false,
        contentType: false,
        type: 'POST',
        success: function (data) {
            modal.hide();
            showToast(data.status, data.message);
            if (data.status == 1) {
                setTimeout(function () {
                    window.location.reload();
                }, 2000);
            }
        },
        error: function () {

        }
    });
}


var activeOption = $(".order-option-item.active").get(0);
if (activeOption) {
    const optionsBox = $(".order-options");
    const offsetBox = optionsBox.get(0).offsetLeft;
    const scrollPosition = (activeOption.offsetLeft - offsetBox) / 2;
    optionsBox.animate({
        scrollLeft: scrollPosition
    }, 0);
}