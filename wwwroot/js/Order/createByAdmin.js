var resultSearch = $(".result-search");
var listGroupResult = $(".result-search .list-group");
var selectedProductList = $("#product-data");

$("#search-input").blur(function () {
    if (!listGroupResult.is(":hover"))
        resultSearch.hide();
});

$("#search-input").focus(function () {
    if (listGroupResult.html() != "")
        resultSearch.show();
})

const soldOut = () => {
    alert("Sản phẩm đã hết hàng");
}

$("#search-input").on("input", function () {

    const searchValue = $(this).val();

    if (searchValue == "" || searchValue.length < 3) {
        resultSearch.hide();
        return;
    }

    const dataQuery = {
        name: searchValue
    }

    $.ajax({
        type: 'get',
        url: '/ProductsManager/SearchProduct',
        data: dataQuery,
        success: function (data) {
            listGroupResult.html("");
            let html = ``;
            data.forEach(e => {
                e.colors.forEach(c => {
                    c.capacities.forEach(cap => {
                        if (cap.quantity > 0) {

                            html += `
                            <li class="list-group-item" role="button" onclick="selectProduct(${e.id}, ${c.id}, ${cap.id})">${e.name} - ${c.name} - ${cap.ram}GB/${cap.rom}GB - ${cap.sellPrice.toLocaleString()}<sup>đ</sup></li>
                        `;
                        }
                        else {
                            html += `
                                        <li class="list-group-item" role="button" onclick="soldOut()">${e.name} - ${c.name} - ${cap.ram}GB/${cap.rom}GB - ${cap.sellPrice.toLocaleString()}<sup>đ</sup></li>
                                    `;
                        }
                    })
                })
            })

            listGroupResult.append(html);
            resultSearch.show();
        }
    })

})

const checkSelectedProduct = function (productId, colorId, capaId) {
    var selectedProduct = document.getElementById(`${productId}-${colorId}-${capaId}`);
    if (selectedProduct) {
        const inputElement = $(selectedProduct).find(".product-quantity");
        if (Number(inputElement.val()) < Number(inputElement.attr("max"))) {
            const qtt = Number(inputElement.val()) + 1;
            $(selectedProduct).find(".product-quantity").val(qtt);
        }
        return true;
    }
    return false;
}

const selectProduct = function (productId, colorId, capaId) {
    resultSearch.hide();

    if (checkSelectedProduct(productId, colorId, capaId))
        return;

    const dataQuery = {
        productId: productId,
        colorId: colorId,
        capaId: capaId
    }

    $.ajax({
        type: 'get',
        url: '/ProductsManager/GetProduct',
        data: dataQuery,
        success: function (data) {
            if (!data)
                alert("có lỗi");

            let html = `
                        <li class="list-group-item d-flex align-items-center position-relative product-selected-item" id="${data.product.id}-${data.color.id}-${data.capacity.id}" data-price="${data.capacity.sellPrice}">
                            <img src="/files/Products/${data.color.image}" style="width:100px">
                            <div class="ms-4">
                                <span class="fw-semibold">${data.product.name}</span> - ${data.color.name} -
                                <span>${data.capacity.ram}GB/${data.capacity.rom}GB</span> - 
                                <span class="">${data.capacity.sellPrice.toLocaleString()}đ</span>
                            </div>
                            <span class="ms-4">Số lượng</span>
                            <input class="form-control form-control-sm ms-2 product-quantity" style="width:60px" type="number" min="1" max="${data.capacity.quantity}" value="1" onchange="updateInfo()">
                            <span class="btn btn-sm btn-danger position-absolute top-0 end-0 me-2 mt-2" onclick="deleteSelectedProduct('${data.product.id}-${data.color.id}-${data.capacity.id}')">
                                <i class="fa-solid fa-xmark"></i>
                            </span>
                        </li>
                    `;

            $("#product-data").append(html);
        }
    }).done(function () {
        updateInfo();
    });

}

const deleteSelectedProduct = function (id) {
    $(`#${id}`).remove();
    updateInfo();
}

const updateInfo = function () {
    let totalProduct = 0;
    let totalPrice = 0;

    selectedProductList.find(".product-selected-item").each((i, e) => {
        const productCount = Number($(e).find(".product-quantity").val());
        totalProduct += productCount;
        totalPrice += Number($(e).data("price")) * productCount;
    });

    $("#product-count").html(totalProduct);
    $("#total-price").html(`${totalPrice.toLocaleString()}<sup>đ</sup>`);

}

const checkInput = function () {
    let alertMessage = "";
    const productList = $("#product-data .product-selected-item").get(0);
    if (!productList)
        alertMessage += "Chưa có sản phẩm \n";

    const customerName = $("#customer-name").val();
    const customerAddress = $("#customer-address").val();
    const city = $("#City").val();
    const district = $("#District").val();
    const commune = $("#Commune").val();
    const customerPhone = $("#customer-phone").val();
    const customerEmail = $("#customer-email").val();

    if (customerName == "" || customerAddress == "" || city == "" || district == "" || commune == "" || customerPhone == "" || customerEmail == "")
        alertMessage += "Phải nhập tất cả thông tin";

    if (alertMessage == "") {
        const data = {
            name: customerName,
            address: customerAddress,
            city: city,
            district: district,
            commune: commune,
            phone: customerPhone,
            email: customerEmail
        }
        return data;
    }
    else {
        alert(alertMessage);
        return false;
    }

}

const createOrder = function () {
    const data = checkInput();
    if (!data) return;

    let products = [];

    $(".product-selected-item").each((i, e) => {
        const Ids = $(e).attr("id").split("-");
        const quantity = $(e).find(".product-quantity").val();
        const sellPrice = $(e).data("price");
        products.push({
            productId: Number(Ids[0]),
            colorId: Number(Ids[1]),
            capaId: Number(Ids[2]),
            quantity: Number(quantity)
        });
    });

    const buyType = $("#in-store").is(":checked") ? "store" : "ship";
    const paid = $("#paid").is(":checked");

    data["products"] = products;
    data["buyType"] = buyType;
    data["paid"] = paid;

    $.ajax({
        type: 'post',
        url: '/OrderManager/Create',
        data: JSON.stringify(data),
        contentType: 'application/json',
        processData: false,
        cache: false,
        success: function (result) {
            if (result.status == 1) {
                window.location.href = `/OrderManager/Details/${result.id}`;
            }
            else {
                alert(result.message);
            }
        },
        error: function (data) {
            if (data != null) {
                alert(data.message);
            }
            else {
                alert("Có lỗi");
            }
        }
    })

};


$("#in-ship").on("input", function () {
    if ($(this).is(":checked")) {
        $(".pay-status").show();
    }
});


$("#in-store").on("change", function () {
    if ($(this).is(":checked")) {
        $(".pay-status").hide();
    }
});