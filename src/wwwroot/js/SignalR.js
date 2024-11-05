var connection = new signalR.HubConnectionBuilder().withUrl("/chat").build();

document.getElementById("button").disabled = true;

connection.on("ReceiveMessage", function (user, message) {
    var li = document.createElement("li");
    document.getElementById("ulteste").appendChild(li);
    li.textContent = `${user} says ${message}`;
});

connection.start().then(function () {
    document.getElementById("button").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("button").addEventListener("click", function (event) {
    var user = "fulano"
    var message = document.getElementById("inputUser").value;
    connection.invoke("SendMessage", user, message).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});
