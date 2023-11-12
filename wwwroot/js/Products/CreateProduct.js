var img = document.getElementById("result-img");
var cropper = undefined;
var modal = $("#modal-edit-img");

// Chỉnh sửa ảnh sản phẩm
const editSubImg = function (element) {
    const src = $(element).siblings(".sub-img").attr("src");
    const id = $(element).closest(".sub-img-item").attr("data-id");
    OpenEdit(element, 'sub', id, src, true);
}

// Chỉnh sửa ảnh Color
const editColorImg = function (element) {
    const src = $(element).siblings(".color-img-show").attr("src");
    const id = $(element).closest(".color-group").attr("data-index");
    OpenEdit(element, 'color', id, src, true);
}

// Mở modal edit ảnh
const OpenEdit = function (e, type, id = null, src = null, isEdit = false) {
    modal.addClass("active");

    if (type == 'sub') {
        modal.attr('data-id', id);
        modal.attr('data-type', 'sub');
        if (isEdit) modal.attr('data-status', 'edit');
    } else if (type == 'color') {
        modal.attr('data-id', id);
        modal.attr('data-type', 'color');
        if (isEdit) modal.attr('data-status', 'edit');
    } else {
        alert("Có lỗi xảy ra, hãy refresh trang (OpenEdit)");
        return;
    }
    if (src) {
        img.src = src;
    } else {
        img.src = URL.createObjectURL(e.files[0]);
    }

    if (cropper) cropper.destroy();
    cropper = new Cropper(img);
    if (type == 'color') {
        cropper.setAspectRatio(1 / 1);
    }
}

// Lưu ảnh được cắt
const SaveEditedFile = function () {
    if (cropper) {
        const fileName = img.getAttribute('src').split('/').pop();
        cropper.getCroppedCanvas().toBlob(function (blob) {
            const url = URL.createObjectURL(blob);
            const link = document.createElement('a');
            link.href = url;
            link.download = fileName + '.png';
            link.click();
        }, 'image/png');
    }
}

// Hủy modal cắt ảnh
const CancelEdit = function (e) {
    const modalType = modal.attr("data-type");
    const isEdit = modal.attr("data-status") == 'edit';
    if (modalType == 'sub' && !isEdit) {
        const id = Number(modal.attr('data-id'));
        $(".sub-input").each((index, element) => {
            if (Number($(element).closest(".sub-img-item").attr("data-id")) == id) {
                $(element).val("");
            }
        });
    } else if (modalType == 'color' && !isEdit) {
        const id = Number(modal.attr('data-id'));
        $(".color-input-img").each((index, element) => {
            if (Number($(element).closest(".color-group").attr("data-index")) == id) {
                $(element).val("");
            }
        })
    }

    if (cropper) {
        img.style.display = 'none';
        cropper.clear();
        cropper.destroy();
    }
    modal.removeClass("active");
}

// Áp dụng ảnh cắt
const ApplyEdit = function () {
    if (cropper) {
        const isEdit = modal.attr("data-status") == 'edit';
        const modalType = modal.attr("data-type");
        const fileName = img.getAttribute('src').split('/').pop();
        cropper.getCroppedCanvas().toBlob(function (blob) {

            img.src = URL.createObjectURL(blob);
            const file = new File([blob], fileName + '.png');
            const fileList = new DataTransfer();
            fileList.items.add(file);


            if (modalType == 'sub') {
                const id = $("#modal-edit-img").attr("data-id");
                if (!id && id != 0) {
                    alert("Có lỗi thiếu Id");
                    return;
                }

                $(".sub-img-item").each((index, e) => {
                    const item = $(e);
                    const itemId = item.attr("data-id");
                    if (itemId == id) {
                        item.children(".sub-img-show").show();
                        item.children(".btn-choose-sub-img").hide();
                        item.children(".sub-img-show").children(".sub-img").attr("src", URL.createObjectURL(blob));
                        const fileInput = item.children(".sub-input").get(0);
                        fileInput.files = fileList.files;
                    }
                });
                if (!isEdit) addSubImgSection();
            } else if (modalType == 'color') {
                const id = $("#modal-edit-img").attr("data-id");
                if (!id && id != 0) {
                    alert("Có lỗi thiếu Id");
                    return;
                }

                $(".color-group").each((index, element) => {
                    const item = $(element);
                    const itemId = item.attr("data-index");
                    if (itemId == id) {
                        item.find(".color-img-show-container").show();
                        item.find(".btn-choose-img").hide();
                        item.find(".color-img-show").attr("src", URL.createObjectURL(blob));

                        const fileInput = item.find(".color-input-img").get(0);
                        fileInput.files = fileList.files;
                    }
                });
            }

        }, 'image/png');

        img.style.display = 'none';
        cropper.clear();
        cropper.destroy();
        modal.removeClass("active");

    } else {
        alert("Có lỗi xảy ra, vui lòng refresh trang");
    }


}


