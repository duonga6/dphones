const SendReply = function (event) {
    event.preventDefault();

    const email = $("#user-email").val();
    const replyContent = $("#content-reply").val();
    const subject = $("#subject-email").val();
    const type = "One";

    const dataObj = {
        Subject: subject,
        Content: replyContent,
        Type: type,
        Receiver: email
    };

    var resultShow = $("#send-mail-result");

    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/send-mail-api",
        data: JSON.stringify(dataObj),
        success: function (response) {
            resultShow.append(`
                <div class="alert alert-success" role="alert">
                    Đã gửi phản hồi
                </div>
            `);

            setTimeout(() => [
                resultShow.html('')
            ], 5000);

            $("#content-reply").val('');

        },
        error: function (response) {
            resultShow.append(`
                <div class="alert alert-danger" role="alert">
                    Gửi phản hồi thất bại
                </div>
            `);

            setTimeout(() => [
                resultShow.html('')
            ], 5000);
        },
    });

}