const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .build();

connection.start().then(function () {

}).catch(function (err) {
    return console.error("Bạn chưa đăng nhập");
});

connection.on("NewClientMessage", function (message) {
    app.TriggerNewMessage(message);
});