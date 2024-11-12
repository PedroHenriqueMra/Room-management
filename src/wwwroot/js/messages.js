export function messageError(errorName, errorMessage) {
    var li = document.createElement("li");
    li.textContent = `Error name: ${errorName}<br>${errorMessage}`;
    document.getElementById("ul-messages").appendChild(li);
}

export function chatMessage(userName, message) {
    var li = document.createElement("li");
    li.textContent = `${userName} says ${message}`;
    document.getElementById("ul-messages").appendChild(li);
}