// Xử lý up ảnh cho sản phẩm

// Chọn ảnh
const changeSubImg = function (element) {
    const id = Number($(element).closest(".sub-img-item").attr("data-id"));
    OpenEdit(element, 'sub', id);

}

// xóa ảnh
const removeSubImg = function (element) {

    if ($(".sub-img-item").length > 1) {
        $(element).closest(".sub-img-item").remove();
        return;
    }

    const imgShowContainer = $(element).closest(".sub-img-show");
    const btnChooseImg = imgShowContainer.siblings(".btn-choose-sub-img");
    const imgShow = $(element).siblings(".sub-img");
    const inputImg = imgShowContainer.siblings(".sub-input");

    btnChooseImg.show();
    imgShow.attr("src", "");
    inputImg.val("");
    imgShowContainer.hide();
}

// Thêm input cho ảnh tiếp theo
const addSubImgSection = function () {
    const Id = Number($(".sub-img-item").last().attr("data-id")) + 1;
    const html = `
                    <div class="sub-img-item col-2 d-flex justify-content-center align-items-center" data-id="${Id}">
                        <label for="SubImage_${Id}__FileUpload" class="btn btn-choose-sub-img">
                            <img src="/images/upload_icon.png" alt="">
                        </label>
                        <input type="hidden" name="SubImage.Index" value="${Id}">
                        <input hidden class="sub-input" oninput="changeSubImg(this, ${Id})" type="file" id="SubImage_${Id}__FileUpload" name="SubImage[${Id}].FileUpload">
                        <div class="sub-img-show position-relative" style="display: none;">
                            <img class="sub-img w-100" style="object-fit: contain; height: 300px;">
                            <span class="btn btn-primary btn-sm position-absolute top-0 end-0 mt-1 me-4 translate-middle-x"
                                onclick="editSubImg(this)">
                                <i class="text-white fa-regular fa-pen-to-square"></i>
                            </span>
                            <span class="btn btn-danger btn-sm position-absolute top-0 end-0 mt-1 me-1" onclick="removeSubImg(this)">
                                <i class="fa-solid fa-xmark"></i>
                            </span>
                        </div>
                    </div>
                `;
    $(".sub-img-container").append(html);
}


