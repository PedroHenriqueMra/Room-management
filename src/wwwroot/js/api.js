import { chatMessageError } from "./components/messageError.js";

export async function sendMessage(userInfos, message, roomId) {
    // userInfos = {"UserId", "UserEmail", "UserName"}
    userInfos = JSON.parse(userInfos);
    console.log("user id: " + userInfos.UserId);
    console.log("user email: " + userInfos.UserEmail);
    console.log("user name: " + userInfos.UserName);
    console.log("message: " + message);
    console.log("room id: " + roomId);
    var statusCode = await fetchCreateMessage(userInfos.UserId, message, roomId);
    console.log("StatusCode: " + statusCode);
    if (statusCode === 200) {
        chatMessageError("Error", message);
        // chatMessage(userInfos, message);
    }
}

// receive user email, user name and message
function fetchCreateMessage(userId, message, roomId) {
    const url = `http://localhost:5229/rooms/${roomId}`;
    const body = { userId, message, roomId }
    return fetch(url, {
        method: "POST",
        body: JSON.stringify(body),
        headers: {
            "RequestVerificationToken": $("input[name='__RequestVerificationToken']").val(),
            "Content-Type": "application/json"
        }
    })
    .then(response => {
        console.log(response);
        if (!response.ok) {
            return response.json()
            .then(err => {
                err["status"] = response.status;
                console.log(JSON.stringify(err));
                throw err;
            });
        }

        return 200;
    })
    .catch(err => {
        if (err.status == 401) {
            console.log("redirected (401)");
            window.location.href = "http://localhost:5229/auth/login";
            return 401;
        }
        console.error(err);
        chatMessageError("Error", err);

        return 400;
    })
}
