export function chatMessageError(errorName, errorMessage) {
    var li = document.createElement("li");
    li.textContent = `${errorName}: ${errorMessage}`;
    document.getElementById("ul-messages").appendChild(li);
}