// Thêm ram rom sl cho màu
const addOption = function (element) {
    const optionIndex = ($(element).closest(".option-group").children(".option-item").last().data("id")) + 1;
    const colorIndex = ($(element).closest(".color-group").data("index"));
    const html = `
                <div class="row option-item position-relative" data-id="${optionIndex}">
                    <div class="col-md-2">
                        <input type="hidden" name="ProductColor[${colorIndex}].Capacities.Index" value="${optionIndex}"> 
                        <label class="form-label" for="ProductColor_${colorIndex}__Capacities_${optionIndex}__Ram">Ram</label>
                        <input class="form-control" type="number" data-val="true" data-val-required="The Ram field is required." id="ProductColor_${colorIndex}__Capacities_${optionIndex}__Ram" name="ProductColor[${colorIndex}].Capacities[${optionIndex}].Ram" value=""><input name="__Invariant" type="hidden" value="ProductColor[${colorIndex}].Capacities[${optionIndex}].Ram">
                        <span class="text-danger field-validation-valid" data-valmsg-for="ProductColor[${colorIndex}].Capacities[${optionIndex}].Ram" data-valmsg-replace="true"></span>
                    </div>
                    <div class="col-md-2">
                        <label class="form-label" for="ProductColor_${colorIndex}__Capacities_${optionIndex}__Rom">Rom</label>
                        <input class="form-control" type="number" data-val="true" data-val-required="The Rom field is required." id="ProductColor_${colorIndex}__Capacities_${optionIndex}__Rom" name="ProductColor[${colorIndex}].Capacities[${optionIndex}].Rom" value=""><input name="__Invariant" type="hidden" value="ProductColor[${colorIndex}].Capacities[${optionIndex}].Rom">
                        <span class="text-danger field-validation-valid" data-valmsg-for="ProductColor[${colorIndex}].Capacities[${optionIndex}].Rom" data-valmsg-replace="true"></span>
                    </div>
                    <div class="col-md-2">
                        <label class="form-label" for="ProductColor_${colorIndex}__Capacities_${optionIndex}__Quantity">SL</label>
                        <input class="form-control" type="number" data-val="true" data-val-required="The Số lượng field is required." id="ProductColor_${colorIndex}__Capacities_${optionIndex}__Quantity" name="ProductColor[${colorIndex}].Capacities[${optionIndex}].Quantity" value=""><input name="__Invariant" type="hidden" value="ProductColor[${colorIndex}].Capacities[${optionIndex}].Quantity">
                        <span class="text-danger field-validation-valid" data-valmsg-for="ProductColor[${colorIndex}].Capacities[${optionIndex}].Quantity" data-valmsg-replace="true"></span>
                    </div>
                    <div class="col-md-3">
                        <label class="form-label" for="ProductColor_${colorIndex}__Capacities_${optionIndex}__EntryPrice">Giá nhập vào</label>
                        <input class="form-control" type="text" data-val="true" data-val-number="The field Giá nhập vào must be a number." data-val-required="Giá nhập vào không được trống" id="ProductColor_${colorIndex}__Capacities_${optionIndex}__EntryPrice" name="ProductColor[${colorIndex}].Capacities[${optionIndex}].EntryPrice" value="">
                        <span class="text-danger field-validation-valid" data-valmsg-for="ProductColor[${colorIndex}].Capacities[${optionIndex}].EntryPrice" data-valmsg-replace="true"></span>
                    </div>
                    <div class="col-md-3">
                        <label class="form-label" for="ProductColor_${colorIndex}__Capacities_${optionIndex}__SellPrice">Giá bán ra</label>
                        <input class="form-control" type="text" data-val="true" data-val-number="The field Giá bán ra must be a number." data-val-required="Giá bán ra không được trống" id="ProductColor_${colorIndex}__Capacities_${optionIndex}__SellPrice" name="ProductColor[${colorIndex}].Capacities[${optionIndex}].SellPrice" value="">
                        <span class="text-danger field-validation-valid" data-valmsg-for="ProductColor[${colorIndex}].Capacities[${optionIndex}].SellPrice" data-valmsg-replace="true"></span>
                    </div>
                    <div class="position-absolute top-0 end-0">
                        <span class="btn btn-sm position-absolute top-0 end-0 me-2 text-danger" id="delete-option" onclick="deleteOption(this)">
                            <i class="fa-solid fa-minus"></i>
                        </span>
                    </div>
                </div>
            `;
    $(element).closest(".option-group").append(html);
}

const deleteOption = function (element) {
    $(element).closest(".option-item").remove();
}

const deleteColor = function (element) {
    $(element).closest(".color-group").remove();
}

