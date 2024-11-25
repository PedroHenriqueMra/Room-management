export function downScreen() {
    const ulElement = $("#ul-messages")[0];
    const bottomElement = ulElement.scrollHeight;
    document.body.scrollTop = bottomElement;
}
