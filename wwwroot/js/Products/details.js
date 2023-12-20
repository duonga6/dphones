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
        type: "get",
        url: `/check-capacity?capaId=${capaId}`,
        processData: false,
        cache: false,
        success: function (data) {
            if (data.status == 0) {
                showToast(0, data.message);
                return;
            }
            else {
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
        },
        error: function () {
            showToast(0, "Có lỗi xảy ra");
            return;
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

// Review comment

const updateRate = function (index) {

    $("#total-star-review").val(index);

    for (let i = 1; i <= index; i++) {
        $(`#star-index-${i}`).addClass("third-text").removeClass("text-muted");
    }

    for (let i = index + 1; i <= 5; i++) {
        $(`#star-index-${i}`).removeClass("third-text").addClass("text-muted");
    }
}

// send review

const sendReview = function () {
    const productId = Number($("#product-id").val());
    const content = $("#create-review-content").val();
    const star = Number($("#total-star-review").val());

    if (!productId || !star) {
        showToast(0, "Thiếu thông tin");
        return;
    }

    if (!content) {
        showToast(0, "Vui lòng nhập đánh giá");
        return;
    }

    const dataRequest = {
        productId: productId,
        content: content,
        rate: star
    }

    $.ajax({
        type: 'post',
        url: '/review',
        data: JSON.stringify(dataRequest),
        contentType: 'application/json',
        success: function (data) {
            showToast(1, "Gửi đánh giá thành công");
            $("#create-review-content").val("");
            $("#total-star-review").val(5);
            updateRate(5);
            loadReview();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            showToast(0, jqXHR.responseJSON.message);
        }
    })

}

const loadReview = function () {
    const productId = Number($("#product-id").val());
    console.log(productId);

    $.ajax({
        type: 'get',
        url: `/review/${productId}`,
        processData: false,
        contentType: false,
        caches: false,
        success: function (data) {
            let html = "";
            if (data.reviews) {
                data.reviews.forEach(item => {
                    rateHtml = "";

                    for (let i = 1; i <= item.rate; i++) {
                        rateHtml += `<i class="fa-solid fa-star third-text"></i>`;
                    }

                    for (let i = item.rate + 1; i <= 5; i++) {
                        rateHtml += `<i class="fa-solid fa-star text-muted"></i>`;
                    }

                    html += `
                    <li class="review-item">
                        <div class="heading d-flex align-items-center">
                            <img class="user-img" src="/files/UserAvatar/${item.image}" alt="">
                            <div class="d-flex flex-column ms-4">
                                <div class="user-info d-flex align-items-baseline">
                                    <p class="user-name mb-0 fw-semibold">${item.userName}</p>
                                    <p class="time mb-0">
                                        <i class="fa-regular fa-clock clock"></i>
                                        ${item.dateCreated}
                                    </p>
                                </div>
                                <div class="rate-star">
                                    ${rateHtml}
                                </div>
                            </div>
                        </div>
                        <div class="content">
                            ${item.content}
                        </div>
                    </li>
                    `;
                });
            } else {
                html = "<p class='mt-4'>Chưa có đánh giá</p>";
            }

            $("#review-list").html(html);

            $("#review-avg-rate").html(data.averageRate);

            $("#rating-heading").html(generateStarRateHtml(data.averageRate));
        },
        error: function () {

        }
    })
}

$(document).ready(function () {
    loadReview();
});