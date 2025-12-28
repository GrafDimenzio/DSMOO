const input = document.getElementById("commandInput");
const responseDiv = document.getElementById("commandResponse");

input.addEventListener("keydown", async function (event) {
    if (event.key !== "Enter") {
        return;
    }
    event.preventDefault();
    const text = input.value.trim();
    if (text === "") return;
    const response = await sendConsoleCommand(text);
    const resultDiv = document.createElement("div");
    resultDiv.classList.add("command-result");
    resultDiv.textContent = "> " + response.Message;
    responseDiv.appendChild(resultDiv);
    responseDiv.scrollTop = responseDiv.scrollHeight;
    input.value = "";
})

async function sendConsoleCommand(message) {
    const response = await fetch("/api/console", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            command: message
        })
    });

    if (!response.ok) {
        throw new Error(`HTTP Fehler: ${response.status}`);
    }

    return await response.json();
}

async function logCommand(message) {
    try {
        const result = await sendConsoleCommand(message);
        console.log(result.Message, result.ResultType);
    } catch (err) {
        console.error(err);
    }
}