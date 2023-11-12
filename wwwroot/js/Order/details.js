$("#generate-bill-btn").click(function() {
    $.ajax( {
        type: "get",
        url: '/OrderManager/GenerateBill/' + $("#order-id").val(),
        contentType: false,
        processData: false,
        cache: false,
        success: function(data) {
            if (data.status == 0)
            {
                alert(`Có lỗi: ${data.message}`);
            }
            else
            {
                window.open(data.message, '_blank');
            }
        },
        error: function() {{
            alert(`Có lỗi !`);
        }}
    })
});