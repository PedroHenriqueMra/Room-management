// <li>
//       <div class="message-box if(message.id == user.id) {"user-message"}">
//              <span class="who">
//                  name user
//              </span>
//              <p class="content">
//                  content
//              </p>
//       </div>
// </li>

// userInfos = {"UserId", "UserEmail", "UserName"}
import { downScreen } from "../functions/downScreen.js";

export function chatMessage(userInfos, message) {
    var li = document.createElement("li");
    var div = document.createElement("div");
    var span = document.createElement("span");
    var p = document.createElement("p");

    // user current id:
    var currentId = $("#hidden_userId")[0].value;
    currentId = parseInt(currentId);

    span.setAttribute("class", "who");
    span.textContent = userInfos.UserId == currentId ? "Your message" : `Menssagem de ${userInfos.UserName}`;

    p.setAttribute("class", "content");
    p.textContent = message;

    var classDiv = "message-box" + (userInfos.UserId == currentId ? " user-message" : "");
    div.setAttribute("class", classDiv);
    div.appendChild(span);
    div.appendChild(p);

    li.appendChild(div);
    var ulTarget = $("#ul-messages")[0];
    ulTarget.appendChild(li);

    downScreen();
}
