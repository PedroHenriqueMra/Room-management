export function chatMessage(userId, message, roomId) {
    fetchCreateMessage(userId, message, roomId);
    
    // var li = document.createElement("li");
    // li.textContent = `${userName} says ${message}`;
    // document.getElementById("ul-messages").appendChild(li);
}

function fetchCreateMessage(userId, message, roomId) {
    const url = `http://localhost:5229/rooms/${roomId}`;
    const body = {
        userId, message, roomId
    }
    fetch(url, {
        method: "POST",
        body: JSON.stringify(body),
        headers: {
            "RequestVerificationToken": $("input[name='__RequestVerificationToken']").val(),
            "Content-Type": "application/json"
        }
    }).then(res => {
        console.log(res);
        console.log(`Status: ${res.StatusCode}, Message: ${res.Message}`);
    })
}

export function messageError(errorName, errorMessage) {
    var li = document.createElement("li");
    li.textContent = `Error name: ${errorName}<br>${errorMessage}`;
    document.getElementById("ul-messages").appendChild(li);
}
