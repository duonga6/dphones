const cartList = $("#cart-list");

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
    if (qtt == 0) {
        cartElement.css("display", "none");
        $(".empty-cart").show();
    }
    else {
        cartElement.css("display", "flex");
        cartElement.html(qtt);
        $(".empty-cart").hide();
    }
}

const loadCart = function () {
    $.ajax({
        type: "get",
        url: '/cart/get-cart',
        contentType: false,
        processData: false,
        cache: false,
        success: function (data) {
            let html = ``;
            Array.from(JSON.parse(data)).forEach(item => {
                const rom = Number(item.Capacity.Rom) % 1024 == 0 ? item.Capacity.Rom / 1024 + "TB" : item.Capacity.Rom + "GB";
                let percentDiscount = 0;
                let moneyDiscount = 0;

                if (item.Product.ProductDiscounts.length > 0) {
                    item.Product.ProductDiscounts.forEach(discount => {
                        percentDiscount += discount.Discount.PercentDiscount;
                        moneyDiscount += discount.Discount.MoneyDiscount
                    });
                }

                const originPrice = item.Capacity.SellPrice;
                const sellPrice = originPrice * (100 - percentDiscount) / 100 - moneyDiscount;

                html += `
                            <li class="cart-item mb-2" data-id="${item.Id}">
                                <div class="cart-item-container position-relative">
                                    <div class="row h-100">
                                        <div class="col-11">
                                            <div class="cart-item-info">
                                                <div class="row h-100">
                                                    <div class="col-5 col-md-4">
                                                        <div class="row h-100">
                                                            <div class="cart-item-img col">
                                                                <img src="/files/Products/${item.Color.Image}" alt="">
                                                            </div>
                                                        </div>
                                                    </div>
                                                    <div class="col-7 col-md-8">
                                                        <div class="row h-100 center-section align-items-md-center">
                                                            <div class="col-md-5 d-flex flex-column">
                                                                <div class="product-info">
                                                                    <div class="product-name d-flex align-items-center pe-3">
                                                                        <span class="">${item.Product.Name}</span>
                                                                    </div>
                                                                    <div class="product-options">
                                                                        <div class="color">
                                                                            <span>Màu sắc: ${item.Color.Name}</span>
                                                                        </div>
                                                                        <div class="capaticy">
                                                                            <span class="ram">${item.Capacity.Ram + "GB"}</span>
                                                                            <span class="rom">${rom}</span>
                                                                        </div>
                                                                        <div class="price">
                                                                            <div class="fw-semibold"><span class="product-sell-price">${sellPrice.toLocaleString("vi-VN")}</span><sup>đ</sup></div>
                                                                            <div class="" style="font-size: 14px"><del>${sellPrice != originPrice ? item.Capacity.SellPrice.toLocaleString("vi-VN") + "<sup>đ</sup>" : ""}</del></div>
                                                                        </div>
                                                                    </div>
                                                                </div>
                                                            </div>
                                                            <div class="col-md-3">
                                                                <div class="cart-item-quantity">
                                                                    <div class="d-flex align-items-center">
                                                                        <span class="btn-quantity btn-add-quantity"
                                                                            onclick="minusQuantity(this,'${item.Id}')">
                                                                            <i style="font-size: 14px;"
                                                                                class="fa-solid fa-minus"></i>
                                                                        </span>
                                                                        <span class="fw-semibold mx-1 product-quantity">${item.Quantity}</span>
                                                                        <span class="btn-quantity btn-add-minus"
                                                                            onclick="plusQuantity(this,'${item.Id}')">
                                                                            <i style="font-size: 14px;"
                                                                                class="fa-solid fa-plus"></i>
                                                                        </span>
                                                                    </div>
                                                                </div>
                                                            </div>
                                                            <div class="col-md-3 total-cost-container">
                                                                <div class="total-cost h-100">
                                                                    <span class="total-price fw-semibold primary-text"><span
                                                                            class="total-product-money">${(sellPrice * item.Quantity).toLocaleString("vi-VN")}</span>
                                                                        <sup>đ</sup></span>
                                                                </div>
                                                            </div>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="col-1">
                                            <div class="h-100 d-flex align-items-center justify-content-center">
                                                <input type="checkbox" name="cartId" value="${item.Id}"
                                                    class="checkbox-choose-cart-item" onchange="updateTotalMoney()">
                                            </div>
                                        </div>
                                    </div>
                                    <span class="position-absolute top-0 end-0 me-2 p-2 btn-delete-cart-item"
                                        onclick="deleteCartItem('${item.Id}')">
                                        <i class="primary-text fa-solid fa-xmark"></i>
                                    </span>
                                </div>
                            </li>
                        `;
            });
            cartList.html("");
            cartList.append(html);
        }
    })
}

