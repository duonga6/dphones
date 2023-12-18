var btnSendMessage = $("#btn-send-message");
var inputSendMessage = $("#message-content-send");
var messageList = $("#message-list");
var alertMustLogin = $("#alert-must-login");

var chatWindow = $(".chat-content");
var notiMessage = $("#noti-new-message");


$(".chat-btn").click(function (event) {
    event.stopPropagation();
    chatWindow.toggleClass("active");

    if (chatWindow.hasClass("active")) {
        SeenMessage();
    }
});

$("#close-chat-btn").click(function () {
    chatWindow.toggleClass("active");
});

$(window).click(function () {
    chatWindow.removeClass("active");
});

chatWindow.click(function (event) {
    event.stopPropagation();
});


const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .build();

connection.start().then(function () {
    btnSendMessage.prop("disabled", false);
    alertMustLogin.hide();
    LoadMessage();
}).catch(function (err) {
    btnSendMessage.prop("disabled", true);
    alertMustLogin.show();
    return console.error("Bạn chưa đăng nhập");
});

connection.on("ClientSendMessageSuccess", function (message) {
    LoadMessage();
});

connection.on("NewAdminMessage", function (message) {
    LoadMessage();
});

btnSendMessage.click(function (event) {
    event.preventDefault();
    if (!inputSendMessage.val())
        return;

    connection.invoke("ClientSendToAdmin", inputSendMessage.val()).then(function () {
        inputSendMessage.val("");
        console.log("send message");
    }).catch(function (err) {
        return console.error(err.toString());
    });
});

const AddMessage = function (message, flag = true) {
    const sender = message.fromAdmin ? "admin" : "user";
    const avatar = sender == "admin" ? "/images/logo-color.png" : $("#user-avatar").val();

    const date = new Date(message.createdAt);
    let day = date.getDate().toString();
    day = day.length > 1 ? day : '0' + day;
    let month = (date.getMonth() + 1).toString();
    month = month.length > 1 ? month : '0' + month;
    let year = date.getFullYear();
    let hour = date.getHours().toString();
    hour = hour.length > 1 ? hour : '0' + hour;
    let minute = date.getMinutes().toString();
    minute = minute.length > 1 ? minute : '0' + minute;

    const fulldate = `${day}/${month}/${year} ${hour}:${minute}`;

    const html = `
    <div class="message-item ${sender}">
        <div class="message-info" title="${fulldate}">
            <div class="message-avatar">
                <img src="${avatar}" ${!flag ? "style='opacity: 0;'" : ""}>
            </div>
            <div class="message-content">
                <span>${message.content}</span>
            </div>
        </div>
        <div class="message-date text-secondary text-center">
            ${!flag ? "" : fulldate}
        </div>
    </div>
    `;

    messageList.append(html);
    messageList.scrollTop(messageList.prop("scrollHeight"));
}

const LoadMessage = function () {
    $.ajax({
        type: "get",
        url: "/client-get-message",
        success: function (data) {
            if (data.length > 0) {
                messageList.html("");

                let flag = false;

                data.forEach((item, index, array) => {

                    if (index == array.length - 1)
                        flag = true;
                    else if (item.senderId != array[index + 1].senderId) {
                        flag = true;
                    }
                    else {
                        const date1 = new Date(item.createdAt);
                        const date2 = new Date(array[index + 1].createdAt);

                        if ((date2 - date1) / 60000 <= 5) {
                            flag = false;
                        }
                        else {
                            flag = true;
                        }

                    }

                    AddMessage(item, flag);
                });

                if (!data[data.length - 1].seen && data[data.length - 1].senderId != userId && !chatWindow.hasClass("active")) {
                    notiMessage.show();
                }
            }


        }
    });
}

const SeenMessage = function () {
    $.ajax({
        type: "get",
        url: "/client-seen-message",
        success: function (response) {
            notiMessage.hide();
        }
    });
}
