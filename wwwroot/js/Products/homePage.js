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

const updateCartQuantity = function (qtt) {
    const cartElement = $(".cart-quantity");
    if (qtt == 0)
        cartElement.css("display", "none");
    else {
        cartElement.css("display", "flex");
        cartElement.html(qtt);
    }
}

const AddToCart = function (productId, colorId = null, capaId = null) {

    var dataRequest = {
        productId,
        colorId,
        capaId
    }

    $.ajax({
        type: "post",
        url: '/cart/add-to-cart',
        data: JSON.stringify(dataRequest),
        contentType: "application/json",
        processData: false,
        cache: false,
        success: function (data) {
            showToast(data.status, data.message);
            if (data.qtt > 0) updateCartQuantity(data.qtt)
        },
        error: function () {
            {
                showToast(0, "Có lỗi xảy ra!");
            }
        }
    });
}

// Scroll Betseler table mobile

const leftArrow = $(".left-arrow");
const rightArrow = $(".right-arrow");
const gridArrow = $(".product-bestseller .product_grid");

leftArrow.click(() => {
    gridArrow.animate({
        scrollLeft: "-=" + (gridArrow.children(".item").width() + 13)
    }, 200);
});

rightArrow.click(() => {
    gridArrow.animate({
        scrollLeft: "+=" + (gridArrow.children(".item").width() + 13)
    }, 200);
});
