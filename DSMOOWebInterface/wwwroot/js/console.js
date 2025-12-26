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