const deleteCartItem = function (id) {
    const formData = new FormData();
    formData.append("Id", id);

    $.ajax({
        type: 'delete',
        url: '/cart/delete-item',
        data: formData,
        processData: false,
        contentType: false,
        cache: false,
        success: function (data) {
            showToast(data.status, data.message);
            if (data.qtt == 0 || !data.qtt) updateCartQuantity(0);
            cartList.find(`[data-id='${id}']`).remove();
        },
        error: function () {
            showToast(0, "Có lỗi khi xóa!");
        }
    }).done(function () {
        updateTotalMoney();
    });

}

const updateProductMoney = function () {
    const allCheckbox = $(".cart-item-container");
    allCheckbox.each((i, e) => {
        const price = Number($(e).find(".product-sell-price").html().replace(/[.]+/g, ""));
        const quantity = Number($(e).find(".product-quantity").html());

        console.log(price + " " + quantity);

        $(e).closest(".cart-item-container").find(".total-product-money").html((price * quantity).toLocaleString("vi-VN"));
    });
}

const updateTotalMoney = function () {
    const allCheckbox = $(".checkbox-choose-cart-item");
    let totalCost = 0;
    let totalProduct = 0;
    allCheckbox.each((i, e) => {
        if ($(e).prop("checked")) {
            totalCost += Number($(e).closest(".cart-item-container").find(".total-product-money").html().replace(/[.]+/g, ""));
            totalProduct += Number($(e).closest(".cart-item-container").find(".product-quantity").html());
        }
    });

    $("#product-choosed-count").html(totalProduct);
    $("#total-money").html(totalCost.toLocaleString("vi-VN"));
}

const plusQuantity = function (e, id) {
    var formData = new FormData();
    formData.append("Id", id);

    $.ajax({
        url: '/cart/plus',
        data: formData,
        type: 'put',
        processData: false,
        contentType: false,
        cache: false,
        success: function (data) {
            showToast(data.status, data.message);
            $(e).siblings(".product-quantity").html(Number(data.qtt));
        },
        error: function () {
            showToast(data.status, data.message);
        }
    }).done(function () {
        updateProductMoney();
        updateTotalMoney();
    })
}

const minusQuantity = function (e, id) {
    var formData = new FormData();
    formData.append("Id", id);

    $.ajax({
        url: '/cart/minus',
        data: formData,
        type: 'put',
        processData: false,
        contentType: false,
        cache: false,
        success: function (data) {
            showToast(data.status, data.message);
            $(e).siblings(".product-quantity").html(Number(data.qtt));
        },
        error: function (data) {
            showToast(data.status, data.message);
        }
    }).done(function () {
        updateProductMoney();
        updateTotalMoney();
    });
}

const checkChooseProduct = function (event) {
    const checkBox = $(".checkbox-choose-cart-item").filter(":checked");
    if (checkBox.length == 0) {
        showToast(0, "Bạn chưa chọn sản phẩm nào")
        event.preventDefault();
    }
}

$(document).ready(function () {
    loadCart();
});