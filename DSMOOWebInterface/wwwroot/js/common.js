function getWebSocketUrl(path)
{
    const protocol = window.location.protocol === "https:" ? "wss://" : "ws://";
    const host = window.location.host;
    return `${protocol}${host}${path}`;
}