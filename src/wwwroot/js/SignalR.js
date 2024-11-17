import { messageError, chatMessage } from "./messages.js";
// room id
var roomId = $("#hidden_roomId")[0].value;

var connection = new signalR.HubConnectionBuilder().withUrl(`/chat?chatId=${roomId}`).build();
document.getElementById("sendMessage").disabled = true;

connection.on("ReceiveMessage", function (userId, message, roomId) {
    // Create element to display message
    chatMessage(userId, message, roomId);
});

connection.on("ReceiveError", function (errorName, errorMessage) {
    if (errorName == "error-onnection") {
        var roomId = $("#hidden_roomId")[0].value;
        roomId == undefined
            ? window.location = "/rooms"
            : window.location.reload();
        // Create element to display error message
        messageError(errorName, errorMessage)
    }
});

connection.start()
    .then(function () {
        connection.invoke("AddToGroup", roomId)
            .catch(function (err) {
                return console.log(err.toString());
            });
        document.getElementById("sendMessage").disabled = false;
    })
    .catch(function (err) {
        return console.error(err.toString());
    });

document.getElementById("sendMessage").addEventListener("click", function (event) {
    var userId = parseInt($("#hidden_userId")[0].value);
    var message = document.getElementById("message").value;
    connection.invoke("SendMesageToGroup", userId, message, roomId)
    .catch(function (err) {
        return console.error(err.toString());
    });

    event.preventDefault();
});
