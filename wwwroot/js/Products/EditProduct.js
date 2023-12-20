var img = document.getElementById("result-img");
var cropper = undefined;
var modal = $("#modal-edit-img");

const btnAddMainPhoto = $("#btn-add-main-photo");
const showMainPhoto = $(".show-main-photo");
const mainImg = $("#main-photo-img");
const inputMainPhoto = $("#PrimaryImage_FileUpload");
const inputIdMainPhoto = $("#input-id-main-photo");

const showSubPhoto = $(".show-sub-photo");
const btnAddSubPhoto = $("#btn-add-sub-photo");
const btnChooseSubPhoto = $("#btn-choose-sub-photo");
const subPhotoContainer = $(".show-sub-photo");
// Xóa hoặc hiển thị ảnh chính
const ActiveMain = function (flag = true, src = null) {
    if (flag) {
        mainImg.attr("src", src);
        btnAddMainPhoto.hide();
        showMainPhoto.show();
    } else {
        btnAddMainPhoto.show();
        showMainPhoto.hide();
        inputMainPhoto.val("");
    }
}

// HÌNH ẢNH CHÍNH

// Chỉnh sửa ảnh chính
const editMainImg = function (element) {
    const src = mainImg.attr("src");
    OpenEdit(inputMainPhoto.get(0), 'main', null, src, true);
}

// Xóa ảnh chính
const deleteMainPhoto = function (id = null) {
    ActiveMain(false);
    inputIdMainPhoto.removeAttr('disabled');
    inputIdMainPhoto.val("0");
}

// Upload ảnh phụ
const changeMainPhoto = function (element) {
    OpenEdit(element, 'main');
}


// HÌNH ẢNH PHỤ

// Chỉnh sửa ảnh phụ
const editSubImg = function (element) {
    const src = $(element).siblings(".sub-photo-img").attr("src");
    const id = $(element).closest(".sub-photo-item").attr("data-id");
    const inputElement = $(element).closest("input[type=file]").get(0);
    OpenEdit(inputElement, 'sub', id, src, true);
}

// Xóa ảnh phụ
const deleteSubPhoto = function (element, id) {
    $(element).siblings("input[type=file]").val("");
    $(element).siblings(".input-id-sub-photo").removeAttr("disabled");
    $(element).closest(".sub-photo-item").hide();
}

// Uplaod ảnh phụ
const changeSubPhoto = function (element) {
    const id = $(element).closest(".sub-photo-item").attr("data-id");
    OpenEdit(element, 'sub', id);
}

// Thêm input cho ảnh phụ sau khi thêm ảnh.
const addSubPhotoItem = function () {
    const index = Number(subPhotoContainer.children(".sub-photo-item").last().attr("data-id")) + 1;
    subItemHtml = `
        <li class="sub-photo-item col-md-4 mb-2 position-relative" data-id="${index}" style="display: none;">
            <input type="hidden" name="SubImage.Index" value="${index}">
            <input type="file" id="SubImage_${index}__FileUpload" name="SubImage[${index}].FileUpload" oninput="changeSubPhoto(this)" hidden>
            <img src="" class="sub-photo-img">
            <span class="btn btn-primary btn-sm position-absolute top-0 end-0 mt-1 me-4 translate-middle-x"
                onclick="editSubImg(this)">
                <i class="text-white fa-regular fa-pen-to-square"></i>
            </span>
            <span class="btn btn-danger btn-sm position-absolute top-0 end-0 mt-1 me-1"
                onclick="deleteSubPhoto(this)">
                <i class="fa-solid fa-xmark"></i>
            </span>
        </li>
    `;

    subPhotoContainer.append(subItemHtml);

    return index;
}