$(document).ready(function () {
    // Thêm màu
    $("#add-color").click(function () {
        const colorIndex = $(this).closest("#color-container").children(".color-group").last().data("index") + 1;
        const html = `
                                        <div class="card mt-2 color-group" data-index="${colorIndex}">
                                            <div class="card-header position-relative">
                                                <div class="row">
                                                    <div class="col-lg-4">
                                                        <input type="hidden" name="ProductColor.Index" value="${colorIndex}">
                                                        <div class="row">
                                                            <div class="col-md-6">
                                                                <label class="form-label" for="ProductColor_${colorIndex}__Name">Tên màu</label>
                                                                <input required class="form-control" type="text" data-val="true" data-val-required="Tên màu không được trống" id="ProductColor_${colorIndex}__Name" name="ProductColor[${colorIndex}].Name" value="">
                                                                <span class="text-danger field-validation-valid" data-valmsg-for="ProductColor[${colorIndex}].Name" data-valmsg-replace="true"></span>
                                                            </div>
                                                            <div class="col-md-6">
                                                                <label class="form-label" for="ProductColor_${colorIndex}__Code">Mã màu</label>
                                                                <input class="form-control form-control-color" type="color" data-val="true" data-val-required="Mã màu không được trống" id="ProductColor_${colorIndex}__Code" name="ProductColor[${colorIndex}].Code" value="">
                                                                <span class="text-danger field-validation-valid" data-valmsg-for="ProductColor[${colorIndex}].Code" data-valmsg-replace="true"></span>
                                                            </div>
                                                        </div>
                                                        <div class="color-image mt-2">
                                                            <label class="btn btn-choose-img" for="ProductColor_${colorIndex}__ImageFile">
                                                                <img src="/images/upload_icon.png" alt="">
                                                            </label>
                                                            <input class="color-input-img" hidden="" accept=".png, .jpg, .jpeg, .webp" oninput="changeColorImg(this)" type="file" data-val="true" data-val-fileextensions="The ImageFile field only accepts files with the following extensions: .png, .jpg, .jpeg, .webp" data-val-fileextensions-extensions=".png,.jpg,.jpeg,.webp" id="ProductColor_${colorIndex}__ImageFile" name="ProductColor[${colorIndex}].ImageFile">
                                                            <div class="color-img-show-container position-relative" style="display: none;">
                                                                <img class="color-img-show mt-2" src="" style="object-fit: contain;">
                                                                <span class="btn btn-primary btn-sm position-absolute top-0 end-0 mt-1 me-4 translate-middle-x"
                                                                    onclick="editColorImg(this)">
                                                                    <i class="text-white fa-regular fa-pen-to-square"></i>
                                                                </span>
                                                                <span class="btn btn-danger btn-sm position-absolute top-0 end-0 mt-1 me-1" onclick="removeColorImg(this)">
                                                                    <i class="fa-solid fa-xmark"></i>
                                                                </span>
                                                            </div>
                                                        </div>
                                                    </div>
                                                    <div class="col-lg-8">
                                                        <div class="option-group">
                                                            <div>
                                                                Options
                                                                <span class="btn btn-primary btn-sm ms-2" id="add-options" onclick="addOption(this)">
                                                                    <i class="fa-solid fa-plus"></i>
                                                                </span>
                                                            </div>
                                                            <div class="row option-item" data-id="0">
                                                                <div class="col-md-2">
                                                                    <input type="hidden" name="ProductColor[${colorIndex}].Capacities.Index" value="0">
                                                                    <label class="form-label" for="ProductColor_${colorIndex}__Capacities_0__Ram">Ram</label>
                                                                    <input class="form-control" type="number" data-val="true" data-val-required="The Ram field is required." id="ProductColor_${colorIndex}__Capacities_0__Ram" name="ProductColor[${colorIndex}].Capacities[0].Ram" required><input name="__Invariant" type="hidden" value="ProductColor[${colorIndex}].Capacities[0].Ram">
                                                                    <span class="text-danger field-validation-valid" data-valmsg-for="ProductColor[${colorIndex}].Capacities[0].Ram" data-valmsg-replace="true"></span>
                                                                </div>
                                                                <div class="col-md-2">
                                                                    <label class="form-label" for="ProductColor_${colorIndex}__Capacities_0__Rom">Rom</label>
                                                                    <input class="form-control" type="number" data-val="true" data-val-required="The Rom field is required." id="ProductColor_${colorIndex}__Capacities_0__Rom" name="ProductColor[${colorIndex}].Capacities[0].Rom" required><input name="__Invariant" type="hidden" value="ProductColor[${colorIndex}].Capacities[0].Rom">
                                                                    <span class="text-danger field-validation-valid" data-valmsg-for="ProductColor[${colorIndex}].Capacities[0].Rom" data-valmsg-replace="true"></span>
                                                                </div>
                                                                <div class="col-md-2">
                                                                    <label class="form-label" for="ProductColor_${colorIndex}__Capacities_0__Quantity">Số lượng</label>
                                                                    <input class="form-control" type="number" data-val="true" data-val-required="The Số lượng field is required." id="ProductColor_${colorIndex}__Capacities_0__Quantity" name="ProductColor[${colorIndex}].Capacities[0].Quantity" required><input name="__Invariant" type="hidden" value="ProductColor[${colorIndex}].Capacities[0].Quantity">
                                                                    <span class="text-danger field-validation-valid" data-valmsg-for="ProductColor[${colorIndex}].Capacities[0].Quantity" data-valmsg-replace="true"></span>
                                                                </div>
                                                                <div class="col-md-3">
                                                                    <label class="form-label" for="ProductColor_${colorIndex}__Capacities_0__EntryPrice">Giá nhập vào</label>
                                                                    <input class="form-control valid" type="text" data-val="true" data-val-number="The field Giá nhập vào must be a number." data-val-required="Giá nhập vào không được trống" id="ProductColor_${colorIndex}__Capacities_0__EntryPrice" name="ProductColor[${colorIndex}].Capacities[0].EntryPrice" value="" aria-describedby="ProductColor_${colorIndex}__Capacities_0__EntryPrice-error" aria-invalid="false">
                                                                    <span class="text-danger field-validation-valid" data-valmsg-for="ProductColor[${colorIndex}].Capacities[0].EntryPrice" data-valmsg-replace="true"></span>
                                                                </div>
                                                                <div class="col-md-3">
                                                                    <label class="form-label" for="ProductColor_${colorIndex}__Capacities_0__SellPrice">Giá bán ra</label>
                                                                    <input class="form-control valid" type="text" data-val="true" data-val-number="The field Giá bán ra must be a number." data-val-required="Giá bán ra không được trống" id="ProductColor_${colorIndex}__Capacities_0__SellPrice" name="ProductColor[${colorIndex}].Capacities[0].SellPrice" value="" aria-describedby="ProductColor_${colorIndex}__Capacities_0__SellPrice-error" aria-invalid="false">
                                                                    <span class="text-danger field-validation-valid" data-valmsg-for="ProductColor[${colorIndex}].Capacities[0].SellPrice" data-valmsg-replace="true"></span>
                                                                </div>
                                                            </div>
                                                        </div>
                                                    </div>
                                                </div>
                                                <span class="btn btn-danger btn-sm position-absolute top-0 end-0 me-1 mt-1" onclick="deleteColor(this)">
                                                    <i class="fa-solid fa-xmark"></i>
                                                </span>
                                            </div>
                                        </div>
                                    `;

        $("#color-container").append(html);
    });
})

// Xử lý ảnh upload cho color

// Chọn ảnh
const changeColorImg = function (element) {
    const id = Number($(element).closest(".color-group").attr('data-index'));
    OpenEdit(element, 'color', id)
}

// Xóa ảnh
const removeColorImg = function (element) {
    const btnChooseImg = $(element).closest(".color-img-show-container").siblings(".btn-choose-img");
    const inputImg = $(element).closest(".color-img-show-container").siblings(".color-input-img");
    const imgShowContainer = $(element).closest(".color-img-show-container");
    const imgShow = imgShowContainer.children(".color-img-show");

    btnChooseImg.show();
    imgShow.attr("src", "");
    inputImg.val("");
    imgShowContainer.hide();
}
