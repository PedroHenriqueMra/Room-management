import { chatMessageError } from "./components/messageError.js";
import { sendMessage } from "./api.js";

// room id
var roomId = $("#hidden_roomId")[0].value;

var connection = new signalR.HubConnectionBuilder().withUrl(`/chat?chatId=${roomId}`).build();
document.getElementById("sendMessage").disabled = true;

// userInfos = {"userName", "userEmail", "userId"}
connection.on("ReceiveMessage", async function (userInfos, message, roomId) {
    // Create element to display message
    await sendMessage(userInfos, message, roomId);
});

connection.on("ReceiveError", function (errorName, errorMessage) {
    if (errorName == "error-connection") {
        var roomId = $("#hidden_roomId")[0].value;
        if (roomId == null) {
            window.location = "/rooms";
        }
        // Create element to display error message
        chatMessageError(errorName, errorMessage)
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

var buttonSendMessage = document.getElementById("sendMessage");
buttonSendMessage.addEventListener("click", function (event) {
    event.preventDefault();
    
    var userId = parseInt($("#hidden_userId")[0].value);
    var message = document.getElementById("message").value;
    document.getElementById("message").value = "";

    connection.invoke("SendMesageToGroup", userId, message, roomId)
    .catch(function (err) {
        return console.error(err.toString());
    });
});