// Modal handle
const OpenEdit = function (e, type = 'main', id = null, src = null, isEdit = false) {
    modal.addClass("active");
    if (type == 'main') {
        if (isEdit) modal.attr('data-status', 'edit');
        modal.attr('data-type', 'main');
    } else if (type == 'sub') {
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
            if (Number($(element).closest(".color-group").attr("data-id")) == id) {
                $(element).val("");
            }
        })
    } else if (modalType == 'main' && !isEdit) {
        inputMainPhoto.val("");
    }

    if (cropper) {
        img.style.display = 'none';
        cropper.clear();
        cropper.destroy();
    }
    modal.removeClass("active");
}

const ApplyEdit = function () {
    if (cropper) {
        const isEdit = modal.attr("data-status") == 'edit';
        const modalType = modal.attr("data-type");
        const fileName = img.getAttribute('src').split('/').pop() + '.png';
        cropper.getCroppedCanvas().toBlob(function (blob) {
            const srcImgEdited = URL.createObjectURL(blob);
            img.src = srcImgEdited;
            const file = new File([blob], fileName);
            const fileList = new DataTransfer();
            fileList.items.add(file);


            if (modalType == 'main') {

                ActiveMain(true, srcImgEdited);
                inputMainPhoto.get(0).files = fileList.files;
                inputIdMainPhoto.removeAttr('disabled');

            } else if (modalType == 'sub') {
                const id = $("#modal-edit-img").attr("data-id");
                if (!id && id != 0) {
                    alert("Có lỗi thiếu Id");
                    return;
                }

                $(".sub-photo-item").each((index, e) => {
                    const item = $(e);
                    const itemId = item.attr("data-id");
                    if (itemId == id) {
                        item.show();
                        item.children(".sub-photo-img").attr("src", URL.createObjectURL(blob));
                        const fileInput = item.children(`#SubImage_${id}__FileUpload`).get(0);
                        fileInput.files = fileList.files;
                        $(e).children(".input-id-sub-photo").removeAttr("disabled");

                    }
                });

                if (!isEdit) {
                    const nextId = addSubPhotoItem();
                    btnChooseSubPhoto.attr("for", `SubImage_${nextId}__FileUpload`);
                }

            } else if (modalType == 'color') {
                const id = $("#modal-edit-img").attr("data-id");
                if (!id && id != 0) {
                    alert("Có lỗi thiếu Id");
                    return;
                }

                $(".color-group").each((index, element) => {
                    const item = $(element);
                    const itemId = item.attr("data-id");
                    if (itemId == id) {
                        item.find(".color-img-show-container").show();
                        item.find(".btn-choose-img").hide();
                        item.find(".color-img-show").attr("src", URL.createObjectURL(blob));

                        const fileInput = item.find(".color-input-img").get(0);
                        fileInput.files = fileList.files;
                        item.find(".input-image-name-color").attr("value", fileName);
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

// Xử lý Color
// - Thêm ảnh cho color
const changeColorImg = function (element) {
    const id = $(element).closest(".color-group").attr('data-id');
    OpenEdit(element, 'color', id);
}
// - Xóa ảnh color
const removeColorImg = function (element) {
    const showImgContainer = $(element).closest(".color-img-show-container");
    const btnChooseImg = showImgContainer.siblings(".btn-choose-img");
    const inputImg = showImgContainer.siblings("input[type='file']");

    btnChooseImg.show();
    showImgContainer.hide();
    inputImg.val("");

    $(element).closest(".color-image").children(".input-image-name-color").val("");
}

const editColorImg = function (element) {
    const id = $(element).closest(".color-group").attr("data-id");
    const inputElement = $(element).closest(".color-image").children(".color-input-img").get(0);
    const src = $(element).siblings(".color-img-show").attr("src");
    OpenEdit(inputElement, 'color', id, src, true);
}

// -Thêm option
const addOption = function (element) {
    const nextIndex = $(element).closest(".option-group").children(".option-item").last().data("id") + 1;
    const colorIndex = $(element).data("id");
    const optionHtml = `
        <div class="row option-item position-relative" data-id="${nextIndex}">
            <input type="hidden" name="ProductColor[${colorIndex}].Capacities.Index" value="${nextIndex}">
            <input type="hidden" name="ProductColor[${colorIndex}].Capacities[${nextIndex}].Id" value="0">
            <div class="col-md-2">
                <label for="ProductColor[${colorIndex}].Capacities[${nextIndex}].Ram"
                    class="form-label">Ram</label>
                <input required id="ProductColor[${colorIndex}].Capacities[${nextIndex}].Ram"
                    name="ProductColor[${colorIndex}].Capacities[${nextIndex}].Ram"
                    class="form-control">
            </div>
            <div class="col-md-2">
                <label for="ProductColor[${colorIndex}].Capacities[${nextIndex}].Rom"
                    class="form-label">Rom</label>
                <input required id="ProductColor[${colorIndex}].Capacities[${nextIndex}].Rom"
                    name="ProductColor[${colorIndex}].Capacities[${nextIndex}].Rom"
                    class="form-control">
            </div>
            <div class="col-md-2">
                <label for="ProductColor[${colorIndex}].Capacities[${nextIndex}].Quantity"
                    class="form-label">Số lượng</label>
                <input required id="ProductColor[${colorIndex}].Capacities[${nextIndex}].Quantity"
                    name="ProductColor[${colorIndex}].Capacities[${nextIndex}].Quantity"
                    class="form-control" min="1">
            </div>
            <div class="col-md-3">
                <label for="ProductColor[${colorIndex}].Capacities[${nextIndex}].EntryPrice"
                    class="form-label">Giá nhập vào</label>
                <input required id="ProductColor[${colorIndex}].Capacities[${nextIndex}].EntryPrice"
                    name="ProductColor[${colorIndex}].Capacities[${nextIndex}].EntryPrice"
                    class="form-control" min="1">
            </div>
            <div class="col-md-3">
                <label for="ProductColor[${colorIndex}].Capacities[${nextIndex}].SellPrice"
                    class="form-label">Giá bán ra</label>
                <input required id="ProductColor[${colorIndex}].Capacities[${nextIndex}].SellPrice"
                    name="ProductColor[${colorIndex}].Capacities[${nextIndex}].SellPrice"
                    class="form-control">
            </div>
            <div class="position-absolute top-0 end-0 d-inline-block">
                <span class="btn text-danger position-absolute top-0 end-0 me-2" id="delete-option" onclick="deleteOption(this)" data-id="${nextIndex}">
                    <i class="fa-solid fa-minus"></i>
                </span>
            </div>
        </div>
    `;
    $(element).closest(".option-group").append(optionHtml);
}

// -Xóa option
const deleteOption = function (element) {
    $(element).closest(".option-item").remove();
}

// -Thêm color
const addColor = function (element) {
    let nextIndex = $("#color-container .color-group").last().data("id") + 1;
    if (isNaN(nextIndex)) nextIndex = 0;
    const colorHtml = `
        <div class="card color-group mb-3" data-id="${nextIndex}">
            <div class="card-header position-relative">
                <span class="btn btn-danger btn-sm position-absolute top-0 end-0 me-1 mt-1" onclick="deleteColor(this,${nextIndex})">
                    <i class="fa-solid fa-xmark"></i>
                </span>
                <div class="row">
                    <div class="col-md-4">
                        <div class="row">
                            <input type="hidden" name="ProductColor.Index" value="${nextIndex}">
                            <input type="hidden" name="ProductColor[${nextIndex}].Id" value="0">
                            <div class="col-md-6">
                                <label for="ProductColor[${nextIndex}].Name" class="form-label">Tên màu</label>
                                <input id="ProductColor[${nextIndex}].Name" name="ProductColor[${nextIndex}].Name"
                                    value="" class="form-control">
                            </div>
                            <div class="col-md-6">
                                <label for="ProductColor[${nextIndex}].Code" class="form-label">Mã màu</label>
                                <input id="ProductColor[${nextIndex}].Code" name="ProductColor[${nextIndex}].Code"
                                    value="" class="form-control form-control-color"
                                    type="color">
                            </div>
                        </div>
                        <div class="color-image mt-2">
                            <input type="hidden" name="ProductColor[${nextIndex}].Image" class="input-image-name-color" value="">
                            <label for="ProductColor[${nextIndex}].ImageFile"
                                class="btn btn-success btn-sm btn-choose-img"
                                >Thêm ảnh</label>
                            <input type="file" id="ProductColor[${nextIndex}].ImageFile"
                                name="ProductColor[${nextIndex}].ImageFile" class="color-input-img" hidden
                                accept=".png, .jpg, .jpeg, .webp" oninput="changeColorImg(this)">
                            <div class="color-img-show-container position-relative"
                                style="display: none;"
                                >
                                <img class="color-img-show mt-2"  
                                    style="object-fit: contain;">
                                <span
                                    class="btn btn-danger btn-sm position-absolute top-0 end-0 mt-1 me-1"
                                    onclick="removeColorImg(this)">
                                    <i class="fa-solid fa-xmark"></i>
                                </span>
                            </div>
                        </div>
                    </div>

                    <div class="col-md-8">
                        <div class="option-group">
                            <div>
                                Options
                                <span class="btn btn-primary btn-sm ms-2" id="add-options"
                                    onclick="addOption(this)" data-id="${nextIndex}">
                                    <i class="fa-solid fa-plus"></i>
                                </span>
                            </div>
                            <div class="row option-item" data-id="0">
                                <input type="hidden" name="ProductColor[${nextIndex}].Capacities.Index" value="0">
                                <input type="hidden" name="ProductColor[${nextIndex}].Capacities[0].CapacitiesId" >
                                <div class="col-md-2">
                                    <label for="ProductColor[${nextIndex}].Capacities[0].Ram"
                                        class="form-label">Ram</label>
                                    <input required id="ProductColor[${nextIndex}].Capacities[0].Ram"
                                        name="ProductColor[${nextIndex}].Capacities[0].Ram"
                                        value="" class="form-control">
                                </div>
                                <div class="col-md-2">
                                    <label for="ProductColor[${nextIndex}].Capacities[0].Rom"
                                        class="form-label">Rom</label>
                                    <input required id="ProductColor[${nextIndex}].Capacities[0].Rom"
                                        name="ProductColor[${nextIndex}].Capacities[0].Rom"
                                        value="" class="form-control">
                                </div>
                                <div class="col-md-2">
                                    <label for="ProductColor${nextIndex}].Capacities[0].Quantity"
                                        class="form-label">Số lượng</label>
                                    <input required id="ProductColor[${nextIndex}].Capacities[0].Quantity"
                                        name="ProductColor[${nextIndex}].Capacities[0].Quantity"
                                        value="" class="form-control" min="1">
                                </div>
                                <div class="col-md-2">
                                    <label for="ProductColor[${nextIndex}].Capacities[0].EntryPrice"
                                        class="form-label">Giá nhập vào</label>
                                    <input required id="ProductColor[${nextIndex}].Capacities[0].EntryPrice"
                                        name="ProductColor[${nextIndex}].Capacities[0].EntryPrice"
                                        value="" class="form-control" min="1">
                                </div>
                                <div class="col-md-2">
                                    <label for="ProductColor[${nextIndex}].Capacities[0].SellPrice"
                                        class="form-label">Giá bán ra</label>
                                    <input required id="ProductColor[${nextIndex}].Capacities[0].SellPrice"
                                        name="ProductColor[${nextIndex}].Capacities[0].SellPrice"
                                        value="" class="form-control">
                                </div>
                                <div class="col-md-2 mb-1 mt-auto">

                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    `;
    $("#color-container").append(colorHtml);
}

// -Xóa color
const deleteColor = function (element, id) {
    $("#color-container .color-group").each((index, elem) => {
        if ($(elem).data("id") == id)
            $(elem).remove();
    })
}