var cropper = undefined;
var modelEdit = $("#modal-edit-img");
var img = document.getElementById("result-img");
var previewCtn = $("#preview-contaner");

const ChooseImg = function (e) {
    OpenEdit(e);
}

const OpenEdit = function (e, isEdit = false) {
    modelEdit.addClass("active");

    img.src = URL.createObjectURL(e.files[0]);

    if (cropper) cropper.destroy();

    cropper = new Cropper(img);
    cropper.setAspectRatio(3 / 2);
}

const SaveEditedFile = function () {
    if (cropper) {
        const fileName = img.getAttribute("src").split('/').pop();
        cropper.getCroppedCanvas().toBlob(function (blob) {
            const url = URL.createObjectURL(blob);
            const link = document.createElement('a');
            link.href = url;
            link.download = fileName + '.png';
            link.click();
        }, 'image/png');
    }
}

const CancelEdit = function () {
    if (cropper) {
        img.style.display = 'none';
        cropper.clear();
        cropper.destroy();
    }
    modelEdit.removeClass("active");
}

const ApplyEdit = function () {
    if (cropper) {
        const fileName = img.getAttribute('src').split('/').pop();
        cropper.getCroppedCanvas().toBlob(function (blob) {

            img.src = URL.createObjectURL(blob);
            const file = new File([blob], fileName + '.png');
            const fileList = new DataTransfer();
            fileList.items.add(file);
            $("#ImageFile").get(0).files = fileList.files;

            $("#img-preview").attr("src", URL.createObjectURL(blob));

            previewCtn.show();

            img.style.display = 'none';
            cropper.clear();
            cropper.destroy();
            modelEdit.removeClass("active");
        });
    }
}

const RemoveImg = function () {
    $("#ImageFile").val("");
    $("#Image").val("");
    $("#img-preview").attr("src", "");
    previewCtn.hide();
} 