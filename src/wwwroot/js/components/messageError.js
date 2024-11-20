// <li>
//      <div class="message-error">
//          <span class="label-error"></span>
//          <p class="text-error">...</p>
//      </div>
// </li>
// 

export function chatMessageError(errorName, errorMessage) {
    var li = document.createElement("li");
    var div = document.createElement("div");
    var span = document.createElement("span");
    var p = document.createElement("p");

    // set classes
    div.setAttribute("class", "message-error");
    span.setAttribute("class", "label-error");
    p.setAttribute("class", "text-error");

    // set content:
    span.textContent = errorName;
    p.textContent = errorMessage;

    // appendChilds:
    div.appendChild(span);
    div.appendChild(p);
    li.appendChild(div);

    // append li in ul:
    var ulTarget = $("#ul-messages")[0];
    ulTarget.appendChild(li);
}
