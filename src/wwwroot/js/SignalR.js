// room id
var roomId = $("#hidden_roomId")[0].value;
console.log(roomId)
// if (roomId == null) {
//     console.log("This room not exists");
//     return;
// }

var connection = new signalR.HubConnectionBuilder().withUrl(`/chat?chatId=${roomId}`).build();

document.getElementById("sendMessage").disabled = true;

connection.on("ReceiveMessage", function (user, message) {
    var li = document.createElement("li");
    li.textContent = `${user} says ${message}`;
    document.getElementById("ulteste").appendChild(li);
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
    var user = "fulano"
    var message = document.getElementById("message").value;
    connection.invoke("SendMesageToGroup", user, message, roomId)
    .catch(function (err) {
        return console.error(err.toString());
    });

    event.preventDefault();
});
