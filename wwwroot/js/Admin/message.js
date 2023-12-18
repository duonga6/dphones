var userList = $(".user-list");
var messageList = $(".message-list");
var messageListName = $(".message-header .user-name");
var messageListImg = $(".message-header .user-img img");
var messageSection = $(".message-section");
var btnSendMessage = $("#btn-send-message");
var inputMessage = $("#send-message-content");

const app = {
    currentUserId: null,
    currentUserAvatar: null,
    currentUserName: null,

    handleEvent: function () {
        const _this = this;
        $(document).on("click", ".user-item", function () {
            _this.ChooseUser(this);
        });

        btnSendMessage.click(function (event) {
            event.preventDefault();

            if (!inputMessage.val())
                return;

            _this.SendMessage()
        });
    },

    AddUserItem: function (model, isNew = false) {
        const html = `
        <div class="user-item ${isNew ? "new-message" : ""} ${!model.seen && !model.isLastMessageFromAdmin ? "new-message" : ""}" data-id="${model.userId}">
            <div class="user-avatar">
                <img class="user-avatar-img" src="${model.userAvatarUrl}" alt="">
            </div>
            <div class="user-info">
                <p class="user-name">
                    ${model.userName}
                </p>
                <div class="user-last-message">
                    ${model.lastMessage}
                </div>
            </div>
        </div>
        `;

        if (isNew) {
            userList.prepend(html);
        }
        else {
            userList.append(html);
        }
    },

    AddMessageItem: function (model, flag = true) {
        const date = new Date(model.createdAt);
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

        const sender = model.fromAdmin ? "admin" : "user";

        const avatar = sender == "admin" ? "/images/logo-color.png" : this.currentUserAvatar;

        const html = `
        <div class="message-item ${sender}">
            <div class="message-info">
                <div class="user-avatar">
                    <img src="${avatar}" alt="" ${!flag ? "style='opacity: 0;'" : ""}>
                </div>
                <div class="message-content">
                    <span>${model.content}</span>
                </div>
            </div>
            <div class="message-date text-secondary">
                ${!flag ? "" : fulldate}
            </div>
        </div>
        `;

        messageList.append(html);
        messageList.scrollTop(messageList.prop("scrollHeight"));
    },

    LoadUser: function () {
        const _this = this;
        $.ajax({
            type: "get",
            url: "/admin-get-user-list",
            success: function (data) {
                data.forEach(item => {
                    _this.AddUserItem(item);
                });
            },
            error: function () {
                console.error("Lỗi lấy list user");
            }
        });
    },

    LoadMessage: function (userId) {
        const _this = this;
        $.ajax({
            type: "get",
            url: `/admin-get-user-message/${userId}`,
            success: function (data) {
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

                    _this.AddMessageItem(item, flag);
                });

                $(`.user-item[data-id="${userId}"] .user-last-message`).html(data[data.length - 1].content);
            }
        });
    },

    ChooseUser: function (e) {
        messageSection.show();

        const _this = this;
        const element = $(e);

        this.currentUserId = element.data("id");
        this.currentUserAvatar = element.find(".user-avatar-img").attr("src");
        this.currentUserName = element.find(".user-name").text().trim();


        $(".user-item").removeClass("active");
        element.addClass("active");

        if (element.hasClass("new-message")) {
            this.SeenMessage(this.currentUserId);
        }



        messageListImg.attr("src", this.currentUserAvatar);
        messageListName.text(this.currentUserName);

        this.LoadMessage(this.currentUserId);
    },

    SeenMessage: function (userId) {
        $.ajax({
            type: "get",
            url: `/admin-seen-message/${userId}`,
            success: function () {
                $(`.user-item[data-id="${userId}"]`).removeClass("new-message");
            },
            error: function () {
                console.log("Error remove class");
            }
        });
    },

    TriggerNewMessage: function (message) {
        const _this = this;

        const userElement = $(`.user-item[data-id="${message.senderId}"]`);

        if (userElement.get(0)) {

            if (message.senderId == this.currentUserId) {
                this.LoadMessage(message.senderId)
                this.SeenMessage(message.senderId);
            }
            else {
                userElement.addClass("new-message");
            }
            userElement.find(".user-last-message").html(message.content);
        } else {
            $.ajax({
                type: "get",
                url: `/admin-get-user/${message.senderId}`,
                success: function (data) {
                    if (data) {
                        _this.AddUserItem(data, true);
                    }
                    else {
                        console.log("Lỗi lấy thông tin user khi có message mới");
                    }
                }
            });
        }
    },

    SendMessage: function () {
        const _this = this;
        connection.invoke("AdminSendToClient", this.currentUserId, inputMessage.val())
            .then(function () {
                inputMessage.val("");
                _this.LoadMessage(_this.currentUserId);
            }).catch(function (err) {
                return console.error(err.toString());
            })
    },

    Start: function () {
        this.handleEvent();
        this.LoadUser();
    }
}

app.Start();