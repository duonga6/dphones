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
    const cartElement = $("#cart-quantity");
    if (qtt == 0)
        cartElement.css("display", "none");
    else {
        cartElement.css("display", "flex");
        cartElement.html(qtt);
    }
}

const BuyNow = function () {
    const productId = Number($("#product-id").val());
    const colorId = $(".color-item.active").data("id");
    const capaId = $(".price-item.active").data("id");

    if (!productId || !colorId || !capaId) {
        showToast(0, "Có lỗi xảy ra")
        return;
    }

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
            if (data.qtt > 0) updateCartQuantity(data.qtt)

            let url = '/order';
            url += `?cartId=${data.id}`
            window.location.href = url;
        },
        error: function () {
            {
                showToast(0, "Có lỗi xảy ra!");
            }
        }
    });
}

const AddToCart = function () {
    const productId = Number($("#product-id").val());
    const colorId = $(".color-item.active").data("id");
    const capaId = $(".price-item.active").data("id");

    if (!productId || !colorId || !capaId) {
        showToast(0, "Có lỗi xảy ra")
        return;
    }

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

//

const btnShowDesc = $("#btn-show-desc");
const btnHideDesc = $("#btn-hide-desc");
const descContent = $("#product-description");

btnShowDesc.click(function () {
    btnHideDesc.show();
    btnShowDesc.hide();
    descContent.addClass("active");
});

btnHideDesc.click(function () {
    btnShowDesc.show();
    btnHideDesc.hide();
    descContent.removeClass("active");
});

//

const resetActice = function () {
    $(".color-item").removeClass("active");
    $(".prices-list").removeClass("active");
    $(".price-item").removeClass("active");
}

const setActive = function (colorId, capaId = null) {
    $(".color-item").each((i, e) => {
        if ($(e).data("id") == colorId) $(e).addClass("active");
    });

    $(".prices-list").each((i, e) => {
        if ($(e).data("id") == colorId) {
            $(e).addClass("active");
            if (capaId) {
                $(e).children(".price-item").each((index, element) => {
                    if ($(element).data("id") == capaId) {
                        $(element).addClass("active");
                    }
                });
            } else {
                $(e).children(".price-item").first().addClass("active");
            }
        }
    });

    $(`#btn-show-color-image-${colorId}`).click();
}

const chooseOption = function (colorId, capaId = null) {
    resetActice();
    setActive(colorId, capaId);
}


