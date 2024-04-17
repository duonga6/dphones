import { showToast } from "/js/Helper/toast.js";

$("#btn-export-excel").click(() => {
    $.ajax({
        type: "get",
        url: "/generate-product-report",
        contentType: false,
        processData: false,
        cache: false,
        success: function (data) {
            if (data.success) {
                var link = document.createElement("a");
                link.href = data.data;
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
            } else {
                showToast.show(0, data.message);
            }
        }
    });
}) 